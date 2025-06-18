using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;

[CustomEditor(typeof(InteractionPoint))]
public class InteractionPointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        InteractionPoint interactionPoint = (InteractionPoint)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Interaction Point", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Action Graph
        EditorGUILayout.BeginHorizontal();
        interactionPoint.actionGraph = (ActionGraph)EditorGUILayout.ObjectField("Action Graph", interactionPoint.actionGraph, typeof(ActionGraph), false);
        if (GUILayout.Button("Create New", GUILayout.Width(80)))
        {
            ActionGraph newGraph = CreateInstance<ActionGraph>();
            string sceneName = SceneManager.GetActiveScene().name;
            string folderPath = Path.Combine("Assets", sceneName);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", sceneName);
            }
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, "NewActionGraph.asset"));
            AssetDatabase.CreateAsset(newGraph, assetPath);
            AssetDatabase.SaveAssets();
            interactionPoint.actionGraph = newGraph;
            EditorUtility.SetDirty(interactionPoint);
            Selection.activeObject = newGraph;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Interaction Type
        interactionPoint.interactionType = (InteractionType)EditorGUILayout.EnumPopup("Interaction Type", interactionPoint.interactionType);
        EditorGUILayout.Space();
        
        // Settings
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        interactionPoint.isOneTimeUse = EditorGUILayout.Toggle("One Time Use", interactionPoint.isOneTimeUse);
        
        if (interactionPoint.isOneTimeUse)
        {
            EditorGUI.BeginDisabledGroup(true);
            interactionPoint.hasBeenUsed = EditorGUILayout.Toggle("Has Been Used", interactionPoint.hasBeenUsed);
            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button("Reset Usage"))
            {
                interactionPoint.hasBeenUsed = false;
                EditorUtility.SetDirty(interactionPoint);
            }
        }
        
        EditorGUILayout.Space();
        
        // Visual Settings
        EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);
        interactionPoint.gizmoColor = EditorGUILayout.ColorField("Gizmo Color", interactionPoint.gizmoColor);
        interactionPoint.gizmoSize = EditorGUILayout.FloatField("Gizmo Size", interactionPoint.gizmoSize);
        
        EditorGUILayout.Space();
        
        // Quick Actions
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Test Interaction"))
        {
            interactionPoint.Interact();
        }
        
        if (GUILayout.Button("Focus in Scene"))
        {
            Selection.activeGameObject = interactionPoint.gameObject;
            SceneView.FrameLastActiveSceneView();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Status
        EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
        string status = interactionPoint.CanInteract() ? "Ready to Interact" : "Cannot Interact";
        Color statusColor = interactionPoint.CanInteract() ? Color.green : Color.red;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Status:", GUILayout.Width(50));
        GUI.color = statusColor;
        EditorGUILayout.LabelField(status);
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
        
        if (interactionPoint.actionGraph == null)
        {
            EditorGUILayout.HelpBox("No Action Graph assigned! This interaction point won't do anything.", MessageType.Warning);
        }
    }
} 