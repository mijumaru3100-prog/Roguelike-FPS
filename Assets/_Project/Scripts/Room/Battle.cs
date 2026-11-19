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

    [Header("エレベーター部屋エフェクト (任意)")]
    public ElevatorRoomEffect elevatorEffect;

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
    private bool _isEndProcessing = false;
    private bool _isEnemiesSpawned = false;
    public override void ResetStage()
    {
        base.ResetStage();

        _finished = false;
        _isBattleStarted = false;
        _isEndProcessing = false;
        _isEnemiesSpawned = false;
        _isSlowMotionStarted = false;
        Time.timeScale = 1f;

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

        if (elevatorEffect != null)
        {
            // ElevatorRoomEffectがプレイヤーの侵入を検知してSpawnEnemiesForElevatorを呼ぶ
        }
        else
        {
            SpawnEnemiesForElevator();
        }
    }

    public void SpawnEnemiesForElevator()
    {
        foreach (var sp in EnemySpawns)
        {

            GameObject enemy = Instantiate(
            sp.enemyPrefab,
            sp.spawnPoint.position,
            sp.spawnPoint.rotation
            );

            _activeEnemies.Add(enemy);

            Transform playerTransform = null;

            if (manager != null && manager.Player != null)
            {
                playerTransform = manager.Player.transform;
            }
            else
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
            }


            // 通常敵
            EnemyAI ai = enemy.GetComponent<EnemyAI>();

            if (ai != null)
            {
                ai.pManager = manager;
                ai.HP.pManager = manager;
                ai.target = playerTransform;
                ai.room = this;
            }


            // ヘリ
            Hericopter helicopter = enemy.GetComponent<Hericopter>();

            if (helicopter != null)
            {
                helicopter.pManager = manager;
                helicopter.HP.pManager = manager;
                helicopter.target = playerTransform;
            }
        }
        
        _isEnemiesSpawned = true;
    }

    private bool _isSlowMotionStarted = false;

    void Update()
    {
        if (!_isBattleStarted) return;
        if (_finished) return;
        if (_isEndProcessing) return;

        _activeEnemies.RemoveAll(item => item == null);

        if (_isEnemiesSpawned)
        {
            bool allDead = true;
            float maxDestroyTime = 3f;

            if (_activeEnemies.Count > 0)
            {
                foreach (var enemy in _activeEnemies)
                {
                    EnemyAI ai = enemy.GetComponent<EnemyAI>();
                    if (ai != null)
                    {
                        if (ai.currentState != EnemyAI.EnemyState.Dead)
                        {
                            allDead = false;
                            break;
                        }
                        if (ai.DestroyTime < maxDestroyTime) maxDestroyTime = ai.DestroyTime;
                    }
                }
            }

            if (allDead && !_isSlowMotionStarted)
            {
                _isSlowMotionStarted = true;
                StartCoroutine(LastEnemySlowMotionCoroutine(maxDestroyTime));
            }
        }
    }

    System.Collections.IEnumerator LastEnemySlowMotionCoroutine(float waitTime)
    {
        Time.timeScale = 0.5f;

        yield return new WaitForSeconds(waitTime);

        Time.timeScale = 1f;

        _isEndProcessing = true;

        if (elevatorEffect != null)
        {
            StartCoroutine(elevatorEffect.FinishEffectCoroutine(() => EndBattle()));
        }
        else
        {
            EndBattle();
        }
    }

    [Header("Reward UI Settings")]
    public TMPro.TMP_FontAsset rewardFont;

    void EndBattle()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null) obj.SetActive( false); 
        }
        
        foreach (var p in manager.activePassives.ToArray())
        {
            p.OnBattleClear(manager);
        }
        
        SpawnRewards();

        _finished = true; 
    }

    void SpawnRewards()
    {
        PassiveDatabase db = null;
        passiveshop[] shops = Resources.FindObjectsOfTypeAll<passiveshop>();
        foreach (var s in shops) {
            if (s.itempool != null) {
                db = s.itempool;
                break;
            }
        }
        
        if (db == null) {
            Debug.LogWarning("PassiveDatabase not found in memory!");
            return;
        }

        List<PassiveEffect> choices = new List<PassiveEffect>();
        for (int i = 0; i < 3; i++)
        {
            PassiveEffect entry = db.GetRandom(manager);
            if (entry != null) choices.Add(entry);
        }

        if (choices.Count > 0)
        {
            GameObject uiObj = new GameObject("RewardSelectionUI");
            RewardSelectionUI ui = uiObj.AddComponent<RewardSelectionUI>();
            ui.customFont = rewardFont;
            ui.BuildAndShow(manager, choices);
        }
    }
}
