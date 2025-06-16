using UnityEngine;

[CreateAssetMenu(fileName = "DirectionalGraphics", menuName = "Containers/DirectionalGraphics")]
public class DirectionalGraphics : ScriptableObject
{
    [Header("Prefabs for each direction")]
    public GameObject downPrefab;
    public GameObject leftPrefab;
    public GameObject upPrefab;
    // For 'Right', we'll flip the leftPrefab at runtime.
}
