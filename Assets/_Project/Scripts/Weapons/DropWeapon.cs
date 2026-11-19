
using UnityEngine;
using TMPro;

public class DropWeapon : MonoBehaviour
{
    [Header("Setup")]
    public PlayerManager pManager;
    public WeaponDatabase itempool;

    [Header("Weapon Data")]
    public WeaponShopEntry selected;
    public WeaponAbility SelectedAbility;

    [Header("Display")]
    public Transform modelSpawnPos;
    public float spinSpeed;

    private GameObject model;
    private bool isInside;
    private bool isSwapping;

    private void Start() 
    {
        Set();
    }

    private void Update()
    {
        if (model != null )
        {
            model.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        }

        if (isInside && !isSwapping &&
            Input.GetKeyDown(KeyCode.E))
        {
            ExchangeWeapon();
        }
    }

    // 初期配置する武器を決定
    public void Set()
{


    if (pManager == null)
    {
        Debug.LogError("[DropWeapon] pManagerがnull");
        return;
    }

    if (itempool == null)
    {
        Debug.LogError("[DropWeapon] itempoolがnull");
        return;
    }

    if (itempool.weapons == null)
    {
        Debug.LogError("[DropWeapon] weaponsリストがnull");
        return;
    }


    foreach (WeaponShopEntry entry in itempool.weapons)
    {
        if (entry == null)
        {
            Debug.LogWarning("[DropWeapon] WeaponShopEntryがnull");
            continue;
        }

        Debug.Log(
            $"[DropWeapon] 武器名={entry.weaponName}, " +
            $"Prefab={(entry.weaponPrefab != null ? entry.weaponPrefab.name : "null")}, " +
            $"Model={(entry.model != null ? entry.model.name : "null")}"
        );
    }

    selected = itempool.GetRandom(pManager);

    if (selected == null)
    {
        Debug.LogError("[DropWeapon] GetRandomの戻り値がnull");
        return;
    }

    Debug.Log($"[DropWeapon] 選択された武器: {selected.weaponName}");

    if (selected.weaponPrefab == null || selected.model == null)
    {
        Debug.LogError("[DropWeapon] 選択された武器のPrefabまたはModelがnull");
        return;
    }

    //if (SelectedAbility == null &&
    //        pManager.DropAbilitys != null &&
    //        pManager.DropAbilitys.Count > 0)
    //    {
    //        SelectedAbility = pManager.DropAbilitys[
    //            Random.Range(0, pManager.DropAbilitys.Count)
    //        ];
    //    }

    RefreshModel();
    UpdatePrompt();

    Debug.Log("[DropWeapon] Set完了");
}

    private void RefreshModel()
    {
        if (model != null)
        {
            Destroy(model);
            model = null;
        }

        if (selected == null || selected.model == null)
            return;

        Transform parent =
            modelSpawnPos != null ? modelSpawnPos : transform;

        model = Instantiate(selected.model, parent);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
    }

    private void UpdatePrompt()
    {
        if (!isInside || pManager == null)
            return;

        if (pManager.PlayerBuyText != null)
        {
            pManager.PlayerBuyText.SetActive(selected != null);

            if (pManager.buyTextData != null && selected != null)
            {
                pManager.buyTextData.text ="Press E to exchange for " + selected.weaponName;
                //pManager.buyTextData.text ="Press E to exchange for " + selected.weaponName +"("+ SelectedAbility.abilityName+")";
            }
        }
    }


       private void ExchangeWeapon()
{

    if (isSwapping || pManager == null || selected == null || itempool == null)
    {
        Debug.LogWarning(
            $"交換中={isSwapping}, " +
            $"pManager={pManager != null}, " +
            $"selected={selected != null}, " +
            $"itempool={itempool != null}"
        );
        return;
    }

    GunBase oldGun = pManager.currentWeapon;

    if (oldGun == null)
    {
        Debug.LogWarning("[DropWeapon] currentWeaponがnull");
        return;
    }

        // 元の武器のデータを取得
        WeaponShopEntry oldEntry = FindEntry(oldGun);

        if (oldEntry == null || oldEntry.model == null)
        {
            Debug.LogWarning(
                "[DropWeapon] 現在の武器に対応するデータ、またはModelがありません。"
            );
            return;
        }

        if (selected.weaponPrefab == null)
            return;

        isSwapping = true;

        WeaponShopEntry newEntry = selected;
        WeaponAbility newAbility = SelectedAbility;
        WeaponAbility oldAbility = oldGun.ability;

        GameObject newWeaponObj = null;

        try
        {
            // 新しい武器を先に生成する
            newWeaponObj = Instantiate(
                newEntry.weaponPrefab,
                pManager.weaponHolder
            );

            GunBase newGun = newWeaponObj.GetComponent<GunBase>();

            // 生成に失敗した場合、古い武器は破棄しない
            if (newGun == null)
            {
                Debug.LogError(
                    "[DropWeapon] 新しいPrefabにGunBaseがありません。"
                );

                Destroy(newWeaponObj);
                return;
            }

            // 新しい武器の初期設定
            newWeaponObj.transform.localPosition =
                newGun.offsetPosition;

            newWeaponObj.transform.localRotation =
                Quaternion.Euler(newGun.offsetRotation);

            newGun.pManager = pManager;
            newGun.stats = pManager.sharedStats;
            newGun.ammoBeltUI = pManager.ammoBeltUI;
            newGun.ammoText = pManager.ammoTextUI;
            newGun.playerCamera = pManager.mainCamera;
            newGun.adsPivot = pManager.adsPivot;
            newGun.recoilPivot = pManager.recoilPivot;
            newGun.ability = newAbility;

            // 新しい武器を装備
            pManager.currentWeapon = newGun;

            // このドロップを元の武器に差し替える
            selected = oldEntry;
            SelectedAbility = oldAbility;

            RefreshModel();

            // 交換が成立した後で古い武器を破棄
            Destroy(oldGun.gameObject);

            Debug.Log(
                "[DropWeapon] 武器を交換しました：" +
                newEntry.weaponName + " ⇔ " + oldEntry.weaponName
            );

            UpdatePrompt();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);

            // 交換に失敗した場合は新しい生成物を破棄
            if (newWeaponObj != null &&
                pManager.currentWeapon !=
                newWeaponObj.GetComponent<GunBase>())
            {
                Destroy(newWeaponObj);
            }
        }
        finally
        {
            isSwapping = false;
        }
    }

    // 装備中の武器に対応するデータを検索
    private WeaponShopEntry FindEntry(GunBase gun)
    {
        if (gun == null || itempool == null ||
            itempool.weapons == null)
            return null;

        string instanceName = gun.gameObject.name;
        WeaponShopEntry bestMatch = null;
        int longestName = -1;

        foreach (WeaponShopEntry entry in itempool.weapons)
        {
            if (entry == null || entry.weaponPrefab == null)
                continue;

            string prefabName = entry.weaponPrefab.name;

            if (instanceName == prefabName ||
                instanceName == prefabName + "(Clone)")
            {
                return entry;
            }

            // 名前の一部が一致する場合は最長一致を採用
            if (instanceName.StartsWith(prefabName) &&
                prefabName.Length > longestName)
            {
                bestMatch = entry;
                longestName = prefabName.Length;
            }
        }

        return bestMatch;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isInside = true;
        UpdatePrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isInside = false;

        if (pManager != null &&
            pManager.PlayerBuyText != null)
        {
            pManager.PlayerBuyText.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (pManager != null &&
            pManager.PlayerBuyText != null)
        {
            pManager.PlayerBuyText.SetActive(false);
        }
    }
}
