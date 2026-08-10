using UnityEngine;

public class PlayerDamagedInvincibility : MonoBehaviour
{
    [SerializeField]
    private float _invincibilityDuration;

    private InvicibilityController _invicibilityController;

    private void Awake()
    {
        _invicibilityController = GetComponent<InvicibilityController>();
    }

    public void StartInvincibility()
    {
        _invicibilityController.StartInvincibility(_invincibilityDuration);
    }
}

