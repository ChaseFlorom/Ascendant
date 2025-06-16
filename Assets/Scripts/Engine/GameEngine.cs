using UnityEngine;

public class GameEngine : MonoBehaviour
{
    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    public void Start()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }
    
}
