using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//Il Tool per costruire le stanze
public class HomeBuilderWindow : EditorWindow
{
    //creiamo una finestra apribile dai menù in alto
    [MenuItem("Tools/Home Builder")]
    public static void ShowWindow()
    {
        GetWindow<HomeBuilderWindow>("Home Builder Tool");
    }
    //immagaziniamo i dati del Prefab
    class CategoryData
    {
        public string Name;
        public string[] prefabPath;
        public string[] prefabName;
        public GameObject[] prefabAsset;
        public Mesh prefabMesh;
        public Material[] prefabMat;
        public Matrix4x4 localPrefabMatrix;
    }

    bool visibleArea = true;
    float areaSize = 20f;

    //Btns
    const string rootFolder = "Assets/Rooms Prefabs";
    readonly List<CategoryData> categories = new();
    CategoryData selectedCategory;

    //variabili relative alle stanze
    readonly List<GameObject> spawnedRooms = new();
    static GameObject container;
    static readonly List<CategoryData> roomParts = new();
    static GameObject selPrefab;

    float curRotY = 0f;
    Vector3 previewPos;
    Quaternion previewRot;
    bool isCurrentlySnapped = false;

    void OnEnable()
    {
        ScanFolders(); //fa lo scan per permettere la suddivisione in categorie delle stanze
        RefreshSpawnedRooms(); //Raccoglie le stanze già presenti in scena e le aggiunge nella lista delle stanze presenti
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
    void RefreshSpawnedRooms()
    {
        spawnedRooms.Clear();
        RoomController[] rooms = FindObjectsByType<RoomController>(FindObjectsSortMode.None);
        foreach (var room in rooms)
        {
            spawnedRooms.Add(room.gameObject);
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

        visibleArea = EditorGUILayout.Toggle("Visible Range", visibleArea); //visibilità dell'autosnap

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5f);

        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        areaSize = EditorGUILayout.FloatField("Range Auto-Snap", areaSize); //range dell'autosnap

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5f);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Label("Choose the numbers of doors", subTitle);

        GUILayout.Space(5f);
        //per ogni categoria trovate nello scan iniziale crea un pulsante correlato
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
        //istruzioni per scorciatoie
        GUILayout.Label("Scorciatoie\nShift+Q: Ruota prefab senso antiorario\nShift+E: Ruota prefab senso orario\nCtrl+Z: toglie l'ultima stanza inserita", subTitle);
    }

