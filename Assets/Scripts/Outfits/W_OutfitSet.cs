using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.U2D.Animation;

namespace Wrestleverse.Outfits
{
    /// <summary>
    /// Component that lives on the paper-doll character prefab.
    /// It keeps a list of equipped outfits and rebuilds overlay SpriteRenderers whenever that list changes.
    /// Now supports direction roots (Base_Down_Fixed, Base_Up_Fixed, Base_Side_Fixed) as children.
    /// </summary>
    [System.Serializable]
    public class W_EquippedOutfit
    {
        public W_OutfitDescriptor baseOutfit;
        public List<W_RecolorSlot> recolorSlots;
        [Range(0f, 1f)]
        public float Tolerance = 0.05f;

        public W_EquippedOutfit(W_OutfitDescriptor asset)
        {
            baseOutfit = asset;
            recolorSlots = asset.recolorSlots.Select(slot => new W_RecolorSlot(slot)).ToList();
        }
    }

    [ExecuteAlways]
    public sealed class W_OutfitSet : MonoBehaviour
    {
        [Tooltip("List of outfits currently worn by this character – order does not matter.")]
        [SerializeField] private List<W_EquippedOutfit> equippedOutfits = new();

        [Tooltip("Which direction the character is currently facing – controls which sprites are used.")]
        [SerializeField] private W_Direction currentDirection = W_Direction.Down;

        // Direction root names must match these exactly
        private static readonly Dictionary<W_Direction, string> DirectionRootNames = new()
        {
            { W_Direction.Down, "Base_Down_Fixed" },
            { W_Direction.Side, "Base_Side_Fixed" },
            { W_Direction.Up,   "Base_Up_Fixed" }
        };

        // Cache of the base renderers for the currently active direction
        private readonly Dictionary<W_BodyPart, SpriteRenderer> _baseRenderers = new();
        // Overlay renderers we spawned for outfit pieces (key = bodyPart + outfit)
        private readonly Dictionary<(W_BodyPart part, W_OutfitDescriptor outfit), SpriteRenderer> _overlayRenderers = new();

        // New: overlay renderers for all directions
        private readonly Dictionary<(W_Direction, W_BodyPart, W_OutfitDescriptor), SpriteRenderer> _overlayRenderersAllDirections = new();

        // Guard to prevent re-entrant or overlapping Rebuilds
        private bool _isRebuilding = false;

        #region Public API --------------------------------------------------------------------

        public IReadOnlyList<W_EquippedOutfit> EquippedOutfits => equippedOutfits;

        public void AddOutfit(W_OutfitDescriptor outfit)
        {
            if (outfit == null || equippedOutfits.Any(eo => eo.baseOutfit == outfit)) return;
            equippedOutfits.Add(new W_EquippedOutfit(outfit));
            Rebuild();
        }

        public void RemoveOutfit(W_OutfitDescriptor outfit)
        {
            var idx = equippedOutfits.FindIndex(eo => eo.baseOutfit == outfit);
            if (idx >= 0)
            {
                equippedOutfits.RemoveAt(idx);
                Rebuild();
            }
        }

        public void ClearOutfits()
        {
            equippedOutfits.Clear();
            Rebuild();
        }

        public void SetFacingDirection(W_Direction dir)
        {
            if (currentDirection == dir) return;
            currentDirection = dir;
            Rebuild();
        }

        /// <summary>
        /// Sync overlays for a specific body part to match the base part's active state. Call from Animation Events.
        /// </summary>
        public void SyncOverlayActiveState(string partName)
        {
            var root = GetCurrentDirectionRoot();
            if (root == null) return;
            var basePartT = root.Find(partName);
            if (basePartT == null) return;
            bool baseActive = basePartT.gameObject.activeSelf;

            foreach (var kvp in _overlayRenderersAllDirections)
            {
                var (direction, part, outfit) = kvp.Key;
                if (direction != currentDirection) continue;
                if (part.ToString() != partName) continue;
                var overlaySr = kvp.Value;
                if (overlaySr == null) continue;
                var overlayGo = overlaySr.gameObject;
                overlayGo.SetActive(baseActive);
            }
        }

        #endregion

        #region Unity callbacks ---------------------------------------------------------------

