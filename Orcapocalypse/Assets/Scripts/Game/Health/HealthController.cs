using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [SerializeField]
    private float _currentHealth;
    [SerializeField]
    private float _maxHealth;

    // Helper Method for UI Health Bar (Useful in the future)
    public float RemainingHealthPercentage
    {
        get
        {
            return _currentHealth / _maxHealth;
        }
    }

    public bool isInvincible;
    public UnityEvent OnDied;
    public UnityEvent OnDamage;
    public UnityEvent OnHealthChanged;

    // Handles the damage calculations
    public void TakeDamage(float damageAmount)
    {
        if (_currentHealth <= 0) return;
        if (isInvincible) return;

        // Mathf.Max ensures currentHealth is never a negative number
        _currentHealth = Mathf.Max(0, _currentHealth - damageAmount);
        OnHealthChanged.Invoke();

        if (_currentHealth == 0)
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
        if (_currentHealth >= _maxHealth) return;

        // Mathf.Min ensures currentHealth is never bigger than maxHealth
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amountToAdd);
        OnHealthChanged.Invoke();
    }
}
