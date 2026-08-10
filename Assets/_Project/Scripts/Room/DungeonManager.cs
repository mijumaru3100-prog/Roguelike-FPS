using UnityEngine;
public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;
    void Awake() => Instance = this;
    [Header("部屋情報")]
    public GameObject[] battleRooms;
    public GameObject[] eliteRooms;
    public GameObject[] shopRooms;
    public GameObject[] treasureRooms;
    public GameObject[] bossRooms;
    [Header("現在の部屋")]
    public GameObject currentRoom;
    public Stage currentRoomScript;
    [Header("UI")]
    public FloorCount FloorCount;

    public int currentFloor = 0;     
    public int bossFloor = 5;

    public enum roomType
    {
        battle,
        elite,
        shop,
        treasure,
        boss
    }

public roomType GetNextRoomType()
{
    int nextFloor = currentFloor + 1;

    if (nextFloor >= bossFloor)
    {
        return roomType.boss;
    }

    roomType[] randomTypes = { roomType.battle, roomType.elite, roomType.shop };
    return randomTypes[Random.Range(0, randomTypes.Length)];
}

    public void AdvanceFloor()
{
    currentFloor++;
}

    public GameObject SelectRoom(roomType type)
    {
        GameObject[] targetRoom = null;
        switch (type)
        {
            case roomType.battle:
                targetRoom = battleRooms;
                break;
            case roomType.elite:
                targetRoom = eliteRooms;
                break;
            case roomType.shop:
                targetRoom = shopRooms;
                break;
            case roomType.treasure:
                targetRoom = treasureRooms;
                break;
            case roomType.boss:
                targetRoom = bossRooms;
                break;
        }
        if (targetRoom == null || targetRoom.Length == 0) return null;
        int idx = Random.Range(0, targetRoom.Length);
        currentRoom = targetRoom[idx];
        currentRoomScript = currentRoom.GetComponent<Stage>();
        return currentRoom;
    }
}