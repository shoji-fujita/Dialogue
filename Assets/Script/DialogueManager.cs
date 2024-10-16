using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

[System.Serializable]
public class Dialogue
{
    public int id;
    public string text;
    public List<Choice> choices;
}

[System.Serializable]
public class Choice
{
    public string text;
    public int nextDialogueId;
}

[System.Serializable]
public class DialogueList
{
    public List<Dialogue> dialogues;
}

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;
    public TextMeshProUGUI dialogueText;
    public GameObject choiceButtonPrefab;
    public Transform choiceButtonParent;
    public Button nextButton;
    public GameObject dialogueWindow;   // 会話ウィンドウを制御するためのGameObject
    public Button skipButton;           // ARSceneに進むボタン
    public Button closeButton;          // CharacterSelectSceneに戻るボタン
    public float typingSpeed = 0.05f;
    private DialogueList dialogueList;
    private Dialogue currentDialogue;
    private int currentDialogueId = 1;
    private bool isTyping = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        LoadDialoguesFromJson();
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonPressed);  // skipButtonにリスナー追加
        closeButton.onClick.AddListener(OnCloseButtonPressed);  // closeButtonにリスナー追加
        ShowDialogue(currentDialogueId);
    }

    void LoadDialoguesFromJson()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "dialogues.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            dialogueList = JsonUtility.FromJson<DialogueList>(json);
        }
        else
        {
            Debug.LogError("JSONファイルが見つかりません: " + filePath);
        }
    }

    void OnNextButtonClicked()
    {
        if (!isTyping && (currentDialogue == null || currentDialogue.choices == null || currentDialogue.choices.Count == 0))
        {
            ShowDialogue(++currentDialogueId);
        }
    }

    public void ShowDialogue(int dialogueId)
    {
        Dialogue dialogue = dialogueList.dialogues.Find(d => d.id == dialogueId);

        if (dialogue != null)
        {
            currentDialogue = dialogue;
            ClearChoices();
            choiceButtonParent.gameObject.SetActive(false);

            StartCoroutine(TypeSentence(dialogue.text, dialogue));
        }
        else
        {
            Debug.LogError("会話が見つかりません: ID = " + dialogueId);
        }
    }

    IEnumerator TypeSentence(string dialogue, Dialogue currentDialogue)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in dialogue.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        if (currentDialogue.choices != null && currentDialogue.choices.Count > 0)
        {
            ShowChoices(currentDialogue.choices);
        }
        else
        {
            choiceButtonParent.gameObject.SetActive(false);
        }
    }

    void ShowChoices(List<Choice> choices)
    {
        if (choices != null && choices.Count > 0)
        {
            choiceButtonParent.gameObject.SetActive(true);
            foreach (Choice choice in choices)
            {
                CreateChoiceButton(choice);
            }
        }
        else
        {
            choiceButtonParent.gameObject.SetActive(false);
        }
    }

    void CreateChoiceButton(Choice choice)
    {
        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceButtonParent);
        TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = choice.text;

        Button button = choiceButton.GetComponent<Button>();
        button.onClick.AddListener(() => OnChoiceSelected(choice.nextDialogueId));
    }

    void OnChoiceSelected(int nextDialogueId)
    {
        ClearChoices();
        nextButton.interactable = true;
        ShowDialogue(nextDialogueId);
    }

    void ClearChoices()
    {
        foreach (Transform child in choiceButtonParent)
        {
            Destroy(child.gameObject);
        }
    }

    // ARシーンに移動して、会話ウィンドウを非表示にする
    public void OnSkipButtonPressed()
    {
        SceneManager.LoadScene("ARScene");
        dialogueWindow.SetActive(false);  // 会話ウィンドウを非表示
        skipButton.gameObject.SetActive(false);  // skipButtonを非表示
    }

    // CharacterSelectSceneに戻る
    public void OnCloseButtonPressed()
    {
        SceneManager.LoadScene("CharacterSelectScene");
        dialogueWindow.SetActive(false);
        Destroy(gameObject);
    }
}
