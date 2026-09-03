using UnityEngine;
using UnityEngine.SceneManagement; // Required to load scenes!

public class SceneChanger : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The exact name of the scene you want to load (e.g., 'Level_02')")]
    [SerializeField] private string _sceneToLoad;

    [Header("Lock Settings")]
    [Tooltip("If true, the player cannot trigger this exit until it is unlocked via script.")]
    public bool isLocked = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Check if it's the Orca
        if (other.CompareTag("Player"))
        {
            // 2. Check if the exit is locked
            if (isLocked)
            {
                Debug.Log("The exit is locked! Finish your tasks first.");
                // TODO: You could trigger a UI popup here later
                return; // Stop the code here so the scene doesn't load
            }

            // 3. Load the scene
            if (!string.IsNullOrEmpty(_sceneToLoad))
            {
                Debug.Log($"Loading Scene: {_sceneToLoad}");
                SceneManager.LoadScene(_sceneToLoad);
            }
            else
            {
                Debug.LogWarning("Scene Changer triggered, but no scene name was typed in the Inspector!");
            }
        }
    }

    // --- Public methods for other scripts to control the exit ---
    public void UnlockExit()
    {
        isLocked = false;
        Debug.Log("Level Exit Unlocked!");
    }

    public void LockExit()
    {
        isLocked = true;
    }
}