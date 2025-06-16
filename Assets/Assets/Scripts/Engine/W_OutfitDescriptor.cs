using System.Collections.Generic;
using UnityEngine;

namespace Wrestleverse.Outfits
{
    /// <summary>
    /// Holds the sprite resources and meta information for a single outfit.
    /// One asset equals one outfit that can be equipped in-game.
    /// </summary>
    [CreateAssetMenu(menuName = "Wrestleverse/Outfit Descriptor", fileName = "NewW_Outfit.asset")]
    public class W_OutfitDescriptor : ScriptableObject
    {
        public string outfitName = "New Outfit";
        public List<W_OutfitPartEntry> parts = new();
        public List<W_RecolorSlot> recolorSlots = new();
    }

    //------------------------------------------------------------------------------------------------------------------
    //  Nested data classes
    //------------------------------------------------------------------------------------------------------------------

    [System.Serializable]
    public class W_OutfitPartEntry
    {
        public W_BodyPart bodyPart;
        public bool hasArt = true;
        public bool replacesBase = false; // true = hides the underlying body part
        public int sortOffset = 0;         // higher draws on top of lower

        public Sprite downSprite;
        public Sprite sideSprite;
        public Sprite upSprite;

        /// <summary>
        /// Convenience accessor for a directional sprite.
        /// </summary>
        public Sprite GetSprite(W_Direction dir)
        {
            return dir switch
            {
                W_Direction.Up   => upSprite,
                W_Direction.Side => sideSprite,
                _                => downSprite,
            };
        }
    }

    /// <summary>
    /// Directions the character can face – maps to the three PSB files.
    /// </summary>
    public enum W_Direction
    {
        Down,
        Side,
        Up
    }

    [System.Serializable]
    public class W_RecolorSlot
    {
        public string slotName;
        public Color baseColor; // The main color in the art for this region
        public List<Color> relativeShades = new(); // The other shades, as colors in the art
        public Color targetColor; // The new base color to swap to

        // Copy constructor for deep copy
        public W_RecolorSlot(W_RecolorSlot other)
        {
            slotName = other.slotName;
            baseColor = other.baseColor;
            targetColor = other.targetColor;
            relativeShades = new List<Color>(other.relativeShades);
        }
        public W_RecolorSlot() { }

        // Computes the new shades for the target color, preserving the original relative differences
        public List<Color> GetTargetShades()
        {
            var result = new List<Color> { targetColor };
            foreach (var rel in relativeShades)
            {
                // Calculate the RGB difference from base to rel
                var diff = rel - baseColor;
                var newShade = targetColor + diff;
                // Clamp to [0,1]
                newShade.r = Mathf.Clamp01(newShade.r);
                newShade.g = Mathf.Clamp01(newShade.g);
                newShade.b = Mathf.Clamp01(newShade.b);
                newShade.a = 1f;
                result.Add(newShade);
            }
            return result;
        }
    }
} 