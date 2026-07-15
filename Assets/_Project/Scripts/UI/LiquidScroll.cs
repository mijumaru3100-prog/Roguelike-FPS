using UnityEngine;
using UnityEngine.UI;

public class LiquidFlow : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;
    [SerializeField] private float speed = 0.05f;

    void Update()
    {
        Rect uv = rawImage.uvRect;

        uv.x += speed * Time.deltaTime;
        uv.y += Mathf.Sin(Time.time) * 0.0005f;

        rawImage.uvRect = uv;


    }
}