using UnityEngine;
public class FloorRandomizer : MonoBehaviour
{
    [Header("ガチャの設定")]
    public float maxTilt = 25f;
    public float maxOffset = 2.0f;
    [Range(1f, 5f)]
    public float curvePower = 2.0f;
    [HideInInspector, SerializeField] private Vector3[] basePositions;
    [HideInInspector, SerializeField] private Quaternion[] baseRotations;
    [ContextMenu("今の位置を『デフォルト』として保存")]
    public void SaveCurrentState()
    {
        int count = transform.childCount;
        basePositions = new Vector3[count];
        baseRotations = new Quaternion[count];
        for (int i = 0; i < count; i++)
        {
            basePositions[i] = transform.GetChild(i).localPosition;
            baseRotations[i] = transform.GetChild(i).localRotation;
        }
        Debug.Log("現在の配置を『デフォルト』として記憶しました。");
    }
    [ContextMenu("ガチャを実行（保存した位置がベース）")]
    public void RandomizeFloors()
    {
        if (basePositions == null || basePositions.Length != transform.childCount)
        {
            SaveCurrentState();
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform floor = transform.GetChild(i);
            float rawX = Mathf.Pow(Random.value, curvePower) * (Random.value > 0.5f ? 1f : -1f);
            float rawZ = Mathf.Pow(Random.value, curvePower) * (Random.value > 0.5f ? 1f : -1f);
            floor.localRotation = baseRotations[i] * Quaternion.Euler(rawX * maxTilt, Random.Range(0, 360f), rawZ * maxTilt);
            float offX = -rawX * maxOffset;
            float offZ = -rawZ * maxOffset;
            floor.localPosition = basePositions[i] + new Vector3(offX, 0, offZ);
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(floor);
            #endif
        }
    }
    [ContextMenu("保存したデフォルト位置に戻す")]
    public void ResetFloors()
    {
        if (basePositions == null) return;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = basePositions[i];
            transform.GetChild(i).localRotation = baseRotations[i];
        }
    }
}