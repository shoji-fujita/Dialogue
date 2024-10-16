using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Text, Image, ButtonなどのUIコンポーネントを使うために必要
using UnityEngine.SceneManagement;  // SceneManagerを使用するための名前空間
using TMPro;  // TextMeshProの名前空間を追加

// キャラクターに関するデータを保持するクラス
public class Character
{
    public string Name;  // キャラクターの名前
    public Sprite Thumbnail;  // キャラクターのサムネイル画像

    // コンストラクタを追加して、簡単にデータをセットできるようにします
    public Character(string name, Sprite thumbnail)
    {
        Name = name;
        Thumbnail = thumbnail;
    }
}

// キャラクター選択を管理するクラス
public class CharacterSelect : MonoBehaviour
{
    public GameObject buttonPrefab;  // キャラクターを選択するためのボタンのPrefab
    public Transform content;  // ScrollViewやGridLayoutのContent部分
    private List<Character> characters;

    private void Start()
    {
        characters = new List<Character>
        {
            new Character("Warrior", Resources.Load<Sprite>("Images/UnityGirl")),
            new Character("Mage", Resources.Load<Sprite>("Images/UnityGirl")),
            new Character("Archer", Resources.Load<Sprite>("Images/UnityGirl"))
        };

        // キャラクター情報を基にボタンを生成
        foreach (var character in characters)
        {
            // ボタンPrefabを生成
            GameObject button = Instantiate(buttonPrefab, content);

            // TextMeshProUGUIコンポーネントを取得
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                // ボタンにキャラクター名を設定
                buttonText.text = character.Name;
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component not found on the button prefab.");
            }

            // ボタンにキャラクターサムネイルを設定
            Image buttonImage = button.GetComponentInChildren<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = character.Thumbnail;
            }

            // キャプチャの問題を防ぐためにローカル変数にキャラクター情報を保存
            string selectedCharacterName = character.Name;

            // ボタンをクリックした時の処理を追加
            button.GetComponent<Button>().onClick.AddListener(() => OnCharacterSelected(selectedCharacterName));
        }
    }

    // キャラクター選択時の処理
    public void OnCharacterSelected(string characterName)
    {
        // 静的変数に選択されたキャラクター情報を保存
        CommonData.SelectedCharacterName = characterName;

        // 遷移するシーンをロード（"3DScene"に変更）
        SceneManager.LoadScene("3DScene");
    }
}
