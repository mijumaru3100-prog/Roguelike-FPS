using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Passives/ManyShotStyle/Shot_BuffShort")]
public class Shot_BuffShort: PassiveEffect
{
    public PlayerStats buffStats;
    [SerializeField] private float buffDuration = 1.5f;

    //親で宣言済み//  
    //public int remainCount;
    //public bool running = true;   

    public override void OnShotComplete(PlayerManager manager) 
    { 
        manager.sharedStats.ApplyModifier(buffStats, true);

        manager.StartCoroutine(buffCancel(manager));
    }

    public IEnumerator buffCancel(PlayerManager manager)
    {
        yield return new WaitForSeconds(buffDuration);
        manager.sharedStats.ApplyModifier(buffStats, false);
    }
}
