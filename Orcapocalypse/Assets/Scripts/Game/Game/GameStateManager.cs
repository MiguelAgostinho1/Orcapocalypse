using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameStateManager : MonoBehaviour
{
    // The static instance for global access
    public static GameStateManager Instance { get; private set; }

    [Header("Combat Configuration")]
    private PlayerAbility _attackExecuted = null;

    [Header("Pause Menu UI")]
    public GameObject pauseMenuUI;
    public GameObject resumeButton;
    public bool isPaused = false;

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

    private void Update()
    {
        // Safe check for Keyboard and Gamepad before accessing their properties to prevent crashes in the case they are not connected or initialized.
        bool escapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool startPressed = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;

        if (escapePressed || startPressed)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // --- Pause Logic ---
    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resume the game
        isPaused = false;

        if (pauseMenuUI != null) { pauseMenuUI.SetActive(false); }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f; // Freeze the game
        isPaused = true;

        if (pauseMenuUI != null) 
        {
            pauseMenuUI.SetActive(true);

            if (resumeButton != null)
            {
                // Clear any previously selected UI element to avoid issues with navigation and select the resume button
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(resumeButton);
            }
        }
    }

    public void ExitToDesktop()
    {
        Debug.Log("Exiting to desktop...");
        Application.Quit();
    }

    // --- Combat Logic ---
    public void SetAttackExecuted(PlayerAbility ability)
    {
        _attackExecuted = ability;
    }

    public PlayerAbility GetAttackExecuted()
    {
        return _attackExecuted;
    }
}