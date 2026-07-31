using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(RoomController))]
public class DoorControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RoomController room = (RoomController)target;
        GUILayout.Space(5f);

        GUILayout.Label("Door Controller", EditorStyles.boldLabel);

        GUILayout.Space(10f);

        GUILayout.Label($"Total Doors Found in this room: {room.GetDoorCount()}");

        GUILayout.Space(10f);

        if (room.doors == null || room.doors.Count == 0)
        {
            EditorGUILayout.HelpBox("This room has no doors. Add at least one door to this room", MessageType.Warning);
        }
        else
        {
            for (int i = 0; i < room.doors.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                string doorName = room.doors[i].doorCollider != null ? room.doors[i].doorCollider.name : "Missing Collider";
                EditorGUILayout.LabelField($"Door: {doorName}");

                GUIStyle statusStyle = new()
                {
                    normal = { textColor = room.doors[i].isUsed ? Color.red : Color.green }
                };
                if (room.doors[i].doorCollider == null)
                {
                    EditorGUILayout.EndVertical();
                    continue;
                }
                string status = room.doors[i].isUsed ? "X - Door Already in use" : "OK - Door ready To be Occupied";
                EditorGUILayout.LabelField(status, statusStyle);
                EditorGUILayout.EndVertical();
                GUILayout.Space(10f);
            }
        }
        DrawDefaultInspector();
    }
}
