using UnityEngine;

public class BuffFlug : MonoBehaviour
{
    public bool IsOneShotKill = false;
    public bool MaxHP = false;
    public bool IsHeadShot = false;



    public void FlugReset()
    {
        IsOneShotKill = false;
        MaxHP = false;
        IsHeadShot = false;   
    }
    void Start()
    {
        FlugReset();    
    }
}
