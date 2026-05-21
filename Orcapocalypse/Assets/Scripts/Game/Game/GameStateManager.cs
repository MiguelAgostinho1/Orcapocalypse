using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // The static instance for global access
    public static GameStateManager Instance { get; private set; }

    [Header("Combat Configuration")]
    private PlayerAbility AttackExecuted = null;

    private void Awake()
    {
        // 1. Check if an Instance already exists
        if (Instance == null)
        {
            // 2. If not, this becomes the global Instance
            Instance = this;
            
            // 3. Keep this object alive when loading new scenes
            DontDestroyOnLoad(gameObject); 
        }
        else if (Instance != this)
        {
            // 4. If an instance already exists and it's not this one, destroy this duplicate
            Destroy(gameObject); 
        }
    }

    public void SetAttackExecuted(PlayerAbility ability)
    {
        AttackExecuted = ability;
    }

    public PlayerAbility GetAttackExecuted()
    {
        return AttackExecuted;
    }
}