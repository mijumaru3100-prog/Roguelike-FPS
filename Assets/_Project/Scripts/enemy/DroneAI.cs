using UnityEngine;
using UnityEngine.AI;

public class DroneAI : ChargerAI
{
    [Header("ドローンホバー設定")]
    [Tooltip("ホバーする高さ (NavMeshの基準面からの高さ)")]
    public float hoverHeight = 2.0f;
    [Tooltip("上下揺れのスピード")]
    public float hoverSpeed = 2.0f;
    [Tooltip("上下揺れの幅")]
    public float hoverAmplitude = 0.3f;

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (body != null)
        {
            Vector3 pos = body.transform.localPosition;
            float hoverOffset = hoverHeight + Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            pos.y = hoverOffset;
            body.transform.localPosition = pos;
        }
    }
}
