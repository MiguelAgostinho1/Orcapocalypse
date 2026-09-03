using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    public SceneChanger levelExit;

    [Header("Objectives")]
    public int requiredBallHits = 3;
    public int requiredBoatHits = 3;

    [SerializeField]
    private int _currentBallHits = 0;
    [SerializeField]
    private int _currentBoatHits = 0;

    // We subscribe to the events when this manager turns on
    private void OnEnable()
    {
        TutorialTarget.OnTargetHit += HandleTargetHit;
    }

    // Always unsubscribe when destroyed to prevent memory leaks!
    private void OnDisable()
    {
        TutorialTarget.OnTargetHit -= HandleTargetHit;
    }

    private void HandleTargetHit(TutorialTarget.TargetType type)
    {
        if (type == TutorialTarget.TargetType.Ball)
        {
            _currentBallHits++;
            Debug.Log($"Ball hit! ({_currentBallHits}/{requiredBallHits})");
        }
        else if (type == TutorialTarget.TargetType.Boat)
        {
            _currentBoatHits++;
            Debug.Log($"Boat hit! ({_currentBoatHits}/{requiredBoatHits})");
        }

        CheckTutorialCompletion();
    }

    private void CheckTutorialCompletion()
    {
        if (_currentBallHits >= requiredBallHits && _currentBoatHits >= requiredBoatHits)
        {
            Debug.Log("Tutorial Complete! Unlocking Exit.");
            levelExit.UnlockExit();

            // Optional: Unsubscribe early so we stop counting after it's unlocked
            TutorialTarget.OnTargetHit -= HandleTargetHit;
        }
    }
}