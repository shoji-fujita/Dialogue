using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Collections;

public class ARPlaceAndRotate : MonoBehaviour
{
    public GameObject objectToPlace;  // 配置するキャラクターのプレハブ
    private ARRaycastManager raycastManager;  // AR Raycast Manager
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();  // Raycast結果を保存するリスト
    private GameObject placedObject;  // 配置されたオブジェクト
    private Camera arCamera;  // ARカメラを保持する
    private DialogueManager dialogueManager;  // DialogueManagerの参照

    void Start()
    {
        // ARRaycastManagerを取得
        raycastManager = GetComponent<ARRaycastManager>();
        arCamera = Camera.main;  // メインカメラ（ARカメラ）を取得
        
        // DialogueManagerを探して取得
        dialogueManager = FindObjectOfType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager が見つかりません！");
        }
    }

    void Update()
    {
        // タッチが1つ以上ある場合のみ処理
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // タッチした場所にRaycastを行い、ARの平面があるか確認
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    // Raycastの結果、平面の位置を取得
                    Pose hitPose = hits[0].pose;

                    // オブジェクトが配置されていない場合に新規に配置
                    if (placedObject == null)
                    {
                        placedObject = Instantiate(objectToPlace, hitPose.position, hitPose.rotation);

                        // キャラクターが出現した後、1秒後にDialogueManagerが会話を再開
                        StartCoroutine(RestartDialogueAfterDelay(1f));
                    }
                    else
                    {
                        // 既存のオブジェクトを移動
                        placedObject.transform.position = hitPose.position;
                        placedObject.transform.rotation = hitPose.rotation;
                    }

                    // オブジェクトをカメラの方向に向かせる
                    LookAtCamera(placedObject);
                }
            }
        }

        // オブジェクトが存在する場合は、常にカメラの方向を向かせる
        if (placedObject != null)
        {
            LookAtCamera(placedObject);
        }
    }

    // オブジェクトをカメラの方向に向けるメソッド
    private void LookAtCamera(GameObject obj)
    {
        // カメラの位置を取得して、キャラクターをカメラの方に回転させる
        Vector3 directionToCamera = arCamera.transform.position - obj.transform.position;
        directionToCamera.y = 0;  // キャラクターが上下方向には回転しないようにする

        // カメラの方向に向かってオブジェクトを回転させる
        obj.transform.rotation = Quaternion.LookRotation(directionToCamera);
    }

    // 1秒後にDialogueManagerの会話を再開するコルーチン
    private IEnumerator RestartDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);  // 指定した秒数待機
        if (dialogueManager != null)
        {
            // DialogueManagerで会話を再開させる (ContinueFromARを呼び出す)
            dialogueManager.ContinueFromAR();
        }
    }
}
