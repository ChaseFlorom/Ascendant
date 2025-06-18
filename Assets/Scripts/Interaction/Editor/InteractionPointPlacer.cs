using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class InteractionPointPlacer : EditorWindow
{
    private Tilemap targetTilemap;
    private ActionGraph defaultActionGraph;
    private bool isPlacing = false;
    private Vector3Int lastHoveredCell = Vector3Int.zero;
    
    [MenuItem("Tools/Interaction Point Placer")]
    public static void OpenWindow()
    {
        GetWindow<InteractionPointPlacer>("Interaction Point Placer");
    }
    
    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    void OnGUI()
    {
        EditorGUILayout.LabelField("Interaction Point Placer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        targetTilemap = (Tilemap)EditorGUILayout.ObjectField("Target Tilemap", targetTilemap, typeof(Tilemap), true);
        defaultActionGraph = (ActionGraph)EditorGUILayout.ObjectField("Default Action Graph", defaultActionGraph, typeof(ActionGraph), false);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Start Placing"))
        {
            isPlacing = true;
            SceneView.RepaintAll();
        }
        
        if (GUILayout.Button("Stop Placing"))
        {
            isPlacing = false;
            SceneView.RepaintAll();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Instructions:\n1. Set your target tilemap\n2. Optionally set a default action graph\n3. Click 'Start Placing'\n4. Click in the Scene view to place interaction points\n5. Right-click on placed points to edit them", MessageType.Info);
    }
    
    void OnSceneGUI(SceneView sceneView)
    {
        if (!isPlacing || targetTilemap == null) return;
        
        Event e = Event.current;
        Vector2 mousePos = e.mousePosition;
        
        // Convert mouse position to world position
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
        Vector3 worldPos = ray.origin;
        worldPos.z = targetTilemap.transform.position.z;
        
        // Convert to tile position
        Vector3Int cellPos = targetTilemap.WorldToCell(worldPos);
        Vector3 cellCenter = targetTilemap.CellToWorld(cellPos) + targetTilemap.cellSize * 0.5f;
        
        // Draw preview
        if (cellPos != lastHoveredCell)
        {
            lastHoveredCell = cellPos;
            SceneView.RepaintAll();
        }
        
        Handles.color = Color.yellow;
        Handles.DrawWireCube(cellCenter, Vector3.one * 0.8f);
        
        // Handle mouse clicks
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            CreateInteractionPoint(cellCenter);
            e.Use();
        }
        
        // Handle right-click for context menu
        if (e.type == EventType.MouseDown && e.button == 1)
        {
            ShowContextMenu(cellCenter);
            e.Use();
        }
    }
    
    void CreateInteractionPoint(Vector3 position)
    {
        GameObject interactionPoint = new GameObject("InteractionPoint");
        interactionPoint.transform.position = position;
        
        InteractionPoint component = interactionPoint.AddComponent<InteractionPoint>();
        if (defaultActionGraph != null)
        {
            component.actionGraph = defaultActionGraph;
        }
        
        // Parent to a container if it exists
        GameObject container = GameObject.Find("InteractionPoints");
        if (container == null)
        {
            container = new GameObject("InteractionPoints");
        }
        interactionPoint.transform.SetParent(container.transform);
        
        // Select the new object
        Selection.activeGameObject = interactionPoint;
        
        Debug.Log($"Created interaction point at {position}");
    }
    
    void ShowContextMenu(Vector3 position)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Interaction Point"), false, () => CreateInteractionPoint(position));
        menu.AddItem(new GUIContent("Create with Default Graph"), false, () => {
            GameObject go = new GameObject("InteractionPoint");
            go.transform.position = position;
            InteractionPoint ip = go.AddComponent<InteractionPoint>();
            if (defaultActionGraph != null) ip.actionGraph = defaultActionGraph;
            
            GameObject container = GameObject.Find("InteractionPoints");
            if (container == null) container = new GameObject("InteractionPoints");
            go.transform.SetParent(container.transform);
            
            Selection.activeGameObject = go;
        });
        menu.ShowAsContext();
    }
} 