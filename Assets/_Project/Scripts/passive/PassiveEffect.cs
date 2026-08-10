using UnityEngine;

public abstract class PassiveEffect : ScriptableObject
{
    public string passiveName;
    public int price;
    public string detailtext; 
    public Sprite icon;
    public GameObject model;
    public bool canStack;
    public bool usecount = false;
    public int remainCount;
    public bool running = true;   

    // ステータス倍率 //(GunBase)
    public virtual float GetFireRateMultiplier(PlayerManager manager) => 0;    
    public virtual float GetDamageMultiplier(PlayerManager manager) => 0;
    public virtual float GetWeakPointBonus(PlayerManager manager) => 0; 
    public virtual float GetReloadSpeedMultiplier(PlayerManager manager) => 0; 
         
    // パッシブ取得 //(Pmanager)
    public virtual void OnGetThisPassive(PlayerManager manager) { }
    public virtual void OnGetPassive(PlayerManager manager) { }

    // 更新 //(Pmanager)
    public virtual void OnUpdate(PlayerManager manager) { }
    public virtual void OnFixedUpdate(PlayerManager manager) { }

    // 移動 //(Pmanager)
    public virtual void OnStopping(PlayerManager manager) { }
    public virtual void OnMoving(PlayerManager manager) { }

    // 敵関連 //(Pmanager)
    public virtual void OnEnemyNear(PlayerManager manager) { }
    public virtual void OnEnemyAway(PlayerManager manager) { }
    public virtual void OnKillEnemy(PlayerManager manager) { }
    public virtual void OnBeforeTakeDamage(PlayerManager manager) { }
    public virtual void OnTakeDamage(PlayerManager manager, float damage, EnemyHP enemyHP) { }

    // 射撃 //(GunBase)
    public virtual void OnHitBullet(PlayerManager manager, float damage, EnemyHP enemyHP) {}
    public virtual void OnBeforeShot(PlayerManager manager) { }
    public virtual void OnShotComplete(PlayerManager manager) { }
    public virtual void OnMiss(PlayerManager manager) { }

    // リロード //(GunBase)
    public virtual void OnReloadComplete(PlayerManager manager) { }
    public virtual void OnBeforeReload(PlayerManager manager) { }

    // 被弾・回復 //(PlayerHP)
    public virtual void OnGetDamage(PlayerManager manager) { }
    public virtual void OnHeal(PlayerManager manager,int amount,PlayerHP HP) { }

    public virtual void OnBattleStart(PlayerManager manager) { }
    public virtual void OnBattleClear(PlayerManager manager) { }

    // public virtual void OnBossStart(PlayerManager manager) { }
    // public virtual void OnBossEnd(PlayerManager manager) { }

    public virtual void OnADSStart(PlayerManager manager) { }
    public virtual void OnADSEnd(PlayerManager manager) { }

    public virtual void OnJump(PlayerManager manager) { }

    // public virtual void OnShopEnter(PlayerManager manager) { }
    // public virtual void OnShopBuy(PlayerManager manager) { }
    // public virtual void OnShopExit(PlayerManager manager) { }
}