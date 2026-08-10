using UnityEngine;

[CreateAssetMenu(menuName = "Gun/mode/semiAuto")]
public class semiAutoMode : shotMode
{
    public override bool IsFiring() 
    {
        return Input.GetButtonDown("Fire1");
    }
}