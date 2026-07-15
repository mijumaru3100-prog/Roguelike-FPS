using UnityEngine;
using System.Collections.Generic;
using System;


public class Battle : Stage
{
    public PlayerManager manager;
    [Header("閉じ込める対象（ドアやバリアなど）")]
    public GameObject[] targetObjects; 
    [Header("エレベーター")]
    private List<Elevator> Elevators = new List<Elevator>();

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
        _finished = false;
        _isBattleStarted = false;

        // もし前の敵が残っていたら消してリストを空にする
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
        _isBattleStarted = true;

        foreach (var p in manager.activePassives)
        {
            p.OnBattleStart(manager);
        }

        // 1. オブジェクトを「アクティブ」にして、退路を断つ
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null) obj.SetActive(true); 
        }

        // 2. 敵を、一斉に解き放つ
        foreach (var sp in EnemySpawns)
        {

            GameObject enemy = Instantiate(sp.enemyPrefab, sp.spawnPoint.position, sp.spawnPoint.rotation);
            _activeEnemies.Add(enemy);

            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            ai.pManager = manager;
            ai.HP.pManager = manager;
    
            // もし AI を持っていたら、あなた（p）をターゲットとして教える、よ
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

        // 3. 生きている敵を、掃除（リストから削除）
        _activeEnemies.RemoveAll(item => item == null);

        // 4. 全滅の、判定
        if (_activeEnemies.Count == 0)
        {
            EndBattle();
        }
    }

        void EndBattle()
    {
        // 5. オブジェクトを「アクティブ」に戻して、解放してあげる
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null) obj.SetActive( false); 
        }
        
        Debug.Log("<color=green>お掃除、完了。先へ、進んで、いい、よ</color>");

        foreach (var p in manager.activePassives)
        {
            p.OnBattleClear(manager);
        }

        
        // この監督の役目は終わり、だ、ね
        _finished = true; 
    }
}
