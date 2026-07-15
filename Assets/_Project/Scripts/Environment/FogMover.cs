using UnityEngine;
public class FogMover : MonoBehaviour
{
    public float speedX = 0.01f;
    public float speedY = 0.01f;
    private Material mat;
    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }
    void Update()
    {
        Vector2 offset = mat.GetTextureOffset("_BaseMap");
        offset.x += speedX * Time.deltaTime;
        offset.y += speedY * Time.deltaTime;
        mat.SetTextureOffset("_BaseMap", offset);
    }
}