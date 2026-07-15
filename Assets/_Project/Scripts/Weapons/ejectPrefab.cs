using UnityEngine;

public class ejectPrefab : MonoBehaviour
{
    public AudioClip shellSound;
    
    void Start()
    {
        // プレイヤー自身（CharacterController等）とぶつからないようにする、よ
        PlayerManager pManager = FindObjectOfType<PlayerManager>();
        if (pManager != null)
        {
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null)
            {
                // キャラクターコントローラーなどを含む全コライダーと相互無視
                Collider[] playerColliders = pManager.GetComponentsInChildren<Collider>();
                foreach (Collider pc in playerColliders)
                {
                    Physics.IgnoreCollision(myCollider, pc);
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
    // 一度だけ「チャリン」と鳴らして、役目を終える……
        if (collision.relativeVelocity.magnitude > 0.5f) 
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if(audioSource != null && shellSound != null)
            {
                audioSource.PlayOneShot(shellSound);
            }
        }
    }
}
