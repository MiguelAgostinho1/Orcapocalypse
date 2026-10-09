using UnityEngine;

[RequireComponent(typeof(SceneChanger))] // Ensures SceneChanger is on the same object
public class LevelObjectiveManager: MonoBehaviour
{
    private SceneChanger _sceneChanger;
    private int _yachtsRemaining;

    private void Awake()
    {
        _sceneChanger = GetComponent<SceneChanger>();
    }

    private void Start()
    {
        // Dynamically count how many yachts are in this specific scene on load
        _yachtsRemaining = GameObject.FindGameObjectsWithTag("Yacht").Length;

        // If there are no yachts, open the exit immediately
        if (_yachtsRemaining <= 0)
        {
            UnlockLevelExit();
        }
    }

    private void OnEnable()
    {
        // Subscribe to the event
        YachtDestroyController.OnYachtDestroyed += HandleYachtSunk;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        YachtDestroyController.OnYachtDestroyed -= HandleYachtSunk;
    }

    private void HandleYachtSunk()
    {
        _yachtsRemaining--;

        if (_yachtsRemaining <= 0)
        {
            UnlockLevelExit();
        }
    }

    private void UnlockLevelExit()
    {
        Debug.Log("All yachts destroyed! Exit unlocked.");
      _sceneChanger.UnlockExit(); // Call the method to unlock the exit
    }
}