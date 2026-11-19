using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using System;

[System.Serializable]
public class DropItemStrengs 
{
    public int NoItem;
    public int Weapon;
    public int Heal;
}
public abstract class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Normal, Damaged, Dead }
    [Header("現在の状態")]
    public EnemyState currentState = EnemyState.Normal;
    [Header("共通設定")]
    public Transform target;
    public PlayerManager pManager;
    public Stage room;
    public EnemyHP HP;
    public GameObject body;
    public Collider collider;
    public List<GameObject> DeathDestoroy;

    public int CoinAmount = 10;
    public float DestroyTime = 3f;
    public DropItemStrengs ItemDrop = new DropItemStrengs();

    public GameObject weaponPickupPrefab;
//public WeaponDatabase weaponDatabase;


    public virtual void OnDamage()
    {
        if (currentState == EnemyState.Dead) return;
        currentState = EnemyState.Damaged;
        if (body != null) body.transform.DOShakePosition(0.5f, new Vector3(0.1f, 0.1f, 0.1f), 10, 90f, true);
        StopCoroutine("RecoverFromDamage"); 
        StartCoroutine(RecoverFromDamage(0.5f));
    }

    protected virtual IEnumerator RecoverFromDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentState != EnemyState.Dead)
        {
            currentState = EnemyState.Normal;
        }
    }

    protected virtual void LateUpdate()
    {
        if (body != null)
        {
            Vector3 pos = body.transform.localPosition;
            pos.x = 0; pos.z = 0;
            body.transform.localPosition = pos;
        }
    }

    public virtual void OnDie()
    {
        if (currentState == EnemyState.Dead) return;

        currentState = EnemyState.Dead;

        StopCoroutine("RecoverFromDamage");
        
        if (body != null)
        {
            body.transform.DOKill(); 
        }

        if (collider != null) collider.enabled = false;
        
        // 弱点コライダーをすべて無効化する
        if (HP != null && HP.weakPointColliders != null)
        {
            foreach (var col in HP.weakPointColliders)
            {
                if (col != null) col.enabled = false;
            }
        }

        Debug.Log(gameObject.name + " has died.");

        if (pManager != null && pManager.coinPool != null)
        {
            GameObject c = pManager.coinPool.Get();
            c.transform.position = transform.position;
            c.transform.rotation = transform.rotation;
            Coin CoinScript = c.GetComponent<Coin>();
            if (CoinScript != null)
            {
                CoinScript.moneyAmount = CoinAmount;
                CoinScript.manager = pManager;
            }
        }

        DropItemChoser();

        foreach(var d in DeathDestoroy)
        {
            Destroy(d);
        }

        Destroy(gameObject, DestroyTime); 
    }

    public void DropItemChoser()
{
    int NoItem = ItemDrop.NoItem;
    int Weapon = ItemDrop.Weapon;
    int Heal = ItemDrop.Heal;

    int AllStrengs = NoItem + Weapon + Heal;

    if (AllStrengs <= 0) return;

    int rnd = Random.Range(1, AllStrengs + 1);

    GameObject d = null;

    if (rnd > NoItem + Weapon)
    {
        d = pManager.HealPool.Get();
    }
    else if (rnd > NoItem)
    {
        d = Instantiate(pManager.dropWeapon);
        DropWeapon dw = d.GetComponent<DropWeapon>();
        dw.pManager = pManager;
        dw.Set();
        if(room!=null)room.TemporalyObjects.Add(d);
    }

    if (d == null) return;

    d.transform.position = transform.position;
    d.transform.rotation = transform.rotation;
}
}