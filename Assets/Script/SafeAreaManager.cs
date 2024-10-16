using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeAreaManager : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    void Start()
    {
        // Panel の RectTransform を取得
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        // デバイスのセーフエリアを取得
        Rect safeArea = Screen.safeArea;

        // すでに設定されたセーフエリアと同じ場合はスキップ
        if (safeArea == lastSafeArea) return;

        lastSafeArea = safeArea;

        // セーフエリアを画面サイズに基づいて正規化
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Panelのアンカーをセーフエリアに適用
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }

    void Update()
    {
        // 画面回転やサイズ変更に対応するため常にチェック
        ApplySafeArea();
    }
}
