using UnityEngine;

public enum GameState
{
    Exploration,  // Free movement with WASD
    Battle       // Tactical turn-based movement
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("State Settings")]
    public GameState currentState = GameState.Exploration;
    
    [Header("Movement Settings")]
    public float explorationMoveSpeed = 5f;
    public float tileSize = 1f;

    private void Awake()
    {
        // If an instance already exists and it's not this one, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance and make it persist
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void EnterBattle()
    {
        currentState = GameState.Battle;
        // Additional battle setup logic can go here
    }

    public void ExitBattle()
    {
        currentState = GameState.Exploration;
        // Additional cleanup logic can go here
    }

    public bool IsInBattle()
    {
        return currentState == GameState.Battle;
    }
} 