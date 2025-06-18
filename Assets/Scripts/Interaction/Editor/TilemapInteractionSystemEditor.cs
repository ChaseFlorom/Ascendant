using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TilemapInteractionSystem))]
public class TilemapInteractionSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
} 