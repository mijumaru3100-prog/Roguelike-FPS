using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ElevatorRoomEffect : MonoBehaviour
{
    [Header("Settings")]
    public Battle battle;
    public List<ElevatorDoor> ElevatorDoors;
    
    [Header("Start Object")]
    [Tooltip("起動時に移動させるオブジェクト")]
    public Transform startObject;
    [Tooltip("移動先の位置を示すマーカーオブジェクト")]
    public Transform startObjectTargetMarker;
    public float startObjectMoveSpeed = 5f;

    [Header("End Object")]
    [Tooltip("終了時に移動させるオブジェクト")]
    public Transform endObject;
    [Tooltip("終了時にオブジェクトが現れる開始位置のマーカー")]
    public Transform endObjectStartMarker;
    [Tooltip("終了時にオブジェクトが移動する目標位置のマーカー")]
    public Transform endObjectTargetMarker;
    public float endObjectMoveSpeed = 5f;

    [Header("Backgrounds (Looping)")]
    public Transform background1;
    public Transform background2;
    public float scrollSpeed = 5f;
    [Tooltip("背景がここまで下がったら上に戻るY座標のマーカー")]
    public Transform backgroundResetMarker; 
    [Tooltip("背景が戻る時の上の位置のY座標のマーカー")]
    public Transform backgroundStartMarker; 

    private bool _isActivated = false;
    private bool _isEndObjectFinished = false;

    void Start()
    {
        foreach(var door in ElevatorDoors) door.Open(); 
    }

    void Reset()
    {
        if (battle == null)
            battle = GetComponentInParent<Battle>();
        if (battle == null)
            battle = GetComponent<Battle>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActivated && other.CompareTag("Player"))
        {
            Activate();
        }
    }

    private void Activate()
    {
        _isActivated = true;

        //ドアを閉める
        {
            foreach(var door in ElevatorDoors) door.Close(); 
        }

        
        // 起動時のオブジェクト移動
        if (startObject != null && startObjectTargetMarker != null)
        {
            StartCoroutine(MoveObject(startObject, startObjectTargetMarker.position, startObjectMoveSpeed, null));
        }
        
        // 敵のスポーンを開始
        if (battle != null)
        {
            battle.SpawnEnemiesForElevator();
        }
    }

    void Update()
    {
        // 起動中から終了処理完了まで背景をスクロールし続ける
        if (_isActivated && !_isEndObjectFinished)
        {
            ScrollBackground(background1);
            ScrollBackground(background2);
        }
    }

    private void ScrollBackground(Transform bg)
    {
        if (bg == null || backgroundResetMarker == null || backgroundStartMarker == null) return;
        
        bg.Translate(Vector3.down * scrollSpeed * Time.deltaTime, Space.World);
        
        if (bg.position.y <= backgroundResetMarker.position.y)
        {
            bg.position = new Vector3(bg.position.x, backgroundStartMarker.position.y, bg.position.z);
        }
    }

    public IEnumerator FinishEffectCoroutine(System.Action onComplete)
    {
        if (endObject != null && endObjectStartMarker != null && endObjectTargetMarker != null)
        {
            // 最初は上部の見えない位置に移動
            endObject.position = endObjectStartMarker.position;
            
            // 下の目標位置に向かって移動
            yield return StartCoroutine(MoveObject(endObject, endObjectTargetMarker.position, endObjectMoveSpeed, () => 
            {
                foreach(var door in ElevatorDoors) door.Open(); 
                _isEndObjectFinished = true;
                onComplete?.Invoke();
            }));
        }
        else
        {
            foreach(var door in ElevatorDoors) door.Open(); 
            _isEndObjectFinished = true;
            onComplete?.Invoke();
        }
    }

    private IEnumerator MoveObject(Transform obj, Vector3 target, float speed, System.Action onComplete)
    {
        if (obj == null) yield break;
        
        while (Vector3.Distance(obj.position, target) > 0.01f)
        {
            obj.position = Vector3.MoveTowards(obj.position, target, speed * Time.deltaTime);
            yield return null;
        }
        obj.position = target;
        onComplete?.Invoke();
    }
}
