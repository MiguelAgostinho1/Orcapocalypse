using UnityEngine;

public class Harpoon : MonoBehaviour
{
    [Header("Settings")]
    public float lifeTime = 4f;
    public int damageAmount = 1;

    // This flag ensures the harpoon only deals damage once, even if it passes through multiple hitboxes on the Orca.
    private bool _hasPierced = false;

    void Start()
    {
        // Acts as maximum rope length / failsafe
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!_hasPierced)
            {
                // Check if the object we hit has a HealthController
                HealthController health = other.GetComponent<HealthController>();

                if (health != null)
                {
                    health.TakeDamage(damageAmount);
                    other.GetComponent<PlayerMovement>()?.Stun(0.1f);
                }

                _hasPierced = true;
            }
        }
        else if (other.CompareTag("Environment"))
        {
            Destroy(gameObject); // The harpoon should stop if it hits the seabed
        }
    }
}