using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
[System.Serializable]
public class DoorsData
{
    public Collider doorCollider;
    public bool isUsed = false;
}

public class RoomController : MonoBehaviour
{
    public List<DoorsData> doors = new();
    public int GetDoorCount()
    {
        return doors.Count;
    }

    public List<DoorsData> GetUnusedDoors()
    {
        List<DoorsData> unusedDoors = new();
        foreach (var door in doors)
        {
            if (!door.isUsed) unusedDoors.Add(door);
        }
        return unusedDoors;
    }
    private void OnDrawGizmos()
    {
        if (doors == null) return;
        foreach (var door in doors)
        {
            if (door.doorCollider != null)
            {
                Color color = door.isUsed ? Color.red : Color.green;
                
                Handles.color = color;
                Handles.DrawWireCube(door.doorCollider.bounds.center, door.doorCollider.bounds.size);

                Handles.color = Color.blue;
                Handles.DrawLine(door.doorCollider.transform.position, door.doorCollider.transform.position + door.doorCollider.transform.forward * 1.5f);
            }
        }
    }
}