        private void Awake()
        {
            // Remove auto-add of OverlayOutfitAnimator
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                CacheBaseRenderers();
                Rebuild();
            }
#else
            CacheBaseRenderers();
            Rebuild();
#endif
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                CacheBaseRenderers();
                Rebuild();
            }
#else
            CacheBaseRenderers();
            Rebuild();
#endif
        }

#if UNITY_EDITOR
        private bool needsRebuild = false;
        private void OnValidate()
        {
            needsRebuild = true;
            UnityEditor.EditorApplication.update -= EditorRebuild;
            UnityEditor.EditorApplication.update += EditorRebuild;
            foreach (var kvp in _overlayRenderersAllDirections)
            {
                var sr = kvp.Value;
                if (sr != null)
                {
                    // Find the correct equipped outfit for this overlay
                    var outfitDescriptor = kvp.Key.Item3;
                    var equipped = equippedOutfits.FirstOrDefault(eo => eo.baseOutfit == outfitDescriptor);
                    float tolerance = equipped != null ? equipped.Tolerance : 0.05f;
                    var recolorSlots = equipped != null ? equipped.recolorSlots : null;
                    UpdatePaletteMaterialWithPropertyBlock(sr, recolorSlots, tolerance);
                }
            }
        }
        private void EditorRebuild()
        {
            if (needsRebuild)
            {
                // Check if this component and its GameObject are still valid
                if (this == null || this.gameObject == null)
                {
                    UnityEditor.EditorApplication.update -= EditorRebuild;
                    return;
                }
                needsRebuild = false;
                Rebuild();
                UnityEditor.EditorApplication.update -= EditorRebuild;
            }
        }

        [UnityEditor.CustomEditor(typeof(W_OutfitSet))]
        public class W_OutfitSetEditor : UnityEditor.Editor
        {
            private UnityEditor.SerializedProperty outfitsProp;
            private W_OutfitSet set;
            private void OnEnable()
            {
                set = (W_OutfitSet)target;
                outfitsProp = serializedObject.FindProperty("equippedOutfits");
            }
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                // Draw Equipped Outfits with per-outfit tolerance
                EditorGUILayout.LabelField("Equipped Outfits", EditorStyles.boldLabel);
                for (int i = 0; i < outfitsProp.arraySize; i++)
                {
                    var outfitProp = outfitsProp.GetArrayElementAtIndex(i);
                    var baseOutfitProp = outfitProp.FindPropertyRelative("baseOutfit");
                    var toleranceProp = outfitProp.FindPropertyRelative("Tolerance");
                    EditorGUILayout.BeginVertical("HelpBox");

                    // Bold label with outfit name or index
                    string outfitName = baseOutfitProp.objectReferenceValue != null ? baseOutfitProp.objectReferenceValue.name : $"Outfit {i + 1}";
                    EditorGUILayout.LabelField(outfitName, EditorStyles.boldLabel);

                    EditorGUILayout.PropertyField(baseOutfitProp, new GUIContent($"Base Outfit"));
                    EditorGUILayout.Slider(toleranceProp, 0f, 1f, new GUIContent("Color Match Tolerance"));
                    EditorGUILayout.PropertyField(outfitProp.FindPropertyRelative("recolorSlots"), true);
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Remove Outfit"))
                    {
                        outfitsProp.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        set.Rebuild();
                        break;
                    }
                    EditorGUILayout.EndVertical();
                    // Add separator and extra space between outfits
                    if (i < outfitsProp.arraySize - 1)
                    {
                        EditorGUILayout.Space(6);
                        GUIStyle line = new GUIStyle("box");
                        Rect rect = GUILayoutUtility.GetRect(1, 2, GUILayout.ExpandWidth(true));
                        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                        EditorGUILayout.Space(6);
                    }
                }
                // Add button to add a new outfit
                if (GUILayout.Button("+ Add Outfit"))
                {
                    outfitsProp.arraySize++;
                    var newOutfit = outfitsProp.GetArrayElementAtIndex(outfitsProp.arraySize - 1);
                    newOutfit.FindPropertyRelative("baseOutfit").objectReferenceValue = null;
                    newOutfit.FindPropertyRelative("Tolerance").floatValue = 0.05f;
                    newOutfit.FindPropertyRelative("recolorSlots").ClearArray();
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUILayout.Space();
                bool changed = EditorGUI.EndChangeCheck();
                serializedObject.ApplyModifiedProperties();
                if (changed)
                {
                    set.Rebuild();
                }
            }
        }
