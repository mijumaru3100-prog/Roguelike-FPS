using System;
using UnityEngine;
using DG.Tweening;

public class CoinMovement : MonoBehaviour
{
    private float duration = 0.8f;

    public void StartMoving(Transform[] pathPoints, Transform targetPoint, Action onComplete)
    {
        Vector3[] path = new Vector3[pathPoints.Length + 1];
        for (int i = 0; i < pathPoints.Length; i++)
        {
            path[i] = pathPoints[i].position;
        }
        path[path.Length - 1] = targetPoint.position;

        transform.DOPath(path, duration, PathType.CatmullRom)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                Destroy(gameObject);
            });

        transform.DORotate(new Vector3(0, 0, -360), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear);
    }
}