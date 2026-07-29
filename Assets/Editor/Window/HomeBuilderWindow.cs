using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
public class HomeBuilderWindow : EditorWindow
{
    [MenuItem("Tools/Home Builder")]
    public static void ShowWindow()
    {
        GetWindow<HomeBuilderWindow>("Home Builder Tool");
    }
    class CategoryData
    {
        public string Name;
        public string[] prefabPath;
        public string[] prefabName;
        public GameObject[] prefabAsset;
    }
    public class MeshData
    {
        public Mesh mesh;
        public Material[] mat;
        public Matrix4x4 localMatrix;
    }

    bool visibleArea = true;
    float areaSize = 20f;

    //Btns
    const string rootFolder = "Assets/Rooms Prefabs";
    readonly List<CategoryData> categories = new();
    static readonly List<MeshData> roomParts = new();
    CategoryData selectedCategory;
    static GameObject container;
    static readonly List<GameObject> spawnedRooms = new();
    static GameObject selPrefab;
    float curRotY = 0f;
    Mesh previewMesh;
    Vector3 previewPos;

    void OnEnable()
    {
        ScanFolders();
        SceneView.duringSceneGui += OnSceneGUI;
    }
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    void ScanFolders()
    {
        categories.Clear();
        selectedCategory = null;
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

            for (int i = 0; i < guids.Length; i++)
            {
                category.prefabPath[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
                category.prefabName[i] = Path.GetFileNameWithoutExtension(category.prefabPath[i]);
                category.prefabAsset[i] = AssetDatabase.LoadAssetAtPath<GameObject>(category.prefabPath[i]);
            }
            categories.Add(category);
        }
        if (categories.Count > 0)
        {
            selectedCategory = categories[0];
            SelectedPrefab(selectedCategory.prefabAsset[0]);
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

        GUIStyle subTitle = new()
        {
            fontSize = 14,
            normal = { textColor = Color.gray8 },
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        GUILayout.Label("Set the Range of the Auto-Snap \n and its visibility", subTitle);

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

        GUILayout.Label("Set the Nubers of doors", subTitle);

        GUILayout.Space(5f);

        foreach (CategoryData category in categories)
        {
            if (GUILayout.Button(category.Name))
            {
                selectedCategory = category;
                if (category.prefabAsset.Length > 0) SelectedPrefab(category.prefabAsset[0]);
            }
        }

        GUILayout.Label("", GUI.skin.horizontalSlider);

        GUILayout.Space(5f);
        
        GUILayout.Label("Scorciatoie\nShift+Q: Ruota prefab senso antiorario\nShift+E: Ruota prefab senso orario\nCtrl+Z: toglie l'ultima stanza inserita", subTitle);
    }

    void OnSceneGUI(SceneView sceneView)
    {
        OverlayBtns();
        InputAndPreview(sceneView);
    }
    void OverlayBtns()
    {
        float tumbSize = 80f;
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 100, Screen.height - 20));
        if (selectedCategory != null)
        {
            for (int i = 0; i < selectedCategory.prefabAsset.Length; i++)
            {
                GameObject prefab = selectedCategory.prefabAsset[i];
                Texture2D preview = AssetPreview.GetAssetPreview(prefab);
                GUIContent content;
                if (preview != null) content = new GUIContent(preview, selectedCategory.prefabName[i]);
                else content = new GUIContent(selectedCategory.prefabName[i]);
                if (GUILayout.Button(content, GUILayout.Width(tumbSize), GUILayout.Height(tumbSize)))
                {
                    SelectedPrefab(prefab);
                }
            }
        }
        GUILayout.Space(10f);
        if (GUILayout.Button("Undo", GUILayout.Width(tumbSize), GUILayout.Height(tumbSize)))
        {
            if (spawnedRooms.Count > 0)
            {
                ContainerCheck();
                spawnedRooms.RemoveAll(x => x == null);
                if (spawnedRooms.Count == 0 && container.transform.childCount > 0)
                {
                    foreach (Transform child in container.transform)
                    {
                        spawnedRooms.Add(child.gameObject);
                    }
                }
                if (spawnedRooms.Count > 0)
                {
                    GameObject lastRoom = spawnedRooms[spawnedRooms.Count - 1];
                    spawnedRooms.RemoveAt(spawnedRooms.Count - 1);
                    Undo.DestroyObjectImmediate(lastRoom);
                }
                GUIUtility.ExitGUI();
            }
        }
        GUILayout.EndArea();
        Handles.EndGUI();
    }
    void InputAndPreview(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.shift)
        {
            if (e.keyCode == KeyCode.Q)
            {
                curRotY -= 90;
                e.Use();
            }
            else if (e.keyCode == KeyCode.E)
            {
                curRotY += 90;
                e.Use();
            }
        }
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane ground = new(Vector3.up, Vector3.zero);
        if (ground.Raycast(ray, out float distance))
        {
            Quaternion rot = Quaternion.Euler(0, curRotY, 0);
            previewPos = ray.GetPoint(distance);
            if (visibleArea)
            {
                Handles.color = Color.green;
                Handles.DrawWireDisc(previewPos, Vector3.up, areaSize);
            }
            if (selPrefab != null && roomParts.Count > 0)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                
                Matrix4x4 matrix = Matrix4x4.TRS(previewPos, rot, selPrefab.transform.localScale);
                foreach (var piece in roomParts)
                {
                    if (piece.mesh == null || piece.mat == null) continue;
                    Matrix4x4 finalMatrix = matrix * piece.localMatrix;
                    for (int i = 0; i < piece.mat.Length; i++)
                    {
                        Graphics.DrawMesh(piece.mesh, finalMatrix, piece.mat[i], 0, sceneView.camera, i);
                    }
                }
            }
            
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                ContainerCheck();
                GameObject gObjSpawned = (GameObject)PrefabUtility.InstantiatePrefab(selPrefab, container.transform);
                gObjSpawned.transform.position = previewPos;
                gObjSpawned.transform.rotation = rot;
                Undo.RegisterCreatedObjectUndo(gObjSpawned, "Spawn Prefab");
                spawnedRooms.Add(gObjSpawned);
                e.Use();
            }
        }
        sceneView.Repaint();
    }
    static void ContainerCheck()
    {
        if (container != null) container = GameObject.Find("Generated Props");
        else container = new GameObject("Generated Props");
    }
    static void SelectedPrefab(GameObject prefab)
    {
        selPrefab = prefab;
        roomParts.Clear();
        if (prefab == null) return;
        MeshFilter[] pieceFilters = prefab.GetComponentsInChildren<MeshFilter>();

        foreach (var pf in pieceFilters)
        {
            MeshRenderer pieceRenderers = pf.GetComponent<MeshRenderer>();

            if (pf.sharedMesh != null && pieceRenderers != null)
            {
                Matrix4x4 _localMtrix = prefab.transform.worldToLocalMatrix * pf.transform.localToWorldMatrix;
                MeshData piece = new MeshData
                {
                    mesh = pf.sharedMesh,
                    mat = pieceRenderers.sharedMaterials,
                    localMatrix = _localMtrix
                };
                roomParts.Add(piece);
            }
        }
    }
}
