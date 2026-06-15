using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SmallFishSchool : MonoBehaviour
{
    [Header("Resource Settings")]
    [Tooltip("Total amount of health points this entire school contains.")]
    [SerializeField] private float totalHealthPool = 25f;
    [Tooltip("Base health points restored per second of traversal.")]
    [SerializeField] private float baseHealthPerSecond = 5f;

    [Header("Visual Feedback")]
    [Tooltip("The particle system representing individual fish in the swarm.")]
    [SerializeField] private ParticleSystem schoolParticles;
    [Tooltip("Particle burst triggered when the player is actively eating.")]
    [SerializeField] private ParticleSystem consumptionBurstParticles;

    private float currentHealthPool;
    private float tickTimer;
    private const float TICK_INTERVAL = 0.1f; // Dispense health 10 times a second for smooth feedback

    private ParticleSystem.EmissionModule emissionModule;
    private float initialEmissionRate;
    private Collider2D schoolCollider;

    private void Start()
    {
        currentHealthPool = totalHealthPool;
        schoolCollider = GetComponent<Collider2D>();

        // Force the bounding volume to act as a trigger matrix
        schoolCollider.isTrigger = true;

        if (schoolParticles != null)
        {
            emissionModule = schoolParticles.emission;
            initialEmissionRate = emissionModule.rateOverTime.constant;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Early exit if the resource pool is already dried up
        if (currentHealthPool <= 0) return;

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
            tickTimer += Time.deltaTime;

            if (tickTimer >= TICK_INTERVAL)
            {
                tickTimer = 0f;

                // Compute dynamic tick allocation
                float dynamicTickAmount = (baseHealthPerSecond * TICK_INTERVAL);

                // Clamp tick to avoid consuming more than what remains in the pool
                dynamicTickAmount = Mathf.Min(dynamicTickAmount, currentHealthPool);

                // Deplete the composite pool
                currentHealthPool -= dynamicTickAmount;

                // Dispense the resource to your Orca's state script
                // Swap 'PlayerOrcaController' with your actual execution script name if different
                orcaHc.AddHealth(dynamicTickAmount);

                // Play localized consumption visual bursts if assigned
                if (consumptionBurstParticles != null && orcaSpeed > 1f)
                {
                    consumptionBurstParticles.transform.position = other.transform.position;
                    consumptionBurstParticles.Emit(2);
                }

                // Recalibrate particle density based on remaining resource levels
                UpdateSwarmVisuals();
            }
        }
    }

    private void UpdateSwarmVisuals()
    {
        if (schoolParticles == null) return;

        float remainingRatio = currentHealthPool / totalHealthPool;

        if (remainingRatio <= 0f)
        {
            // Stops emission, but lets the last few survivors swim away
            schoolParticles.Stop();
            schoolCollider.enabled = false;
        }
        else
        {
            // Procedurally reduce the structural density of the swarm
            var rate = emissionModule.rateOverTime;
            rate.constant = initialEmissionRate * remainingRatio;
            emissionModule.rateOverTime = rate;
        }
    }

    // Optional utility script reset for debugging or zone respawns
    public void ResetSchoolPool()
    {
        currentHealthPool = totalHealthPool;
        schoolCollider.enabled = true;
        if (schoolParticles != null)
        {
            var rate = emissionModule.rateOverTime;
            rate.constant = initialEmissionRate;
            emissionModule.rateOverTime = rate;
            schoolParticles.Play();
        }
    }
}