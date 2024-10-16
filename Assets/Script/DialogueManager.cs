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
    public string type;  // Optional type to control actions
    public List<Choice> choices;
    public float seconds;  // Optional: 時間で自動進行するためのフィールド
}

[System.Serializable]
public class Choice
{
    public string text;
    public int nextDialogueId;
    public string type;
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
    private int nextDialogueIdAfterAR = -1;  // AR後に再開するためのID
    private Coroutine autoNextCoroutine;     // 自動で次に進むためのコルーチン

    void Awake()
    {
        // シングルトンパターンでDialogueManagerのインスタンスを管理
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
        // 初期化: JSONを読み込み、ボタンにリスナーを追加
        LoadDialoguesFromJson();
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonPressed);  // skipButtonにリスナー追加
        closeButton.onClick.AddListener(OnCloseButtonPressed);  // closeButtonにリスナー追加
        ShowDialogue(currentDialogueId);  // 最初の会話を表示
    }

    // JSONファイルから会話データをロードする
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

    // 次の会話へ進むボタンが押された時の処理
    void OnNextButtonClicked()
    {
        if (!isTyping && (currentDialogue == null || currentDialogue.choices == null || currentDialogue.choices.Count == 0))
        {
            ShowDialogue(++currentDialogueId);
        }
    }

    // 特定の会話IDに基づいて会話を表示するメソッド
    public void ShowDialogue(int dialogueId)
    {
        if (autoNextCoroutine != null)
        {
            StopCoroutine(autoNextCoroutine);  // 自動進行をキャンセル
        }

        Dialogue dialogue = dialogueList.dialogues.Find(d => d.id == dialogueId);

        if (dialogue != null)
        {
            currentDialogue = dialogue;
            currentDialogueId = dialogueId;  // 現在の会話IDを更新
            ClearChoices();
            choiceButtonParent.gameObject.SetActive(false);

            StartCoroutine(TypeSentence(dialogue.text, dialogue));

            // 自動で次の会話に進むための秒数が設定されている場合
            if (dialogue.seconds > 0 && (dialogue.choices == null || dialogue.choices.Count == 0))
            {
                autoNextCoroutine = StartCoroutine(AutoNextDialogue(dialogue.seconds, dialogue));
            }
        }
        else
        {
            Debug.LogError("会話が見つかりません: ID = " + dialogueId);
        }
    }

    // 一定時間後に自動的に次の会話に進む。endの場合は終了処理を実行
    IEnumerator AutoNextDialogue(float seconds, Dialogue dialogue)
    {
        yield return new WaitForSeconds(seconds);

        if (!string.IsNullOrEmpty(dialogue.type) && dialogue.type == "end")
        {
            OnCloseButtonPressed();  // 終了処理
        }
        else
        {
            ShowDialogue(currentDialogueId + 1);  // 次の会話に進む
        }
    }

    // 会話を一文字ずつ表示するためのコルーチン
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

        // 選択肢の表示
        if (currentDialogue.choices != null && currentDialogue.choices.Count > 0)
        {
            ShowChoices(currentDialogue.choices);
        }
        else
        {
            choiceButtonParent.gameObject.SetActive(false);
        }

        // 特定のtypeがある場合はその動作を行う
        if (!string.IsNullOrEmpty(currentDialogue.type))
        {
            HandleSpecialType(currentDialogue.type);
        }
    }

    // 選択肢を表示するメソッド
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

    // 選択肢ボタンを生成するメソッド
    void CreateChoiceButton(Choice choice)
    {
        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceButtonParent);
        TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = choice.text;

        Button button = choiceButton.GetComponent<Button>();
        button.onClick.AddListener(() => OnChoiceSelected(choice.nextDialogueId));

        // 選択肢にtypeがある場合、そのtypeに応じた処理を追加
        if (!string.IsNullOrEmpty(choice.type))
        {
            button.onClick.AddListener(() => HandleSpecialType(choice.type));
        }
    }

    // 選択肢が選ばれたときの処理
    void OnChoiceSelected(int nextDialogueId)
    {
        ClearChoices();
        nextButton.interactable = true;
        ShowDialogue(nextDialogueId);
    }

    // 選択肢ボタンをクリアするメソッド
    void ClearChoices()
    {
        foreach (Transform child in choiceButtonParent)
        {
            Destroy(child.gameObject);
        }
    }

    // 特定のtypeに応じた処理
    void HandleSpecialType(string type)
    {
        switch (type)
        {
            case "ar_start":
                // ARSceneに進む前に、次に再開する会話IDを保存
                nextDialogueIdAfterAR = currentDialogueId + 1;  // 次に再開するID
                OnSkipButtonPressed();  // ARSceneに進む
                break;
            case "end":
                // 秒数待たせてから終了する処理をAutoNextDialogueで実行するため、ここでは何もしない
                break;
            default:
                Debug.LogWarning($"Unhandled type: {type}");  // 未処理のtypeを警告
                break;
        }
    }

    // ARシーン後に会話を再開するメソッド
    public void ContinueFromAR()
    {
        if (nextDialogueIdAfterAR != -1)
        {
            dialogueWindow.SetActive(true);
            ShowDialogue(nextDialogueIdAfterAR);  // ARシーン後に会話を再開
            nextDialogueIdAfterAR = -1;  // リセット
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
        Destroy(gameObject);  // この場合のオブジェクトは削除する
    }
}
