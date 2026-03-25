using UnityEngine;

public class PlayerDamagedInvincibility : MonoBehaviour
{
    [SerializeField]
    private float invincibilityDuration;

    private InvicibilityController invicibilityController;

    private void Awake()
    {
        invicibilityController = GetComponent<InvicibilityController>();
    }

    public void StartInvincibility()
    {
        invicibilityController.StartInvincibility(invincibilityDuration);
    }
}

