using UnityEngine;
public class Stage : MonoBehaviour
{
    public GameObject wall;
    public Transform respawnPoint;
    public Transform elevatorStartPos;
    public Transform elevatorEndPos;
    public virtual void ResetStage()
    {
        Debug.Log("Resetting stage");
    }
    public virtual void StartStage()
    {
        Debug.Log("Starting stage");
    }
}