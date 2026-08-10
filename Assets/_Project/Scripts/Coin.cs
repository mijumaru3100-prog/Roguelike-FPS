using UnityEngine;

public class Coin : MonoBehaviour
{
    public int moneyAmount = 10;
    [Header("移動設定")]
    [SerializeField] private string playerTag = "Player"; 
    [SerializeField] private float moveSpeed = 5f;     
    [SerializeField] private float acceleration = 2f;   

    private Transform playerTransform;
    public PlayerManager manager;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        moveSpeed += acceleration * Time.deltaTime;

        transform.position = Vector3.MoveTowards(
            transform.position, 
            playerTransform.position, 
            moveSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            manager = other.GetComponent<PlayerManager>();
            if(manager != null)
            {
                manager.AddMoney(moneyAmount);
            }

            manager.coinPool.ReturnToPool(gameObject);
        }
    }
}