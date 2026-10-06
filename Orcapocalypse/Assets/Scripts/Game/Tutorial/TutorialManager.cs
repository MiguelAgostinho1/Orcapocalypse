using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    public SceneChanger levelExit;
    public TextMeshProUGUI BallObjectiveText;
    public TextMeshProUGUI BoatObjectiveText;

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
            BallObjectiveText.text = $"Attack the Ball: {_currentBallHits}/{requiredBallHits}";
            Debug.Log($"Ball hit! ({_currentBallHits}/{requiredBallHits})");
        }
        else if (type == TutorialTarget.TargetType.Boat)
        {
            _currentBoatHits++;
            BoatObjectiveText.text = $"Attack the Boat: {_currentBoatHits}/{requiredBoatHits}";
            Debug.Log($"Boat hit! ({_currentBoatHits}/{requiredBoatHits})");
        }

        CheckTutorialCompletion();
    }

    private void CheckTutorialCompletion()
    {
        if (_currentBallHits >= requiredBallHits && _currentBoatHits >= requiredBoatHits)
        {
            BallObjectiveText.text = "Tutorial Complete! Unlocking Exit.";
            BoatObjectiveText.text = "";
            Debug.Log("Tutorial Complete! Unlocking Exit.");
            levelExit.UnlockExit();

            // Optional: Unsubscribe early so we stop counting after it's unlocked
            TutorialTarget.OnTargetHit -= HandleTargetHit;
        }
    }
}