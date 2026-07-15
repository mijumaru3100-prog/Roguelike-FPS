using UnityEngine;
using TMPro;

public class FloorCount : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public DungeonManager manager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Text.text ="CurrentFloor:0";
    }

    public void CountUpdate()
    {
        int currentFloor = manager.currentFloor; 
        Text.text ="CurrentFloor:"+currentFloor;
    }
}
