using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SpriteLayerTools
{
    [MenuItem("Tools/Sprites/Set Sorting Layer On Selected And Children")]
    static void SetSortingLayerOnSelectedAndChildren()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("No GameObject selected!");
            return;
        }

        string sortingLayerName = "Player"; // <-- Change this to your desired sorting layer name
        int baseOrder = 0;

        // Custom order arrays for each direction
        string[] downOrder = {
            "Back_Arm", "Back_Leg", "L_Leg_Back", "R_Leg_Back",
            "Chest", "Waist", "L_Leg", "R_Leg",
            "Head",
            "Front_Arm", "Front_Leg"
        };
        string[] upOrder = {
            "Front_Arm", "Front_Leg",
            "L_Leg_Back", "R_Leg_Back", "Back_Arm", "Back_Leg",
            "Chest", "Waist", "L_Leg", "R_Leg",
            "Head"
        };
        string[] sideOrder = {
            "Back_Arm", "Back_Leg", "L_Leg_Back", "R_Leg_Back",
            "Chest", "Waist", "L_Leg", "R_Leg",
            "Head",
            "Front_Arm", "Front_Leg"
        };

        var directionRoots = new[] { "Base_Down_Fixed", "Base_Up_Fixed", "Base_Side_Fixed" };
        int totalBases = 0, totalOverlays = 0;

        foreach (var dirRootName in directionRoots)
        {
            var dirRoot = Selection.activeGameObject.transform.Find(dirRootName);
            if (dirRoot == null) continue;

            string[] orderArray = downOrder;
            if (dirRootName.Contains("Up")) orderArray = upOrder;
            else if (dirRootName.Contains("Side")) orderArray = sideOrder;

            // Map part name to order
            var partOrder = new Dictionary<string, int>();
            for (int i = 0; i < orderArray.Length; i++)
                partOrder[orderArray[i]] = baseOrder + i * 10; // leave room for overlays

            int nextOrder = baseOrder + orderArray.Length * 10;

            // First, assign base parts
            foreach (Transform child in dirRoot)
            {
                var sr = child.GetComponent<SpriteRenderer>();
                if (sr == null) continue;
                int order = partOrder.ContainsKey(child.name) ? partOrder[child.name] : nextOrder++;
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = order;
                totalBases++;

                // Now overlays for this part
                var overlays = new List<SpriteRenderer>();
                foreach (Transform overlayChild in child)
                {
                    var overlaySr = overlayChild.GetComponent<SpriteRenderer>();
                    if (overlaySr != null && overlayChild.name.Contains("_Overlay_"))
                        overlays.Add(overlaySr);
                }
                overlays = overlays.OrderBy(x => x.gameObject.name).ToList();
                for (int j = 0; j < overlays.Count; j++)
                {
                    overlays[j].sortingLayerName = sortingLayerName;
                    overlays[j].sortingOrder = order + 1 + j;
                    totalOverlays++;
                }
            }
        }

        Debug.Log($"Set sorting layer '{sortingLayerName}' on {totalBases} base SpriteRenderers and {totalOverlays} overlays, using custom order arrays.");
    }
} 