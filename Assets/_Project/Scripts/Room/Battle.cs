using UnityEngine;
using System.Collections.Generic;
using System;

public class Battle : Stage
{
    public PlayerManager manager;
    [Header("閉じ込める対象（ドアやバリアなど）")]
    public GameObject[] targetObjects; 
    [Header("エレベーター")]
    public List<Elevator> Elevators = new List<Elevator>();

    [Header("出現させる敵のプレハブ")]
    [SerializeField]
    private List<SpawnData> EnemySpawns = new List<SpawnData>();
    [Serializable]
    public class SpawnData
    {
        public Transform spawnPoint;
        public GameObject enemyPrefab;
    }
    private List<GameObject> _activeEnemies = new List<GameObject>();

    private bool _isBattleStarted = false;
    private bool _finished = false;
    public override void ResetStage()
    {
        base.ResetStage();

        _finished = false;
        _isBattleStarted = false;

        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        _activeEnemies.Clear();

        foreach (var ele in Elevators)
        {
            if (ele != null) ele.ResetStage();
        }
    }

    public override void StartStage()
    {
        if (_finished) return;
        if (_isBattleStarted) return;

        base.StartStage();
        
        _isBattleStarted = true;

        foreach (var p in manager.activePassives.ToArray())
        {
            p.OnBattleStart(manager);
        }

        foreach (GameObject obj in targetObjects)
        {
            if (obj != null) obj.SetActive(true); 
        }

        foreach (var sp in EnemySpawns)
        {

            GameObject enemy = Instantiate(sp.enemyPrefab, sp.spawnPoint.position, sp.spawnPoint.rotation);
            _activeEnemies.Add(enemy);

            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            ai.pManager = manager;
            ai.HP.pManager = manager;
    
            if (ai != null)
            {
                if (manager != null && manager.Player != null)
                {
                    ai.target = manager.Player.transform;
                }
                else
                {
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null)
                    {
                        ai.target = playerObj.transform;
                    }
                }
            }
        }
        
        Debug.Log("<color=red>戦いの、始まりだ、よ</color>");
    }

        void Update()
    {
        if (!_isBattleStarted) return;
        if (_finished) return;

        _activeEnemies.RemoveAll(item => item == null);

        if (_activeEnemies.Count == 0)
        {
            EndBattle();
        }
    }

        void EndBattle()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null) obj.SetActive( false); 
        }
        
        Debug.Log("<color=green>お掃除、完了。先へ、進んで、いい、よ</color>");

        foreach (var p in manager.activePassives.ToArray())
        {
            p.OnBattleClear(manager);
        }

        _finished = true; 
    }
}
