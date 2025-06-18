using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ActionGraphEditorWindow : EditorWindow
{
    private ActionGraph currentGraph;
    private Vector2 scrollPos;
    private Dictionary<string, InteractionNode> nodeLookup = new Dictionary<string, InteractionNode>();

    [MenuItem("Tools/Action Graph Editor")]
    public static void OpenWindow()
    {
        GetWindow<ActionGraphEditorWindow>("Action Graph Editor");
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        currentGraph = (ActionGraph)EditorGUILayout.ObjectField("Action Graph", currentGraph, typeof(ActionGraph), false);
        if (GUILayout.Button("Reload") && currentGraph != null)
        {
            BuildNodeLookup();
        }
        EditorGUILayout.EndHorizontal();

        if (currentGraph == null || currentGraph.startNode == null)
        {
            EditorGUILayout.HelpBox("Assign an ActionGraph with a start node.", MessageType.Info);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        DrawNodeRecursive(currentGraph.startNode, 0);
        EditorGUILayout.EndScrollView();
    }

    void BuildNodeLookup()
    {
        nodeLookup.Clear();
        if (currentGraph == null || currentGraph.startNode == null) return;
        Queue<InteractionNode> queue = new Queue<InteractionNode>();
        queue.Enqueue(currentGraph.startNode);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node == null || nodeLookup.ContainsKey(node.guid)) continue;
            nodeLookup[node.guid] = node;
            if (node.nextNode != null) queue.Enqueue(node.nextNode);
        }
    }

    void DrawNodeRecursive(InteractionNode node, int depth)
    {
        if (node == null) return;
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"{node.GetType().Name}", EditorStyles.boldLabel);
        node.editorPosition = EditorGUILayout.Vector2Field("Position", node.editorPosition);
        node.name = EditorGUILayout.TextField("Name", node.name);
        node.nextNode = (InteractionNode)EditorGUILayout.ObjectField("Next Node", node.nextNode, typeof(InteractionNode), false);
        EditorGUILayout.EndVertical();
        if (node.nextNode != null)
        {
            GUILayout.Space(20);
            DrawNodeRecursive(node.nextNode, depth + 1);
        }
    }
} 