using UnityEngine;
public class ChainSwinger : MonoBehaviour
{
    [Header("揺れの設定")]
    public float speed = 0.5f;     
    public float amount = 2.0f;    
    private Quaternion startRotation;
    private float randomOffset;
    void Start()
    {
        startRotation = transform.localRotation;
        randomOffset = Random.Range(0f, 100f);
    }
    void Update()
    {
        float angle = Mathf.Sin(Time.time * speed + randomOffset) * amount;
        transform.localRotation = startRotation * Quaternion.Euler(0, 0, angle);
    }
}