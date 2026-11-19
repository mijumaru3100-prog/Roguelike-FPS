using UnityEngine;

public class ShootableObject : MonoBehaviour
{
    public virtual void Attacked(Collider hitCollider,Vector3 AttackedDirection,float AttackedForce)
    {

    }

    public virtual void Reset()
    {
        
    }
}
