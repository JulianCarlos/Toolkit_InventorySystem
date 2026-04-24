using UnityEngine;

public enum DemoGameState
{
    Default,
    InventoryOpen
}

public class DemoGameStateManager : MonoBehaviour
{
    public static DemoGameStateManager Instance { get; private set; }

    public DemoGameState CurrentState { get; private set; } = DemoGameState.Default;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetState(DemoGameState newState)
    {
        CurrentState = newState;
        Cursor.lockState = newState == DemoGameState.Default ? CursorLockMode.None : CursorLockMode.None;
    }
}
