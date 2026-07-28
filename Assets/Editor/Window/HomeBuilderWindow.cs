using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
public class HomeBuilderWindow : EditorWindow
{
    [MenuItem("Tools/Home Builder")]
    public static void ShowWindow()
    {
        GetWindow<HomeBuilderWindow>("Home Builder Tool");
    }
    private class CategoryData
    {
        public string Name;
        public string[] prefabPath;
        public string[] prefabName;
        public GameObject[] prefabAsset;
    }
    bool visibleArea = true;
    float areaSize = 20f;
    
    //Btns
    bool visibleRommsBtns = true;
    
    static readonly List<CategoryData> categories = new();
    const string rootFolder = "Assets/Rooms Prefabs";

    private static GameObject container;
    private static GameObject selPrefab;
    private static string selName;
    private static Mesh previewMesh;
    private static Material[] previewMat;
    private static Vector3 previewPos;
    private static Vector3 previewScale;

    void OnEnable()
    {
        ScanFolders();
        SceneView.duringSceneGui += OnSnapArea;
        SceneView.duringSceneGui += OnRoomsSelection;
    }
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSnapArea;
        SceneView.duringSceneGui -= OnRoomsSelection;
    }
    static void ScanFolders()
    {
        categories.Clear();

        if (!AssetDatabase.IsValidFolder(rootFolder)) return;

        string fullRootPath = Path.GetFullPath(rootFolder);

        string[] Dirs = Directory.GetDirectories(fullRootPath);

        foreach (string dir in Dirs)
        {
            string folderName = Path.GetFileName(dir);
            string assetFolderPath = rootFolder + "/" + folderName;
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { assetFolderPath });
            if (guids.Length == 0) continue;

            var category = new CategoryData
            {
                Name = folderName,
                prefabPath = new string[guids.Length],
                prefabName = new string[guids.Length],
                prefabAsset = new GameObject[guids.Length]
            };

            for (int i = 0; i<guids.Length; i++)
            {
                category.prefabPath[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
                category.prefabName[i] = Path.GetFileNameWithoutExtension(category.prefabPath[i]);
                category.prefabAsset[i] = AssetDatabase.LoadAssetAtPath<GameObject>(category.prefabPath[i]);
            }
            categories.Add(category);
        }
    }
    void OnGUI()
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

        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        visibleRommsBtns = EditorGUILayout.Toggle("Visible Selection", visibleRommsBtns);

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5f);

        if (GUILayout.Button("1 Door")) 
        {
            ScanFolders();
        }
        
        if (GUILayout.Button("2 Doors")) 
        {
            ScanFolders();
        }

        if (GUILayout.Button("3 Doors")) 
        {
            ScanFolders();
        }

    }
    void OnSnapArea(SceneView sceneView)
    {
        if (!visibleArea) return;
        
        GUIStyle btnStyle = new()
        {
            fixedHeight = 80,
            fixedWidth = 140
        };

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(hit.point, hit.normal, areaSize);

            if (selPrefab == null || previewMesh == null) return;
            previewPos = hit.point;
            Matrix4x4 matrix = Matrix4x4.TRS(previewPos, Quaternion.identity, previewScale);
            
            for (int i = 0; i < previewMat.Length; i++)
            {
                Graphics.DrawMesh(previewMesh, matrix, previewMat[i], 0, sceneView.camera, i);
            }
        }
        if(e.type == EventType.MouseDown && e.button == 0)
        {
            ContainerCheck();
            GameObject gObjSpawned = PrefabUtility.InstantiatePrefab(selPrefab, container.transform) as GameObject;
            gObjSpawned.transform.position = previewPos;

            Undo.RegisterCreatedObjectUndo(gObjSpawned, "Spawn Prefab");
            e.Use();
        }
        sceneView.Repaint();
    }
    static void ContainerCheck()
    {
        if (container != null) container = GameObject.Find("Generated Props");
        else container = new("Generated Props");
    }
    void OnRoomsSelection(SceneView sceneView)
    {
        //foreach ()
        //if (GUILayout.Button("Room A")) SelectedPrefab();
        //if (GUILayout.Button("Room B")) SelectedPrefab();
        //if (GUILayout.Button("Room C")) SelectedPrefab();
    }
    static void SelectedPrefab(GameObject prefab, string name)
    {
        selPrefab = prefab;
        selName = name;
        var meshFilters = prefab.GetComponentInChildren<MeshFilter>();
        var meshRenderers = prefab.GetComponentInChildren<MeshRenderer>();

        if(meshFilters != null && meshRenderers != null)
        {
            previewMesh = meshFilters.sharedMesh;
            previewMat = meshRenderers.sharedMaterials;
            previewScale = prefab.transform.localScale;
        }
    }    
}
