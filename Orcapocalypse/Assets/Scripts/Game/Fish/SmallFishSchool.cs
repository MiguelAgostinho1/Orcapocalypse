using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SmallFishSchool : MonoBehaviour
{
    [Header("Resource Settings")]
    [Tooltip("Total amount of health points this entire school contains.")]
    [SerializeField] private float _totalHealthPool = 25f;
    [Tooltip("Base health points restored per second of traversal.")]
    [SerializeField] private float _baseHealthPerSecond = 5f;

    [Header("Visual Feedback")]
    [Tooltip("The particle system representing individual fish in the swarm.")]
    [SerializeField] private ParticleSystem _schoolParticles;
    [Tooltip("Particle burst triggered when the player is actively eating.")]
    [SerializeField] private ParticleSystem _consumptionBurstParticles;

    [SerializeField] private float _currentHealthPool;
    private float _tickTimer;
    private const float TICK_INTERVAL = 0.1f; // Dispense health 10 times a second for smooth feedback

    private ParticleSystem.EmissionModule _emissionModule;
    private float _initialEmissionRate;
    private Collider2D _schoolCollider;

    private void Start()
    {
        _currentHealthPool = _totalHealthPool;
        _schoolCollider = GetComponent<Collider2D>();

        // Force the bounding volume to act as a trigger matrix
        _schoolCollider.isTrigger = true;

        if (_schoolParticles != null)
        {
            _emissionModule = _schoolParticles.emission;
            _initialEmissionRate = _emissionModule.rateOverTime.constant;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Early exit if the resource pool is already dried up
        if (_currentHealthPool <= 0) return;

        // Verify collision with the Player Orca
        if (other.CompareTag("Player"))
        {
            // Grab the Rigidbody2D to evaluate traversal velocity vectors
            Rigidbody2D orcaRb = other.GetComponent<Rigidbody2D>();
            HealthController orcaHc = other.GetComponent<HealthController>();

            if (orcaRb == null) return;
            if (orcaHc == null || orcaHc.isInvincible || orcaHc.RemainingHealthPercentage >= 1f) return;

            // Calculate magnitude of movement (supports modern Unity linearVelocity syntax)
            float orcaSpeed = orcaRb.linearVelocity.magnitude;

            // Accumulate traversal time inside the volume boundary
            _tickTimer += Time.deltaTime;

            if (_tickTimer >= TICK_INTERVAL)
            {
                _tickTimer = 0f;

                // Compute dynamic tick allocation
                float dynamicTickAmount = (_baseHealthPerSecond * TICK_INTERVAL);

                // Clamp tick to avoid consuming more than what remains in the pool
                dynamicTickAmount = Mathf.Min(dynamicTickAmount, _currentHealthPool);

                // Deplete the composite pool
                _currentHealthPool -= dynamicTickAmount;

                // Dispense the resource to your Orca's state script
                // Swap 'PlayerOrcaController' with your actual execution script name if different
                orcaHc.AddHealth(dynamicTickAmount);

                // Play localized consumption visual bursts if assigned
                if (_consumptionBurstParticles != null && orcaSpeed > 1f)
                {
                    _consumptionBurstParticles.transform.position = other.transform.position;
                    _consumptionBurstParticles.Emit(2);
                }

                // Recalibrate particle density based on remaining resource levels
                UpdateSwarmVisuals();
            }
        }
    }

    private void UpdateSwarmVisuals()
    {
        if (_schoolParticles == null) return;

        float remainingRatio = _currentHealthPool / _totalHealthPool;

        if (remainingRatio <= 0f)
        {
            // Stops emission, but lets the last few survivors swim away
            _schoolParticles.Stop();
            _schoolCollider.enabled = false;
        }
        else
        {
            // Procedurally reduce the structural density of the swarm
            var rate = _emissionModule.rateOverTime;
            rate.constant = _initialEmissionRate * remainingRatio;
            _emissionModule.rateOverTime = rate;
        }
    }

    // Optional utility script reset for debugging or zone respawns
    public void ResetSchoolPool()
    {
        _currentHealthPool = _totalHealthPool;
        _schoolCollider.enabled = true;
        if (_schoolParticles != null)
        {
            var rate = _emissionModule.rateOverTime;
            rate.constant = _initialEmissionRate;
            _emissionModule.rateOverTime = rate;
            _schoolParticles.Play();
        }
    }
}