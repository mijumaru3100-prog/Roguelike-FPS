using UnityEngine;

[CreateAssetMenu(menuName = "Gun/mode/semiAuto")]
public class semiAutoMode : shotMode
{
    // GunBaseのUpdateから呼び出される、よ
    public override bool IsFiring() 
    {
        return Input.GetButtonDown("Fire1");
    }
}