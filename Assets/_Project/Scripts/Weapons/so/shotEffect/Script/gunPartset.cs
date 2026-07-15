using UnityEngine;
    [System.Serializable]
    public class GunPartSet
    {
        public GameObject part;  
        public GunPartAction actionData; 
        [HideInInspector] public Vector3 defaultPos;
        [HideInInspector] public Vector3 defaultRot;
        public void Init()
        {
            if (part == null) return;
            defaultPos = part.transform.localPosition;
            defaultRot = part.transform.localEulerAngles;
        }
    }