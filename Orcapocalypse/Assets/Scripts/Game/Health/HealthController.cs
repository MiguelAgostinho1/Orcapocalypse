using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [SerializeField]
    private float currentHealth;
    [SerializeField]
    private float maxHealth;

    // Helper Method for UI Health Bar (Useful in the future)
    public float RemainingHealthPercentage
    {
        get
        {
            return currentHealth / maxHealth;
        }
    }

    public bool isInvincible;
    public UnityEvent OnDied;
    public UnityEvent OnDamage;
    public UnityEvent OnHealthChanged;

    // Handles the damage calculations
    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0) return;
        if (isInvincible) return;

        // Mathf.Max ensures currentHealth is never a negative number
        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        OnHealthChanged.Invoke();
        GamepadHaptics.Instance.VibrateDamage();

        if (currentHealth == 0)
        {
            OnDied.Invoke();

        }
        else
        {
            OnDamage.Invoke();
        }
    }

    // Mehod to add Health to a target
    // Useful for Power-Ups
    public void AddHealth(float amountToAdd)
    {
        if (currentHealth >= maxHealth) return;

        // Mathf.Min ensures currentHealth is never bigger than maxHealth
        currentHealth = Mathf.Min(maxHealth, currentHealth + amountToAdd);
        OnHealthChanged.Invoke();
    }
}
