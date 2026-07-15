using UnityEngine;
[CreateAssetMenu(menuName = "Passives/TwoStyle/near_stop")]
public class near_stop : PassiveEffect
{
    [SerializeField] private float startTime;
    [SerializeField] private float chargeTime = 2f;
    [SerializeField] private float damageBuff = 2f;
    [SerializeField] private float range = 5f;
    private bool canBuff = false;
    public override void OnStopping(PlayerManager manager)
    {
        if(manager.minEnemyDistance < range)
        {
            running = true;
            canBuff = true;
            startTime = Time.time;
        }
    }
    public override void OnMoving(PlayerManager manager)
    {
        canBuff = false;
        running = false;
    }
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        if (canBuff && Time.time - startTime >= chargeTime)
        {
            return damageBuff;
        }
        return 0f;
    }
}
