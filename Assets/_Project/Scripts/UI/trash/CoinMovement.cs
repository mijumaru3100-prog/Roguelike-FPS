using System;
using UnityEngine;
using DG.Tweening; // 必ずこれを追加

public class CoinMovement : MonoBehaviour
{
    private float duration = 0.8f; // ゴールまでの移動時間（秒）

    public void StartMoving(Transform[] pathPoints, Transform targetPoint, Action onComplete)
    {
        // 1. 移動パスの作成（始点、中間点、終点）
        Vector3[] path = new Vector3[pathPoints.Length + 1];
        for (int i = 0; i < pathPoints.Length; i++)
        {
            path[i] = pathPoints[i].position;
        }
        path[path.Length - 1] = targetPoint.position; // 最後に箱の位置を追加

        // 2. DOPathで滑らかな曲線移動（CatmullRomを指定すると自然なカーブになります）
        transform.DOPath(path, duration, PathType.CatmullRom)
            .SetEase(Ease.OutQuad) // 終盤にかけて少し減速（重力や摩擦の表現）
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                Destroy(gameObject); // 到着したら自身を削除
            });

        // 3. 移動中にコインを回転させる（360度ぐるぐる回す）
        // Z軸をループ回転。移動時間と同じ時間をかけて回します
        transform.DORotate(new Vector3(0, 0, -360), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear);
    }
}