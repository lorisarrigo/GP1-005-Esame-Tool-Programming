using UnityEditor;
using UnityEngine;

public class HomeBuilderWindow : EditorWindow
{
    [MenuItem("Tools/Home Builder")]
    public static void ShowWindow()
    {
        GetWindow<HomeBuilderWindow>("Home Builder Tool");
    }
    bool visibleArea = true;
    float areaSize = 20f;
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnScenGUI;
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnScenGUI;
    }
    private void OnGUI()
    {
        GUILayout.Space(10f);

        GUIStyle titleStyle = new()
        {
            fontSize = 18,
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        GUILayout.Label("Home Builder", titleStyle);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Space(5f);

        GUIStyle subTytle = new()
        {
            fontSize = 14,
            normal = { textColor = Color.gray8 },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        GUILayout.Label("Set the Range of the Auto-Snap \n and its visibility", subTytle);

        GUILayout.Space(10f);

        EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            visibleArea = EditorGUILayout.Toggle("Visible Range", visibleArea);

            GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5f);

        EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            areaSize = EditorGUILayout.FloatField("Range Auto-Snap", areaSize);

            GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5f);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Label("Set the Nubers of doors", subTytle);

        GUILayout.Space(5f);

        if (GUILayout.Button("1 Door")) { }
        
        if (GUILayout.Button("2 Doors")) { }

        if (GUILayout.Button("3 Doors")) { }

    }
    void OnScenGUI(SceneView sceneView)
    {
        if (!visibleArea) return;
        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(hit.point, hit.normal, areaSize);
        }
    }
}
