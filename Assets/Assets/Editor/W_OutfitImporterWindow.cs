using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Wrestleverse.Outfits;

namespace Wrestleverse.EditorTools
{
    public class W_OutfitImporterWindow : EditorWindow
    {
        private string _outfitName = "New Outfit";
        private Object _downPsb;
        private Object _sidePsb;
        private Object _upPsb;

        private readonly Dictionary<W_BodyPart, bool> _includePart = new();
        private readonly Dictionary<W_BodyPart, bool> _replacePart = new();

        [MenuItem("Tools/Wrestleverse/Outfit Importer")]
        public static void Open()
        {
            var window = GetWindow<W_OutfitImporterWindow>(true, "Outfit Importer");
            window.minSize = new Vector2(350, 500);
        }

        private void OnEnable()
        {
            foreach (W_BodyPart part in System.Enum.GetValues(typeof(W_BodyPart)))
            {
                if (!_includePart.ContainsKey(part))
                    _includePart[part] = false;
                if (!_replacePart.ContainsKey(part))
                    _replacePart[part] = false;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Create Outfit Descriptor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _outfitName = EditorGUILayout.TextField("Outfit Name", _outfitName);

            _downPsb = PSBField("Down PSB", _downPsb);
            _sidePsb = PSBField("Side PSB", _sidePsb);
            _upPsb   = PSBField("Up PSB", _upPsb);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Body Parts", EditorStyles.boldLabel);

            foreach (W_BodyPart part in System.Enum.GetValues(typeof(W_BodyPart)))
            {
                EditorGUILayout.BeginHorizontal();
                _includePart[part] = EditorGUILayout.ToggleLeft(part.ToString(), _includePart[part], GUILayout.Width(170));
                if (_includePart[part])
                {
                    _replacePart[part] = EditorGUILayout.ToggleLeft("Replace", _replacePart[part], GUILayout.Width(70));
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            bool allValid = IsValidPSB(_downPsb) && IsValidPSB(_sidePsb) && IsValidPSB(_upPsb);
            if (!IsValidPSB(_downPsb) && _downPsb != null) EditorGUILayout.HelpBox("Down PSB must be a .psb file.", MessageType.Error);
            if (!IsValidPSB(_sidePsb) && _sidePsb != null) EditorGUILayout.HelpBox("Side PSB must be a .psb file.", MessageType.Error);
            if (!IsValidPSB(_upPsb) && _upPsb != null) EditorGUILayout.HelpBox("Up PSB must be a .psb file.", MessageType.Error);

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_outfitName) || !allValid))
            {
                if (GUILayout.Button("Create Outfit"))
                {
                    CreateOutfit();
                }
            }
        }

        private Object PSBField(string label, Object current)
        {
            Object obj = EditorGUILayout.ObjectField(label, current, typeof(Object), false);
            if (obj != null && !IsValidPSB(obj))
            {
                // If not a PSB, show warning and clear
                return obj;
            }
            return obj;
        }

        private static bool IsValidPSB(Object obj)
        {
            if (obj == null) return false;
            string path = AssetDatabase.GetAssetPath(obj);
            return path != null && path.ToLower().EndsWith(".psb");
        }

        //------------------------------------------------------------------------------------------------------------------
        //  INTERNALS
        //------------------------------------------------------------------------------------------------------------------

        private void CreateOutfit()
        {
            // Validate.
            if (!IsValidPSB(_downPsb) || !IsValidPSB(_sidePsb) || !IsValidPSB(_upPsb))
            {
                EditorUtility.DisplayDialog("Missing PSB", "Please assign all three PSB files before creating the outfit.", "OK");
                return;
            }

            string folder = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(_downPsb));
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(folder, _outfitName + ".asset"));

            var outfit = ScriptableObject.CreateInstance<W_OutfitDescriptor>();
            outfit.outfitName = _outfitName;

            foreach (W_BodyPart part in System.Enum.GetValues(typeof(W_BodyPart)))
            {
                if (!_includePart[part]) continue;

                var entry = new W_OutfitPartEntry
                {
                    bodyPart = part,
                    hasArt = true,
                    replacesBase = _replacePart[part],
                    sortOffset = 0,
                    downSprite = FindSprite(_downPsb, part.ToString()),
                    sideSprite = FindSprite(_sidePsb, part.ToString()),
                    upSprite = FindSprite(_upPsb, part.ToString())
                };

                outfit.parts.Add(entry);
            }

            AssetDatabase.CreateAsset(outfit, assetPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = outfit;

            EditorUtility.DisplayDialog("Success", $"Created outfit asset at {assetPath}", "OK");
        }

        private static Sprite FindSprite(Object psbObj, string bodyPartName)
        {
            if (!IsValidPSB(psbObj)) return null;
            string path = AssetDatabase.GetAssetPath(psbObj);
            var objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            foreach (var obj in objects)
            {
                if (obj is Sprite sprite && sprite.name.StartsWith(bodyPartName))
                    return sprite;
            }
            Debug.LogWarning($"Sprite for body part '{bodyPartName}' not found in '{psbObj.name}'.");
            return null;
        }
    }
} 