using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System;

public class ActionGraphNodeEditorWindow : EditorWindow
{
    private ActionGraph currentGraph;
    private Vector2 panOffset;
    private float zoom = 1f;
    private List<BaseNode> nodes = new List<BaseNode>();
    private List<Connection> connections = new List<Connection>();
    private BaseNode selectedNode = null;
    private bool isMakingConnection = false;
    private BaseNode connectionStartNode = null;
    private Vector2 connectionDragPos;
    private BaseNode nodeBeingDragged = null;
    private Vector2 nodeDragOffset;

    private const float NODE_WIDTH = 200f;
    private const float NODE_HEIGHT = 80f;
    private const float PORT_RADIUS = 12f;
    private const float NODE_CORNER_RADIUS = 18f;
    private const float NODE_LABEL_MARGIN = 12f;
    private const float NODE_SHADOW_OFFSET = 4f;

    [MenuItem("Tools/Action Graph Node Editor")]
    public static void OpenWindow()
    {
        GetWindow<ActionGraphNodeEditorWindow>("Action Graph Node Editor");
    }

    [OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID) as ActionGraph;
        if (obj != null)
        {
            var window = GetWindow<ActionGraphNodeEditorWindow>();
            window.LoadGraph(obj);
            return true;
        }
        return false;
    }

    private void LoadGraph(ActionGraph graph)
    {
        currentGraph = graph;
        nodes.Clear();
        connections.Clear();
        if (graph.startNode == null)
        {
            var startNode = CreateInstance<StartNode>();
            startNode.name = "StartNode";
            startNode.editorPosition = new Vector2(100, 200);
            AssetDatabase.AddObjectToAsset(startNode, graph);
            AssetDatabase.SaveAssets();
            graph.startNode = startNode;
            EditorUtility.SetDirty(graph);
        }
        Queue<InteractionNode> queue = new Queue<InteractionNode>();
        HashSet<InteractionNode> visited = new HashSet<InteractionNode>();
        queue.Enqueue(graph.startNode);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node == null || visited.Contains(node)) continue;
            visited.Add(node);
            if (node is StartNode sn) nodes.Add(new BaseNode(sn));
            else if (node is DebugLogNode dn) nodes.Add(new BaseNode(dn));
            if (node.nextNode != null) queue.Enqueue(node.nextNode);
        }
        foreach (var n in nodes)
        {
            if (n.node.nextNode != null)
            {
                var target = nodes.Find(x => x.node == n.node.nextNode);
                if (target != null)
                {
                    connections.Add(new Connection(n, target));
                }
            }
        }
    }

    private void OnGUI()
    {
        DrawToolbar();
        if (currentGraph == null)
        {
            EditorGUILayout.HelpBox("No ActionGraph loaded. Open or double-click an ActionGraph asset.", MessageType.Info);
            return;
        }
        DrawGrid(); // Draw grid first, behind everything
        DrawConnections();
        DrawNodes();
        DrawNodeContextMenu();
        ProcessEvents(Event.current);
        if (GUI.changed) Repaint();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Open ActionGraph...", EditorStyles.toolbarButton))
        {
            var path = EditorUtility.OpenFilePanel("Open ActionGraph", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                var relPath = "Assets" + path.Substring(Application.dataPath.Length);
                var graph = AssetDatabase.LoadAssetAtPath<ActionGraph>(relPath);
                if (graph != null) LoadGraph(graph);
            }
        }
        GUILayout.Label(currentGraph ? currentGraph.name : "No Graph", EditorStyles.toolbarButton);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawGrid()
    {
        // Subtle, nearly transparent grid
        var gridColor = new Color(0.2f, 0.2f, 0.2f, 0.08f);
        Handles.BeginGUI();
        Handles.color = gridColor;
        float gridSpacing = 32 * zoom;
        for (float x = 0; x < position.width; x += gridSpacing)
            Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, position.height, 0));
        for (float y = 0; y < position.height; y += gridSpacing)
            Handles.DrawLine(new Vector3(0, y, 0), new Vector3(position.width, y, 0));
        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes()
    {
        foreach (var node in nodes)
        {
            Rect nodeRect = new Rect(node.node.editorPosition, new Vector2(NODE_WIDTH, NODE_HEIGHT));
            // Draw node shadow
            EditorGUI.DrawRect(new Rect(nodeRect.x + NODE_SHADOW_OFFSET, nodeRect.y + NODE_SHADOW_OFFSET, nodeRect.width, nodeRect.height), new Color(0,0,0,0.18f));
            // Draw node background (solid, opaque, with border)
            Color nodeColor = node.node is StartNode ? new Color(0.35f,0.65f,1f,1f) :
                              node.node is DebugLogNode ? new Color(1f,0.55f,0.15f,1f) : // orange for debug
                              new Color(0.22f,0.22f,0.22f,1f);
            EditorGUI.DrawRect(nodeRect, nodeColor);
            // Draw border
            Handles.color = node.node is StartNode ? new Color(0.2f,0.5f,1f,1f) :
                              node.node is DebugLogNode ? new Color(1f,0.4f,0.1f,1f) :
                              new Color(0.1f,0.1f,0.1f,1f);
            Handles.DrawAAPolyLine(4f, new Vector3[] {
                new Vector3(nodeRect.x, nodeRect.y),
                new Vector3(nodeRect.x + nodeRect.width, nodeRect.y),
                new Vector3(nodeRect.x + nodeRect.width, nodeRect.y + nodeRect.height),
                new Vector3(nodeRect.x, nodeRect.y + nodeRect.height),
                new Vector3(nodeRect.x, nodeRect.y)
            });
            Handles.color = Color.white;
            // Draw node label, centered and bold
            var labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                normal = { textColor = Color.white }
            };
            Rect labelRect = new Rect(nodeRect.x + NODE_LABEL_MARGIN, nodeRect.y + NODE_LABEL_MARGIN, nodeRect.width - 2 * NODE_LABEL_MARGIN, nodeRect.height - 2 * NODE_LABEL_MARGIN);
            GUI.Label(labelRect, node.node.name, labelStyle);
            // Draw ports as circles
            if (node.node is StartNode)
            {
                DrawPortCircle(nodeRect, true, false);
            }
            else if (node.node is DebugLogNode)
            {
                DrawPortCircle(nodeRect, true, true);
            }
            // Node selection (on body click)
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Vector2 outPos = new Vector2(nodeRect.xMax, nodeRect.center.y);
                Vector2 inPos = new Vector2(nodeRect.xMin, nodeRect.center.y);
                // Output port: allow disconnect and drag
                if (Vector2.Distance(Event.current.mousePosition, outPos) < PORT_RADIUS)
                {
                    // Remove existing connection from this output
                    connections.RemoveAll(c => c.from == node);
                    node.node.nextNode = null;
                    isMakingConnection = true;
                    connectionStartNode = node;
                    Event.current.Use();
                    return;
                }
                // Input port: allow disconnect and drag
                if (Vector2.Distance(Event.current.mousePosition, inPos) < PORT_RADIUS)
                {
                    // Remove existing connection to this input
                    var conn = connections.Find(c => c.to == node);
                    if (conn != null)
                    {
                        conn.from.node.nextNode = null;
                        connections.Remove(conn);
                        isMakingConnection = true;
                        connectionStartNode = conn.from;
                        Event.current.Use();
                        return;
                    }
                }
                // Node body selection
                if (nodeRect.Contains(Event.current.mousePosition))
                {
                    selectedNode = node;
                    GUI.FocusControl(null);
                    Repaint();
                }
            }
        }
    }

    private void DrawPortCircle(Rect nodeRect, bool output, bool input)
    {
        if (output)
        {
            Vector2 outPos = new Vector2(nodeRect.xMax, nodeRect.center.y);
            EditorGUI.DrawRect(new Rect(outPos.x - PORT_RADIUS, outPos.y - PORT_RADIUS, PORT_RADIUS * 2, PORT_RADIUS * 2), Color.clear);
            Handles.color = Color.green;
            Handles.DrawSolidDisc(outPos, Vector3.forward, PORT_RADIUS * 0.7f);
        }
        if (input)
        {
            Vector2 inPos = new Vector2(nodeRect.xMin, nodeRect.center.y);
            EditorGUI.DrawRect(new Rect(inPos.x - PORT_RADIUS, inPos.y - PORT_RADIUS, PORT_RADIUS * 2, PORT_RADIUS * 2), Color.clear);
            Handles.color = Color.cyan;
            Handles.DrawSolidDisc(inPos, Vector3.forward, PORT_RADIUS * 0.7f);
        }
        Handles.color = Color.white;
    }

    private void DrawConnections()
    {
        int removeIndex = -1;
        for (int i = 0; i < connections.Count; i++)
        {
            var conn = connections[i];
            Vector2 fromPos = conn.from.node.editorPosition + new Vector2(NODE_WIDTH, NODE_HEIGHT / 2);
            Vector2 toPos = conn.to.node.editorPosition + new Vector2(0, NODE_HEIGHT / 2);
            // Draw drop shadow
            Handles.color = Color.black;
            Handles.DrawBezier(
                fromPos + Vector2.one * 2f,
                toPos + Vector2.one * 2f,
                fromPos + new Vector2(50, 0) + Vector2.one * 2f,
                toPos + new Vector2(-50, 0) + Vector2.one * 2f,
                Color.black, null, 10f);
            // Draw main line
            Handles.color = Color.yellow;
            Handles.DrawBezier(
                fromPos,
                toPos,
                fromPos + new Vector2(50, 0),
                toPos + new Vector2(-50, 0),
                Color.yellow, null, 8f);
            Handles.color = Color.white;
            // Detect right-click on connection
            if (Event.current.type == EventType.ContextClick)
            {
                Vector2 mouse = Event.current.mousePosition;
                float dist = HandleUtility.DistancePointBezier(mouse, fromPos, toPos, fromPos + new Vector2(50, 0), toPos + new Vector2(-50, 0));
                if (dist < 10f)
                {
                    removeIndex = i;
                    Event.current.Use();
                }
            }
        }
        if (removeIndex >= 0)
        {
            // Remove connection and clear nextNode
            var conn = connections[removeIndex];
            conn.from.node.nextNode = null;
            connections.RemoveAt(removeIndex);
            EditorUtility.SetDirty(currentGraph);
        }
        // Draw connection in progress (magnet snap)
        if (isMakingConnection && connectionStartNode != null)
        {
            Vector2 fromPos = connectionStartNode.node.editorPosition + new Vector2(NODE_WIDTH, NODE_HEIGHT / 2);
            Vector2 mousePos = Event.current.mousePosition;
            Vector2 snapPos = mousePos;
            // Check if near any input port
            foreach (var node in nodes)
            {
                if (node == connectionStartNode) continue;
                Rect nodeRect = new Rect(node.node.editorPosition, new Vector2(NODE_WIDTH, NODE_HEIGHT));
                Vector2 inPos = new Vector2(nodeRect.xMin, nodeRect.center.y);
                if (Vector2.Distance(mousePos, inPos) < PORT_RADIUS * 1.5f)
                {
                    snapPos = inPos;
                    // Highlight the input port
                    Handles.color = Color.yellow;
                    Handles.DrawSolidDisc(inPos, Vector3.forward, PORT_RADIUS * 0.9f);
                    Handles.color = Color.white;
                    break;
                }
            }
            // Draw drop shadow
            Handles.color = Color.black;
            Handles.DrawBezier(
                fromPos + Vector2.one * 2f,
                snapPos + Vector2.one * 2f,
                fromPos + new Vector2(50, 0) + Vector2.one * 2f,
                snapPos + new Vector2(-50, 0) + Vector2.one * 2f,
                Color.black, null, 10f);
            // Draw main line
            Handles.color = Color.yellow;
            Handles.DrawBezier(
                fromPos,
                snapPos,
                fromPos + new Vector2(50, 0),
                snapPos + new Vector2(-50, 0),
                Color.yellow,
                null, 8f);
            Handles.color = Color.white;
        }
    }

    private void ProcessEvents(Event e)
    {
        // Mouse down: check for connector or node drag
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            bool clickedConnector = false;
            foreach (var node in nodes)
            {
                Rect nodeRect = new Rect(node.node.editorPosition, new Vector2(NODE_WIDTH, NODE_HEIGHT));
                Vector2 outPos = new Vector2(nodeRect.xMax, nodeRect.center.y);
                if (Vector2.Distance(e.mousePosition, outPos) < PORT_RADIUS)
                {
                    // Start connection drag
                    isMakingConnection = true;
                    connectionStartNode = node;
                    e.Use();
                    clickedConnector = true;
                    break;
                }
            }
            if (!clickedConnector)
            {
                foreach (var node in nodes)
                {
                    Rect nodeRect = new Rect(node.node.editorPosition, new Vector2(NODE_WIDTH, NODE_HEIGHT));
                    if (nodeRect.Contains(e.mousePosition))
                    {
                        nodeBeingDragged = node;
                        nodeDragOffset = e.mousePosition - node.node.editorPosition;
                        selectedNode = node;
                        e.Use();
                        break;
                    }
                }
            }
        }
        // Mouse drag: move node if dragging, update connection line if making connection
        if (e.type == EventType.MouseDrag && e.button == 0)
        {
            if (nodeBeingDragged != null && !isMakingConnection)
            {
                nodeBeingDragged.node.editorPosition = e.mousePosition - nodeDragOffset;
                GUI.changed = true;
                e.Use();
            }
            if (isMakingConnection)
            {
                Repaint(); // Real-time connection line
            }
        }
        // Mouse move: update connection line in real time
        if (e.type == EventType.MouseMove && isMakingConnection)
        {
            Repaint();
        }
        // Mouse up: finish connection or stop dragging
        if (e.type == EventType.MouseUp && e.button == 0)
        {
            if (isMakingConnection && connectionStartNode != null)
            {
                bool connected = false;
                foreach (var node in nodes)
                {
                    if (node == connectionStartNode) continue;
                    Rect nodeRect = new Rect(node.node.editorPosition, new Vector2(NODE_WIDTH, NODE_HEIGHT));
                    Vector2 inPos = new Vector2(nodeRect.xMin, nodeRect.center.y);
                    if (Vector2.Distance(e.mousePosition, inPos) < PORT_RADIUS * 1.5f)
                    {
                        connections.RemoveAll(c => c.from == connectionStartNode || c.to == node);
                        connections.Add(new Connection(connectionStartNode, node));
                        connectionStartNode.node.nextNode = node.node;
                        EditorUtility.SetDirty(currentGraph);
                        connected = true;
                        break;
                    }
                }
                isMakingConnection = false;
                connectionStartNode = null;
                e.Use();
            }
            nodeBeingDragged = null;
        }
        // Context menu for adding nodes
        if (e.type == EventType.ContextClick)
        {
            ShowContextMenu(e.mousePosition);
            e.Use();
        }
        // Delete selected node (except StartNode)
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete && selectedNode != null)
        {
            if (!(selectedNode.node is StartNode))
            {
                // Remove all connections to/from this node
                connections.RemoveAll(c => c.from == selectedNode || c.to == selectedNode);
                // Remove nextNode references
                foreach (var n in nodes)
                {
                    if (n.node.nextNode == selectedNode.node)
                        n.node.nextNode = null;
                }
                // Remove from asset
                ScriptableObject.DestroyImmediate(selectedNode.node, true);
                nodes.Remove(selectedNode);
                selectedNode = null;
                AssetDatabase.SaveAssets();
                GUI.changed = true;
                e.Use();
            }
        }
    }

    private void ShowContextMenu(Vector2 mousePos)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add Debug Log Node"), false, () => AddDebugLogNode(mousePos));
        menu.ShowAsContext();
    }

    private void AddDebugLogNode(Vector2 pos)
    {
        if (currentGraph == null) return;
        var node = CreateInstance<DebugLogNode>();
        node.name = "DebugLogNode";
        node.editorPosition = pos;
        AssetDatabase.AddObjectToAsset(node, currentGraph);
        AssetDatabase.SaveAssets();
        nodes.Add(new BaseNode(node));
        EditorUtility.SetDirty(currentGraph);
    }

    private void DrawNodeContextMenu()
    {
        if (selectedNode == null) return;
        GUILayout.FlexibleSpace();
        GUILayout.BeginArea(new Rect(0, position.height - 80, position.width, 80), EditorStyles.helpBox);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Selected Node: {selectedNode.node.name}", EditorStyles.boldLabel, GUILayout.Width(200));
        if (selectedNode.node is DebugLogNode debugNode)
        {
            GUILayout.Label("Debug Message:", GUILayout.Width(110));
            string newMsg = GUILayout.TextField(debugNode.message, GUILayout.Width(position.width - 350));
            if (newMsg != debugNode.message)
            {
                debugNode.message = newMsg;
                EditorUtility.SetDirty(debugNode);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private class BaseNode
    {
        public InteractionNode node;
        public BaseNode(InteractionNode n) { node = n; }
    }
    private class Connection
    {
        public BaseNode from, to;
        public Connection(BaseNode f, BaseNode t) { from = f; to = t; }
    }
} 