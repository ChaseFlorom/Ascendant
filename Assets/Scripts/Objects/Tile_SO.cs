using UnityEngine;

[CreateAssetMenu(fileName = "New Tile", menuName = "Tiles/New Tile")]
public class Tile_SO : ScriptableObject
{
    public string tileName;
    public bool isRing;
    public bool isPassable = true;
    public int z = 0;

    public Sprite sprite;
}