    void OnSceneGUI(SceneView sceneView)
    {
        OverlayBtns(); //Gestisce i bottoni nella SceneView
        InputAndPreview(sceneView); //Gestisce gli input e la Preview
    }
    void OverlayBtns()
    {
        float tumbSize = 80f;
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 100, Screen.height - 20)); //creo un'area apposta a schermo dove inserire i bottoni
        if (selectedCategory != null)
        {
            for (int i = 0; i < selectedCategory.prefabAsset.Length; i++)
            {
                GameObject prefab = selectedCategory.prefabAsset[i];
                Texture2D preview = AssetPreview.GetAssetPreview(prefab);
                GUIContent content;
                if (preview != null) content = new GUIContent(preview, selectedCategory.prefabName[i]);
                else content = new GUIContent(selectedCategory.prefabName[i]);
                if (GUILayout.Button(content, GUILayout.Width(tumbSize), GUILayout.Height(tumbSize))) //creo tanti bottoni quanti prefab nella categoria creata
                {
                    SelectedPrefab(prefab); //crea la preview Disegnata in scena della stanza selezionata
                }
            }
        }
        GUILayout.Space(10f);
        if (GUILayout.Button("Undo", GUILayout.Width(tumbSize), GUILayout.Height(tumbSize)))
        {
            if (spawnedRooms.Count > 0)
            {
                UndoBtn(); //pulsante di Undo
            }
        }
        GUILayout.EndArea();
        Handles.EndGUI();
    }
    void UndoBtn()
    {
        spawnedRooms.RemoveAll(x => x == null);
        if (spawnedRooms.Count > 0)
        {
            GameObject lastRoom = spawnedRooms[spawnedRooms.Count - 1];
            spawnedRooms.RemoveAt(spawnedRooms.Count - 1);
            Undo.DestroyObjectImmediate(lastRoom);
        }
        GUIUtility.ExitGUI();
    }
    void InputAndPreview(SceneView sceneView)
    {
        Event e = Event.current;
        #region Rotate Preview 
        if (e.type == EventType.KeyDown && e.shift) //scorciatoie per ruotare la preView 
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
        #endregion
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Plane ground = new(Vector3.up, Vector3.zero);
        if (ground.Raycast(ray, out float distance)) //Plane Cast per far sì che la preview rimanga sullo stesso piano, quello colpito
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Quaternion baseRot = Quaternion.Euler(0, curRotY, 0);

            isCurrentlySnapped = CalculateSnap(hitPoint, baseRot, out previewPos, out previewRot); //bool per controllare se è Snappato

            DrawVisuals(sceneView); //disegna la preView

            if (e.type == EventType.MouseDown && e.button == 0) //permette di Istanziare il prefab solo se è snappata la preView
            {
                if (!isCurrentlySnapped)
                {
                    e.Use();
                    return;
                }
                ContainerCheck();
                GameObject gObjSpawned = (GameObject)PrefabUtility.InstantiatePrefab(selPrefab, container.transform);
                gObjSpawned.transform.SetPositionAndRotation(previewPos, previewRot);

                Undo.RegisterCreatedObjectUndo(gObjSpawned, "Spawn Prefab");
                spawnedRooms.Add(gObjSpawned);
                UpdateDoorsStatus(gObjSpawned);
                e.Use();
            }

        }
        sceneView.Repaint();
    }
    bool CalculateSnap(Vector3 basePos, Quaternion baseRot, out Vector3 finalPos, out Quaternion finalRot)
    {
        finalPos = basePos;
        finalRot = baseRot;

        if (selPrefab == null) return false;

        RoomController selRoomController = selPrefab.GetComponent<RoomController>();
        if (selRoomController == null || selRoomController.doors.Count == 0) return false;

        spawnedRooms.RemoveAll(x => x == null);

        float closestDist = float.MaxValue;
        bool foundSnap = false;

        foreach (var selRoomDoor in selRoomController.doors) //per ogni porta all'interno della stanza che abbiamo selezionando
        {
            if (selRoomDoor.doorCollider == null) continue;
            //ne calcola la rotazione e la posizione locale 
            Quaternion localDoorRot = Quaternion.Inverse(selPrefab.transform.rotation) * selRoomDoor.doorCollider.transform.rotation;
            Vector3 localDoorPos = Quaternion.Inverse(selPrefab.transform.rotation) * (selRoomDoor.doorCollider.transform.position - selPrefab.transform.position);

            Vector3 currentPreviewDoorWorldPos = basePos + (baseRot * localDoorPos);
            Vector3 currentPreviewDoorForward = baseRot * localDoorRot * Vector3.forward;
            foreach (var exRoomObj in spawnedRooms) //per ogni stanza già presente
            {
                RoomController exRoom = exRoomObj.GetComponent<RoomController>(); //ricava il Room Controller
                if (exRoom == null) continue;
                foreach (var targetDoor in exRoom.doors) //per ogni porta di queste stanze
                {
                    if (targetDoor.doorCollider == null || targetDoor.isUsed) continue;

                    //calcola la distanza tra essa e la posizione di quella in Preview
                    float dist = Vector3.Distance(currentPreviewDoorWorldPos, targetDoor.doorCollider.transform.position); 
                    //in modo da poterla snappare con più precisione (prende in considerazione il raggio dell'auto-snap, la distanza minima e l'allineamento tra le 2 porte)
                    if (dist <= areaSize && dist < closestDist && Vector3.Dot(currentPreviewDoorForward, targetDoor.doorCollider.transform.forward) < -0.7f)
                    {
                        Quaternion desiredDoorRot = Quaternion.LookRotation(-targetDoor.doorCollider.transform.forward, Vector3.up); 
                        Quaternion desiredRootRot = desiredDoorRot * Quaternion.Inverse(localDoorRot);
                        Vector3 desiredRootPos = targetDoor.doorCollider.transform.position - (desiredRootRot * localDoorPos);

                        closestDist = dist;
                        finalPos = desiredRootPos;
                        finalRot = desiredRootRot;
                        foundSnap = true;
                    }
                }
            }
        }
        return foundSnap;
    }
    void DrawVisuals(SceneView sceneView)
    {
        if (visibleArea) //disegna il disco colorato 
        {
            Handles.color = isCurrentlySnapped ? Color.purple : Color.green;
            Handles.DrawWireDisc(previewPos, Vector3.up, areaSize);
        }
        if (selPrefab != null && roomParts.Count > 0) //disegna la preView
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Matrix4x4 matrix = Matrix4x4.TRS(previewPos, previewRot, selPrefab.transform.localScale); //ne calcola la matrice
            foreach (var piece in roomParts) //per ogni pezzo ne calcola la matice personale per disegnare il prefab completo
            {
                if (piece.prefabMesh == null || piece.prefabMat == null) continue;
                Matrix4x4 finalMatrix = matrix * piece.localPrefabMatrix;
                for (int i = 0; i < piece.prefabMat.Length; i++)
                {
                    Graphics.DrawMesh(piece.prefabMesh, finalMatrix, piece.prefabMat[i], 0, sceneView.camera, i);
                }
            }
        }
    }
    void UpdateDoorsStatus(GameObject newlySpawnedRoom)
    {
        RoomController newRooms = newlySpawnedRoom.GetComponent<RoomController>();
        if (newRooms == null) return;
        foreach (var exRoomObj in spawnedRooms)
        {
            RoomController exRoom = exRoomObj.GetComponent<RoomController>();
            if (exRoom == newRooms || exRoom == null) continue;
            foreach (var targetDoor in exRoom.doors)
            {
                if (targetDoor.doorCollider == null || targetDoor.isUsed) continue;
                foreach (var newDoor in newRooms.doors)
                {
                    if (newDoor.doorCollider == null) continue;
                    if (Vector3.Distance(newDoor.doorCollider.transform.position, targetDoor.doorCollider.transform.position) < 0.05f)
                    {

                        Undo.RecordObject(exRoom, "Update Doors");
                        Undo.RecordObject(newRooms, "Update Doors");
                        targetDoor.isUsed = true;
                        newDoor.isUsed = true;
                    }
                }
            }
        }
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
                CategoryData piece = new()
                {
                    prefabMesh = pf.sharedMesh,
                    prefabMat = pieceRenderers.sharedMaterials,
                    localPrefabMatrix = _localMtrix
                };
                roomParts.Add(piece);
            }
        }
    }
}
