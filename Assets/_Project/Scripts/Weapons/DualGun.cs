using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Random = UnityEngine.Random;

public class DualGun : GunBase
{
    [Header("DualSetting")]
    public Transform muzzlePoint1;
    public Transform muzzlePoint2;

    protected override void Start()
    {
        base.Start();
        muzzlePoint = muzzlePoint1;
    }

    public override int maxAmmo => Mathf.RoundToInt(Mathf.Max(1, (baseMaxAmmo + stats.bonusMaxAmmo * 2) * (1 + stats.maxAmmoMultiple * 2)));

    public override float fireRate
    {
        get
        {
            float totalMult = stats.fireRateMultiple;

            if (pManager != null)
            {
                foreach (var p in pManager.activePassives.ToArray())
                    totalMult += p.GetFireRateMultiplier(pManager);
            }

            float ammoBonus = (maxAmmo - baseMaxAmmo) / (float)baseMaxAmmo;
            totalMult += ammoBonus * 0.2f;

            totalMult = Mathf.Max(0.01f, totalMult);

            return ((60f / defaultRPM) / (1 + totalMult));
        }
    }

    protected override void fire()
    {
       if(isNPC == false)
        {
            foreach (var p in pManager.activePassives.ToArray())
            {
                p.OnBeforeShot(pManager);
            }
        }
        
        if (shotAction == null && isNPC == false) 
        {
            Debug.Log("shotactionが未設定...だ、よ。ままならないね...");
            return;
        }

        if(muzzlePoint == muzzlePoint1) muzzlePoint = muzzlePoint2;
        else muzzlePoint = muzzlePoint1;

        _currentAmmo --;
        lastFireTime = Time.time;

        if(isNPC == false)
        {
            UpdateAmmoDisplay();
            ApplyGunRecoil();
        }

        shotAction.shot(this);
        currentHeat += heatPerShot;
        PlayShotSound();
        PlayMuzzleFlash();
        
        if(!isNPC)
        {
            foreach (var set in gunPartSets)
            {
                if (set.actionData != null)
                {
                    set.actionData.Execute(this, set.part, set.defaultPos, set.defaultRot);
                }
            }

             if (crosshair != null)
            {
                crosshair.AddSpread(10f); 
            }

            foreach (var p in pManager.activePassives.ToArray())
            {
                p.OnShotComplete(pManager);
            }
        }

        if(useEject) EjectShell();
    }
}