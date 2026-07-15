using UnityEngine;
using UnityEngine.AI;

public class SniperAI : EnemyAI
{
    public GameObject Razer;
    
    protected override void Update()
    {
        base.Update();

        if (Razer != null)
        {
            bool hasLineOfSight = target != null && currentState == EnemyState.Normal && CanSeePlayer(target.position + Vector3.up * aimHeightOffset);
            Razer.SetActive(hasLineOfSight);
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void UpdateMovement(float dist, bool canSee)
    {
        //移動しない
    }

protected override float GetAnimationSpeed(float currentDist)
    {
         return 0.5f;
    }
}