#endif

        #endregion

        //------------------------------------------------------------------------------------------------------------------
        //  INTERNAL LOGIC
        //------------------------------------------------------------------------------------------------------------------

        private void CacheBaseRenderers()
        {
            _baseRenderers.Clear();
            var root = GetCurrentDirectionRoot();
            if (root == null) return;
            foreach (W_BodyPart part in Enum.GetValues(typeof(W_BodyPart)))
            {
                Transform t = root.Find(part.ToString());
                if (t == null) continue;
                if (t.TryGetComponent<SpriteRenderer>(out var sr))
                    _baseRenderers[part] = sr;
            }
        }

        private void CacheBaseRenderersForDirection(W_Direction direction, Dictionary<W_BodyPart, SpriteRenderer> rendererDict)
        {
            rendererDict.Clear();
            var root = GetDirectionRoot(direction);
            if (root == null) return;
            foreach (W_BodyPart part in Enum.GetValues(typeof(W_BodyPart)))
            {
                Transform t = root.Find(part.ToString());
                if (t == null) continue;
                if (t.TryGetComponent<SpriteRenderer>(out var sr))
                    rendererDict[part] = sr;
            }
        }

        private void Rebuild()
        {
            if (_isRebuilding) return;
            _isRebuilding = true;
            try
            {
                // --- HARD CLEANUP: Destroy all overlay GameObjects under each body part in each direction ---
                foreach (W_Direction direction in Enum.GetValues(typeof(W_Direction)))
                {
                    var root = GetDirectionRoot(direction);
                    if (root == null) continue;
                    foreach (W_BodyPart part in Enum.GetValues(typeof(W_BodyPart)))
                    {
                        var partT = root.Find(part.ToString());
                        if (partT == null) continue;
                        var overlaysToDestroy = new List<GameObject>();
                        foreach (Transform child in partT)
                        {
                            if (child.name.Contains("_Overlay_"))
                            {
                                overlaysToDestroy.Add(child.gameObject);
                            }
                        }
                        foreach (var go in overlaysToDestroy)
                        {
#if UNITY_EDITOR
                            if (!Application.isPlaying)
                                DestroyImmediate(go);
                            else
#endif
                                Destroy(go);
                        }
                    }
                }

                // Hide all direction roots except the current one
                foreach (var kvp in DirectionRootNames)
                {
                    var dirRoot = transform.Find(kvp.Value);
                    if (dirRoot != null)
                        dirRoot.gameObject.SetActive(kvp.Key == currentDirection);
                }

                // --- CLEANUP: Remove all overlays and enable all base parts for all directions ---
                // 1. Destroy all overlays and clear the overlay dictionary
                foreach (var kvp in _overlayRenderersAllDirections)
                {
                    var sr = kvp.Value;
                    if (sr != null)
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                        {
                            var go = sr.gameObject;
                            DestroyImmediate(go);
                        }
                        else
#endif
                        {
                            Destroy(sr.gameObject);
                        }
                    }
                }
                _overlayRenderersAllDirections.Clear();

                // 2. Enable all base body part SpriteRenderers for all directions
                foreach (W_Direction direction in Enum.GetValues(typeof(W_Direction)))
                {
                    var baseRenderers = new Dictionary<W_BodyPart, SpriteRenderer>();
                    CacheBaseRenderersForDirection(direction, baseRenderers);
                    foreach (var sr in baseRenderers.Values)
                    {
                        if (sr != null)
                            sr.enabled = true;
                    }
                }

                // --- Now apply outfit logic for ALL directions ---
                foreach (W_Direction direction in Enum.GetValues(typeof(W_Direction)))
                {
                    var baseRenderers = new Dictionary<W_BodyPart, SpriteRenderer>();
                    CacheBaseRenderersForDirection(direction, baseRenderers);

                    foreach (W_BodyPart part in Enum.GetValues(typeof(W_BodyPart)))
                    {
                        bool partIsReplaced = false;
                        int baseSortingOrder = 0;
                        SpriteRenderer baseSr = null;
                        if (baseRenderers.TryGetValue(part, out baseSr) && baseSr != null)
                        {
                            baseSortingOrder = baseSr.sortingOrder;
                            baseSr.enabled = true;
                        }

                        var overlays = new List<(Sprite sprite, int sortOffset, W_EquippedOutfit equippedOutfit)>();

                        foreach (var equipped in equippedOutfits)
                        {
                            if (equipped == null || equipped.baseOutfit == null) continue;
                            var entry = equipped.baseOutfit.parts.Find(p => p.bodyPart == part && p.hasArt);
                            if (entry == null) continue;

                            if (entry.replacesBase)
                                partIsReplaced = true;

                            Sprite spriteForDir = entry.GetSprite(direction);
                            if (spriteForDir == null) continue;

                            overlays.Add((spriteForDir, entry.sortOffset, equipped));
                        }

                        if (baseSr != null)
                            baseSr.enabled = !partIsReplaced;

                        overlays.Sort((a, b) => a.sortOffset.CompareTo(b.sortOffset));

                        for (int i = 0; i < overlays.Count; i++)
                        {
                            var (sprite, offset, equipped) = overlays[i];
                            var key = (direction, part, equipped.baseOutfit);
                            Material paletteMat = null;
                            if (!_overlayRenderersAllDirections.TryGetValue(key, out var sr) || sr == null)
                            {
#if UNITY_EDITOR
                                if (!Application.isPlaying)
                                {
                                    var parent = baseSr != null ? baseSr.transform : GetDirectionRoot(direction);
                                    var partName = $"{part}_{equipped.baseOutfit.name}_Overlay_{direction}";
                                    var spriteCopy = sprite;
                                    var offsetCopy = offset;
                                    var equippedCopy = equipped;
                                    GameObject editorGo = new GameObject(partName);
                                    editorGo.transform.SetParent(parent, false);
                                    editorGo.transform.localPosition = Vector3.zero;
                                    editorGo.transform.localRotation = Quaternion.identity;
                                    editorGo.transform.localScale = Vector3.one;
                                    var sr2 = editorGo.AddComponent<SpriteRenderer>();
                                    sr2.sprite = spriteCopy;
                                    sr2.sortingLayerID = baseSr != null ? baseSr.sortingLayerID : 0;
                                    sr2.sortingOrder = baseSr != null ? baseSr.sortingOrder + 1 + offsetCopy : 0;
                                    sr2.color = baseSr != null ? baseSr.color : Color.white;
                                    sr2.flipX = baseSr != null && baseSr.flipX;
                                    sr2.flipY = baseSr != null && baseSr.flipY;
                                    sr2.enabled = true;
                                    paletteMat = Resources.Load<Material>("W_PaletteSwap");
                                    if (paletteMat != null)
                                    {
                                        sr2.material = paletteMat;
                                    }
                                    // Copy SpriteSkin if present
                                    if (baseSr != null)
                                    {
                                        var baseSkin = baseSr.GetComponent<SpriteSkin>();
                                        if (baseSkin != null)
                                        {
                                            var overlaySkin = editorGo.AddComponent<SpriteSkin>();
                                            overlaySkin.autoRebind = baseSkin.autoRebind;
                                            overlaySkin.alwaysUpdate = baseSkin.alwaysUpdate;
                                            overlaySkin.SetRootBone(baseSkin.rootBone);
                                            overlaySkin.SetBoneTransforms(baseSkin.boneTransforms);
                                        }
                                    }
                                    UpdatePaletteMaterialWithPropertyBlock(sr2, equipped.recolorSlots, equipped.Tolerance);
                                    _overlayRenderersAllDirections[key] = sr2;
                                    continue;
                                }
#endif
                                // Play mode: create overlay immediately
                                GameObject go = new($"{part}_{equipped.baseOutfit.name}_Overlay_{direction}");
                                go.transform.SetParent(baseSr != null ? baseSr.transform : GetDirectionRoot(direction), false);
                                go.transform.localPosition = Vector3.zero;
                                go.transform.localRotation = Quaternion.identity;
                                go.transform.localScale = Vector3.one;
                                sr = go.AddComponent<SpriteRenderer>();
                                paletteMat = Resources.Load<Material>("W_PaletteSwap");
                                if (paletteMat != null)
                                {
                                    sr.material = paletteMat;
                                }
                                // Copy SpriteSkin if present
                                if (baseSr != null)
                                {
                                    var baseSkin = baseSr.GetComponent<SpriteSkin>();
                                    if (baseSkin != null)
                                    {
                                        var overlaySkin = go.AddComponent<SpriteSkin>();
                                        overlaySkin.autoRebind = baseSkin.autoRebind;
                                        overlaySkin.alwaysUpdate = baseSkin.alwaysUpdate;
                                        overlaySkin.SetRootBone(baseSkin.rootBone);
                                        overlaySkin.SetBoneTransforms(baseSkin.boneTransforms);
                                    }
                                }
                                UpdatePaletteMaterialWithPropertyBlock(sr, equipped.recolorSlots, equipped.Tolerance);
                                _overlayRenderersAllDirections[key] = sr;
                            }

                            // Set properties (play mode only; edit mode handled above)
                            if (Application.isPlaying && sr != null)
                            {
                                paletteMat = Resources.Load<Material>("W_PaletteSwap");
                                if (paletteMat != null)
                                {
                                    sr.material = paletteMat;
                                }
                                UpdatePaletteMaterialWithPropertyBlock(sr, equipped.recolorSlots, equipped.Tolerance);
                                sr.sprite = sprite;
                                sr.sortingLayerID = baseSr != null ? baseSr.sortingLayerID : 0;
                                sr.sortingOrder = baseSr != null ? baseSr.sortingOrder + 1 + offset : 0;
                                sr.color = baseSr != null ? baseSr.color : Color.white;
                                sr.flipX = baseSr != null && baseSr.flipX;
                                sr.flipY = baseSr != null && baseSr.flipY;
                                sr.enabled = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                _isRebuilding = false;
            }
        }

        private Transform GetCurrentDirectionRoot()
        {
            if (!DirectionRootNames.TryGetValue(currentDirection, out var rootName))
                return null;
            return transform.Find(rootName);
        }

        private Transform GetDirectionRoot(W_Direction direction)
        {
            if (!DirectionRootNames.TryGetValue(direction, out var rootName))
                return null;
            return transform.Find(rootName);
        }

        private void UpdatePaletteMaterialWithPropertyBlock(SpriteRenderer sr, List<W_RecolorSlot> recolorSlots, float tolerance)
        {
            if (sr == null) return;
            var block = new MaterialPropertyBlock();
            sr.GetPropertyBlock(block);

            block.SetFloat("_Tolerance", tolerance);

            // Always set _MainTex if possible
            if (sr.sprite != null && sr.sprite.texture != null)
            {
                block.SetTexture("_MainTex", sr.sprite.texture);
            }

            if (recolorSlots != null && recolorSlots.Count > 0)
            {
                var origs = new List<Color>();
                var targets = new List<Color>();
                foreach (var slot in recolorSlots)
                {
                    origs.Add(slot.baseColor);
                    origs.AddRange(slot.relativeShades);
                    var tShades = slot.GetTargetShades();
                    targets.AddRange(tShades);
                }
                int count = Mathf.Min(origs.Count, targets.Count, 8);
                block.SetFloat("_SwapCount", count);
                for (int k = 0; k < count; k++)
                {
                    var orig = origs[k]; orig.a = 1f;
                    var targ = targets[k]; targ.a = 1f;
                    block.SetColor($"_OriginalColor{k}", orig);
                    block.SetColor($"_TargetColor{k}", targ);
                }
                // Fill unused slots with white
                for (int k = count; k < 8; k++)
                {
                    block.SetColor($"_OriginalColor{k}", Color.white);
                    block.SetColor($"_TargetColor{k}", Color.white);
                }
            }
            else
            {
                // No recolor slots: set _SwapCount to 0 and all color properties to white
                block.SetFloat("_SwapCount", 0);
                for (int k = 0; k < 8; k++)
                {
                    block.SetColor($"_OriginalColor{k}", Color.white);
                    block.SetColor($"_TargetColor{k}", Color.white);
                }
            }
            sr.SetPropertyBlock(block);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Application.isPlaying)
            {
                Debug.Log($"[PaletteSwap] PropertyBlock for {sr.gameObject.name}: Original[0]={block.GetColor("_OriginalColor0")}, Target[0]={block.GetColor("_TargetColor0")}");
            }
#endif
        }
    }
} 