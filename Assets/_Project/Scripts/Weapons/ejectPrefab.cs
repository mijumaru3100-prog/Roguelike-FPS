using UnityEngine;

public class ejectPrefab : MonoBehaviour
{
    public AudioClip shellSound;
    
    void Start()
    {
        PlayerManager pManager = FindObjectOfType<PlayerManager>();
        if (pManager != null)
        {
            Collider myCollider = GetComponent<Collider>();
            if (myCollider != null)
            {
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
