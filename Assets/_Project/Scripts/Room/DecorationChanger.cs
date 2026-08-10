using UnityEngine;

public enum RoomStyle { gorgeous, broken ,religion}

public class DecorationChanger : MonoBehaviour
{
    public GameObject gorgeousObjects;
    public GameObject brokenObjects;
    public GameObject religionobjects;

    public void DecorationChange(RoomStyle style)
    {
        if (gorgeousObjects != null)
        {
            gorgeousObjects.SetActive(style == RoomStyle.gorgeous);
        }

        if (brokenObjects != null)
        {
            brokenObjects.SetActive(style == RoomStyle.broken);
        }

        if(religionobjects != null)
        {
            religionobjects.SetActive(style == RoomStyle.religion);
        }
    }
}
