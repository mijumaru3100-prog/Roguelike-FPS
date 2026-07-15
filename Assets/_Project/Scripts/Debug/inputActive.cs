using UnityEngine;
public class inputActive : MonoBehaviour
{
    [SerializeField]private KeyCode actionkey = KeyCode.M;
    [SerializeField]private GameObject target;
    void Start()
    {
        target.SetActive(false);
    }
    void Update()
    {
        if(Input.GetKeyDown(actionkey))
        {
            target.SetActive(true);
        }
        else if (Input.GetKeyUp(actionkey))
        {
            target.SetActive(false);
        }
    }
}
