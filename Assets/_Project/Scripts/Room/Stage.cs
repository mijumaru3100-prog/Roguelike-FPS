using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Random = UnityEngine.Random;

public class Stage : MonoBehaviour
{
    public GameObject wall;
    public Transform respawnPoint;
    public Transform elevatorStartPos;
    public Transform elevatorEndPos;

    public RoomStyle CurrentStyle;
    public List<GameObject> TemporalyObjects = new();
    public List<ShootableObject> ShootableObjects;
    
    [SerializeField]
    private List<DecorationChanger> decorationChangers = new();

    void Start()
    {
        ChangeDecoration();
    }

    public virtual void ResetStage()
    {
        Debug.Log("Resetting stage");
        RoomStyleChanger();
        ChangeDecoration();
        
        foreach(var o in TemporalyObjects)
        {
            Destroy(o);
        }
        TemporalyObjects.Clear();

        foreach(var o in ShootableObjects)
        {
            o.Reset();
        }
    }

    public virtual void StartStage()
    {
        Debug.Log("Starting stage");
    }

    void RoomStyleChanger()
    {
        CurrentStyle = (RoomStyle)new System.Random().Next(3);
    }

    void ChangeDecoration()
    {
        foreach(var d in decorationChangers)
        {
            if(d != null)
            {
                d.DecorationChange(CurrentStyle); 
            }
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        decorationChangers = GetComponentsInChildren<DecorationChanger>(true).ToList();
    }
    #endif
}