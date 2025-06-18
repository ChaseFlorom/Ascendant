using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Wrestleverse.Outfits;

namespace Wrestleverse.EditorTools
{
    public class W_OutfitRecolorEditorWindow : EditorWindow
    {
        private W_OutfitDescriptor selectedOutfit;
        private Vector2 scroll;

        [MenuItem("Tools/Wrestleverse/Outfit Recolor Editor")]
        public static void Open()
        {
            GetWindow<W_OutfitRecolorEditorWindow>(false, "Outfit Recolor Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("Outfit Recolor Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            selectedOutfit = (W_OutfitDescriptor)EditorGUILayout.ObjectField("Outfit Asset", selectedOutfit, typeof(W_OutfitDescriptor), false);
            if (selectedOutfit == null)
            {
                EditorGUILayout.HelpBox("Select a W_OutfitDescriptor to edit its recolor slots.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Recolor Slot"))
            {
                Undo.RecordObject(selectedOutfit, "Add Recolor Slot");
                selectedOutfit.recolorSlots.Add(new W_RecolorSlot { slotName = "New Region", relativeShades = new List<Color>(), targetColor = Color.white });
                EditorUtility.SetDirty(selectedOutfit);
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < selectedOutfit.recolorSlots.Count; i++)
            {
                var slot = selectedOutfit.recolorSlots[i];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                slot.slotName = EditorGUILayout.TextField("Region Name", slot.slotName);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    Undo.RecordObject(selectedOutfit, "Remove Recolor Slot");
                    selectedOutfit.recolorSlots.RemoveAt(i);
                    EditorUtility.SetDirty(selectedOutfit);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                // Base color
                slot.baseColor = EditorGUILayout.ColorField("Base Color", slot.baseColor);

                // Relative shades
                EditorGUILayout.LabelField("Relative Shades:");
                for (int j = 0; j < slot.relativeShades.Count; j++)
                {
                    EditorGUILayout.BeginHorizontal();
                    slot.relativeShades[j] = EditorGUILayout.ColorField(slot.relativeShades[j]);
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        slot.relativeShades.RemoveAt(j);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Add Relative Shade"))
                {
                    slot.relativeShades.Add(slot.baseColor);
                }

                // Target color
                slot.targetColor = EditorGUILayout.ColorField("Target Color", slot.targetColor);

                // Preview computed new shades
                var previewShades = slot.GetTargetShades();
                EditorGUILayout.LabelField("Preview Target Shades:");
                EditorGUILayout.BeginHorizontal();
                foreach (var c in previewShades)
                {
                    var prev = GUI.backgroundColor;
                    GUI.backgroundColor = c;
                    GUILayout.Button(" ", GUILayout.Width(30), GUILayout.Height(20));
                    GUI.backgroundColor = prev;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(selectedOutfit);
            }
        }
    }
} 