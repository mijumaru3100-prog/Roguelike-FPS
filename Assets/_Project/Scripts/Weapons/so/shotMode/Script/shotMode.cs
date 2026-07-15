using UnityEngine;

public abstract class shotMode : ScriptableObject 
{
    // GunBaseのUpdateから呼び出される、よ
    public abstract bool IsFiring();
}