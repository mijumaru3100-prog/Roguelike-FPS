using UnityEngine;

[CreateAssetMenu(menuName = "Gun/mode/FullAuto")]
public class fullAutoMode : shotMode
{
    // GunBaseのUpdateから呼び出される、よ
    public override bool IsFiring() 
    {
        return Input.GetButton("Fire1");
    }
}