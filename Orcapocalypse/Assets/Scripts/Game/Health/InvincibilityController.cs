using System.Collections;
using UnityEngine;

public class InvicibilityController : MonoBehaviour
{
    private HealthController healthController;
    private Coroutine _invincibilityRoutine;

    private void Awake()
    {
        healthController = GetComponent<HealthController>();
    }

    // Public method to trigger the invincibility state
    // Useful for Post-Hit frames or Power-Ups
    public void StartInvincibility(float invincibilityDuration)
    {
        // Stop all coroutines to ensure multiple hits don't cause the invincibility to expire prematurely
        if (_invincibilityRoutine != null) StopCoroutine(_invincibilityRoutine);
        _invincibilityRoutine = StartCoroutine(InvincibilityCoroutine(invincibilityDuration));
    }

    // Handles the timed transition of the invincibility flag
    private IEnumerator InvincibilityCoroutine(float invincibilityDuration)
    {
        healthController.isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        healthController.isInvincible = false;
    }

}
