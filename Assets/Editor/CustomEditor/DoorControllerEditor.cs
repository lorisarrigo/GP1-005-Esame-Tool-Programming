using UnityEditor;
using UnityEngine;


//un Custom Editor per le Stanze, tiene sott'occhio lo status delle stanze
[CustomEditor(typeof(RoomController))]
public class DoorControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RoomController room = (RoomController)target;

        GUILayout.Space(5f);

        GUILayout.Label("Door Controller", EditorStyles.boldLabel);

        GUILayout.Space(10f);

        GUILayout.Label($"Total Doors Found in this room: {room.doors.Count}");

        GUILayout.Space(10f);

        //se non abbiamo porte avvisa di inserirne almeno una
        if (room.doors == null || room.doors.Count == 0)
        {
            EditorGUILayout.HelpBox("This room has no doors. Add at least one door to this room", MessageType.Warning);
        }
        /* altrimenti:
         * controlla che ci siano i Collider;
         * se non ci sono avvisa che mancano;
         * e infine ritorna visivamente se o meno la stanza è disponibile
         */
        else
        {
            for (int i = 0; i < room.doors.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                string doorName = room.doors[i].doorCollider != null ? room.doors[i].doorCollider.name : "Missing Collider";
                EditorGUILayout.LabelField($"Door: {doorName}");

                if (room.doors[i].doorCollider == null)
                {
                    EditorGUILayout.EndVertical();
                    continue;
                }
                GUIStyle statusStyle = new()
                {
                    normal = { textColor = room.doors[i].isUsed ? Color.red : Color.green }
                };
                string status = room.doors[i].isUsed ? "X - Door Already in use" : "OK - Door ready To be Occupied";
                EditorGUILayout.LabelField(status, statusStyle);
                EditorGUILayout.EndVertical();
                GUILayout.Space(10f);
            }
        }
        DrawDefaultInspector(); //lascio l'inspector di base per poter inserire a piacimento le porte, altrimenti non riesco ad aggiungerne di nuove
    }
}
