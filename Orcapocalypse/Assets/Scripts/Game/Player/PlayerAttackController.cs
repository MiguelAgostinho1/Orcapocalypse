using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float _ramDamage = 20f;
    [SerializeField] private float _bumpDamage = 5f;
    [SerializeField] private float _selfDamage = 10f;
    [SerializeField] private float _hitCooldown = 1f;

    [Header("Rebound Settings")]
    [SerializeField] private float _reboundForce = 2f;
    [SerializeField] private float _stunDuration = 1f;

    private Rigidbody2D _rb;
    private PlayerMovement _playerMovement;
    private HealthController _orcaHealth;
    private float _nextHitTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerMovement = GetComponent<PlayerMovement>();
        _orcaHealth = GetComponent<HealthController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only proceed if it's been long enough since the last hit
        if (Time.time < _nextHitTime) return;

        if (collision.gameObject.TryGetComponent<YachtDestroyController>(out var yachtDeath))
        {
            // If the boat is sinking: Do nothing here. 
            // OnTriggerStay2D will handle the "crushing" damage instead.
            if (yachtDeath.IsSinking) return;
        }

        // Check if the orca hit the Yacht
        if (collision.gameObject.CompareTag("Yacht"))
        {
            if (!collision.gameObject.TryGetComponent<HealthController>(out var boatHealth)) return;

            // Set the cooldown here
            _nextHitTime = Time.time + _hitCooldown;

            if (_playerMovement.IsSurging)
            {
                boatHealth.TakeDamage(_ramDamage);
            }
            else
            {
                boatHealth.TakeDamage(_bumpDamage);
                _orcaHealth.TakeDamage(_selfDamage);

                // --- 1. Calculate Rebound ---
                Vector2 hitPoint = collision.GetContact(0).point;
                Vector2 reboundDir = ((Vector2)transform.position - hitPoint).normalized;

                // --- 2. STUN ORCA ---
                _playerMovement.Stun(_stunDuration);

                // --- 3. APPLY FORCE ---
                _rb.linearVelocity = Vector2.zero;
                _rb.AddForce(reboundDir * _reboundForce, ForceMode2D.Impulse);

                Debug.Log("Orca recoiled and is briefly stunned!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we are touching the sinking wreck
        if (collision.CompareTag("Yacht"))
        {
            // Damage Orca for being too close to the sinking yacht
            _orcaHealth.TakeDamage(_selfDamage);

            // Small visual feedback for the player to show the siking yacht hurts
            _playerMovement.Stun(0.5f);

            Debug.Log("Orca is being scraped by the sinking wreck!");
        }
    }
}
