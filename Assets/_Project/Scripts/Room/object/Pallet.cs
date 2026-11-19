using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pallet : ShootableObject
{
    [SerializeField] private List<Collider> boards;
    [SerializeField] private List<Rigidbody> others;

    [System.Serializable]
    private class PartData
    {
        public Rigidbody rb;
        public Vector3 position;
        public Quaternion rotation;
        public bool isKinematic;
    }

    private List<PartData> partDataList = new List<PartData>();
    private HashSet<Collider> removedBoardSet = new HashSet<Collider>();

    private int removedBoards = 0;

    private void Start()
    {
        foreach (var b in boards)
        {
            SavePart(b.GetComponent<Rigidbody>());
        }

        foreach (var r in others)
        {
            SavePart(r);
        }
    }

    private void SavePart(Rigidbody rb)
    {
        if (rb == null) return;

        partDataList.Add(new PartData
        {
            rb = rb,
            position = rb.position,
            rotation = rb.rotation,
            isKinematic = rb.isKinematic
        });
    }

    public override void Attacked(Collider hitCollider,Vector3 AttackedDirection,float AttackedForce)
    {
        if (!boards.Contains(hitCollider)) return;
        if (!removedBoardSet.Add(hitCollider)) return;

        Rigidbody rb = hitCollider.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(AttackedDirection * AttackedForce , ForceMode.Impulse);
        }
        removedBoards++;

        if (removedBoards > boards.Count / 2)
        {
            foreach(var r in others)
            {
                r.isKinematic = false;
            }

            foreach(var b in boards)
            {
                Rigidbody rib = b.GetComponent<Rigidbody>();
                rib.isKinematic = false;
            }
        }
    }

    public void BreakAll(Vector3 explosionCenter, float explosionRadius, float explosionForce)
    {
        foreach(var r in others)
        {
            if (r != null)
            {
                r.isKinematic = false;
                r.AddExplosionForce(explosionForce, explosionCenter, explosionRadius, 1f, ForceMode.Impulse);
            }
        }

        foreach(var b in boards)
        {
            if (b != null)
            {
                Rigidbody rib = b.GetComponent<Rigidbody>();
                if (rib != null)
                {
                    rib.isKinematic = false;
                    rib.AddExplosionForce(explosionForce, explosionCenter, explosionRadius, 1f, ForceMode.Impulse);
                }
            }
        }
        
        removedBoards = boards.Count;
    }

    public override void Reset()
    {
        foreach (var part in partDataList)
        {
            if (part.rb == null) continue;

            part.rb.isKinematic = true;

            part.rb.linearVelocity = Vector3.zero;
            part.rb.angularVelocity = Vector3.zero;

            part.rb.position = part.position;
            part.rb.rotation = part.rotation;

            part.rb.isKinematic = part.isKinematic;
        }

        removedBoardSet.Clear();
        removedBoards = 0;
    }
}