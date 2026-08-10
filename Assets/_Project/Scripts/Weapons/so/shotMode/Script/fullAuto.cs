using UnityEngine;

[CreateAssetMenu(menuName = "Gun/mode/FullAuto")]
public class fullAutoMode : shotMode
{
    public override bool IsFiring() 
    {
        return Input.GetButton("Fire1");
    }
}