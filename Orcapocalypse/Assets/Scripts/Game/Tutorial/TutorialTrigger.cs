using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public DialogSequence startingDialog;

    private void Start()
    {
        // As soon as the scene loads, tell the DialogManager to start this sequence!
        if (startingDialog != null)
        {
            DialogManager.Instance.StartDialog(startingDialog);
        }
    }
}