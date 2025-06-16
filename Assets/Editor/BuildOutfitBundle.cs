using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildOutfitBundle : Editor
{
    // The output path for your AssetBundles (within the project)
    // You could point this to anywhere on disk, e.g. outside the Assets folder if preferred.
    private static string outputPath = "Assets/AssetBundles";

    // These are the child part names you consider "required/optional".
    // We'll check if they exist but won't fail if they don't.
    private static string[] outfitChildPartNames = new string[]
    {
        "Chest",
        "Head",
        "L_Arm",
        "L_Leg",
        "R_Arm",
        "R_Leg",
        "L_Leg_Back",
        "R_Leg_Back"
    };

    [MenuItem("Tools/Build Outfit Bundle")]
    public static void BuildSelectedOutfitBundle()
    {
        // 1. Ensure user has selected a prefab in the Project window.
        Object selected = Selection.activeObject;
        if (selected == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a rigged outfit prefab in the Project view.", "OK");
            return;
        }

        string path = AssetDatabase.GetAssetPath(selected);
        // Check if it's actually a prefab (rather than a scene object, etc.)
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Invalid Selection", "Selected object is not a prefab.", "OK");
            return;
        }

        // 2. Validate the prefab structure (parent with an Animator, children named as desired).
        Animator parentAnimator = prefab.GetComponent<Animator>();
        if (parentAnimator == null)
        {
            EditorUtility.DisplayDialog("No Animator Found",
                "The parent must have an Animator component (even if it's empty). Add one and try again.",
                "OK");
            return;
        }

        // 3. Check child parts (some are optional, so we'll just log a warning if missing).
        foreach (var partName in outfitChildPartNames)
        {
            Transform child = prefab.transform.Find(partName);
            if (child == null)
            {
                Debug.LogWarning($"Optional part '{partName}' is missing from prefab '{prefab.name}'. That's okay, continuing.");
            }
            else
            {
                Debug.Log($"Found child part: {partName}");
            }
        }

        // 4. Assign an AssetBundle name to this prefab (done via code, or you can do it manually in Inspector).
        //    We'll use the prefab's name for the bundle name, or you can choose a generic "outfits" name, etc.
        string bundleName = prefab.name.ToLower() + "_outfitbundle";
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        assetImporter.SetAssetBundleNameAndVariant(bundleName, string.Empty);

        // 5. Build the AssetBundle
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        // 6. Clear the AssetBundle assignment so we don't leave it set in the editor
        assetImporter.SetAssetBundleNameAndVariant(null, null);

        EditorUtility.DisplayDialog("Build Complete",
            $"Outfit bundle built and placed in '{outputPath}'.\n\nBundle name: {bundleName}",
            "OK");
    }
}
