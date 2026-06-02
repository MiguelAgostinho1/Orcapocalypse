using System;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float _bumpDamage = 5f;
    [SerializeField] private float _selfDamage = 10f;
    [SerializeField] private float _hitCooldown = 1f;

    [Header("Rebound Settings")]
    [SerializeField] private float _reboundForce = 2f;
    [SerializeField] private float _stunDuration = 1f;

    private Rigidbody2D _rb;
    private PlayerMovement _playerMovement;
    private PlayerCombat _playerCombat;
    private HealthController _orcaHealth;
    private float _nextHitTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCombat = GetComponent<PlayerCombat>();
        _orcaHealth = GetComponent<HealthController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Time.time < _nextHitTime) return;

        if (collision.gameObject.TryGetComponent<YachtDestroyController>(out var yachtDeath) && yachtDeath.IsSinking)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Yacht"))
        {
            if (!collision.gameObject.TryGetComponent<HealthController>(out var boatHealth)) return;

            _nextHitTime = Time.time + _hitCooldown;

            // Check if PlayerCombat has an active ability running right now
            PlayerAbility activeMove = _playerCombat.CurrentActiveAbility;

            if (activeMove != null)
            {
                // Attack Successful! Use the damage value directly from the asset
                boatHealth.TakeDamage(activeMove.GetDamage());
            }
            else
            {
                // Standard accidental bump
                boatHealth.TakeDamage(_bumpDamage);
                _orcaHealth.TakeDamage(_selfDamage);

                ApplyReboundStun(collision.GetContact(0).point);
            }
        }
    }

    private void ApplyReboundStun(Vector2 hitPoint)
    {
        Vector2 reboundDir = ((Vector2)transform.position - hitPoint).normalized;

        _playerMovement.Stun(_stunDuration);

        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(reboundDir * _reboundForce, ForceMode2D.Impulse);

        Debug.Log("Orca recoiled and is briefly stunned!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Yacht"))
        {
            _orcaHealth.TakeDamage(_selfDamage);
            _playerMovement.Stun(0.5f);
            Debug.Log("Orca is being scraped by the sinking wreck!");
        }
    }
}