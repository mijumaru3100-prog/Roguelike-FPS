using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/ManyShotStyle/MissCover")]
public class MissCover: PassiveEffect
{

    [SerializeField] private float buffDuration = 0.5f;
    private float lastHitTime;
    public override float GetDamageMultiplier(PlayerManager manager)
    {
        running = false;
        if(Time.time >= lastHitTime+buffDuration)
        {
            return 1f;
        }
        else
        {
            return 0f;
        }
    }

    public override void OnGetThisPassive(PlayerManager manager) 
    {
        running = false;
    }

    public override void OnMiss(PlayerManager manager)
    {
        running = true;
        lastHitTime = Time.time;
        manager.StartCoroutine(wait());
    }

   IEnumerator wait()
    {
        yield return new WaitForSeconds(buffDuration);
        running = false;
    }
}
