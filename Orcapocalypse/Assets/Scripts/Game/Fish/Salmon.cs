using UnityEngine;

public class Salmon : Fish
{
    [Header("Salmon Specific Wander Settings")]
    [Tooltip("How often the salmon picks a new random direction to swim while idling.")]
    [SerializeField] private float _wanderInterval = 3f;
    [Tooltip("Adds random variety to the wander interval so all salmon don't turn at the exact same time.")]
    [SerializeField] private float _wanderVariance = 1f;

    [Header("Tank Stats")]
    [Tooltip("Total health pool of this heavy salmon variety.")]
    [SerializeField] private float _maxHealth = 40f;
    [Tooltip("The time (in seconds) the salmon remains immune to damage after being hit once. Prevents multi-frame instant kills.")]
    [SerializeField] private float _damageGraceWindow = 0.4f;

    private float _currentHealth;
    private float _currentWanderTimer;
    private float _nextWanderInterval;
    private float _gracePeriodTimer;

    /// <summary>
    /// Overrides the base initialization hook to set up custom wander timing.
    /// </summary>
    protected override void InitializeFish()
    {
        _currentHealth = _maxHealth;
        SetNextWanderTime();
        // Give the salmon an immediate random starting direction so they don't all look identical on spawn
        PickRandomWanderDirection();
    }

    /// <summary>
    /// Overrides the state engine loop to update directions based on whether it is idling or panicking.
    /// </summary>
    protected override void HandleStateUpdate()
    {
        // Tick down the damage grace window timer if active
        if (_gracePeriodTimer > 0f)
        {
            _gracePeriodTimer -= Time.deltaTime;
        }

        switch (currentState)
        {
            case FishState.Idle:
                HandleWanderBehavior();
                break;

            case FishState.Fleeing:
                HandleEscapeBehavior();
                break;
        }
    }

    /// <summary>
    /// Smoothly rotates the wander direction over time.
    /// </summary>
    private void HandleWanderBehavior()
    {
        _currentWanderTimer += Time.deltaTime;

        if (_currentWanderTimer >= _nextWanderInterval)
        {
            _currentWanderTimer = 0f;
            SetNextWanderTime();
            PickRandomWanderDirection();
        }
    }

    /// <summary>
    /// Calculates a vector pointing directly away from the player Orca.
    /// </summary>
    private void HandleEscapeBehavior()
    {
        if (playerTransform == null) return;

        // Look purely at the horizontal difference
        float deltaX = transform.position.x - playerTransform.position.x;

        if (Mathf.Approximately(deltaX, 0f))
        {
            // If perfectly aligned on X, escape in a random direction
            PickRandomWanderDirection();
        }
        else
        {
            // Swim in the direction of the sign (positive difference means swim right, negative means left)
            moveDirection = new Vector2(Mathf.Sign(deltaX), 0f);
        }
    }

    /// <summary>
    /// Visual/Audio/Gameplay trigger for when the Salmon notices the Orca.
    /// </summary>
    protected override void OnFleeStart()
    {
        // Optional: Could trigger a quick tail-swish animation or play an alert sound effect here.
        // For now, it immediately forces an escape recalculation
        HandleEscapeBehavior();
    }

    /// <summary>
    /// Resets the salmon back into a calm wander state once the Orca leaves the detection matrix.
    /// </summary>
    protected override void OnFleeEnd()
    {
        _currentWanderTimer = 0f;
        SetNextWanderTime();
        PickRandomWanderDirection();
    }

    // --- Combat Interceptions & Collisions ---

    /// <summary>
    /// Intercepts the player collision to block the base Fish script's instant-kill behavior.
    /// </summary>
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_gracePeriodTimer > 0f)
            {
                Debug.Log($"{gameObject.name} is temporarily immune to damage during grace period frames.");
                return;
            }

            PlayerCombat playerCombat = other.GetComponent<PlayerCombat>();
            HealthController playerHealth = other.GetComponent<HealthController>();

            // Check if the Orca is actively using an attack ability (like Surge)
            bool isAttacking = playerCombat != null && playerCombat.CurrentActiveAbility != null;

            if (isAttacking)
            {
                // Determine how much damage the attack deals
                float damageToTake = playerCombat.CurrentActiveAbility.GetDamage();

                ProcessDamage(damageToTake, playerHealth);
            }
        }
    }

    /// <summary>
    /// Subtracts health pool points and processes either execution or survival fleeing responses.
    /// </summary>
    private void ProcessDamage(float damageAmount, HealthController playerHealth)
    {
        _currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} tanked a hit! Spent {damageAmount} HP. Remaining: {_currentHealth}/{_maxHealth}");

        if (_currentHealth <= 0f)
        {
            Debug.Log($"{gameObject.name} health depleted. Consumed!");
            OnEaten(playerHealth); // Safely trigger standard base depletion/destruction sequences
        }
        else
        {
            _gracePeriodTimer = _damageGraceWindow;

            // Force the salmon to immediately flip direction and start fleeing if it wasn't already
            HandleEscapeBehavior();
        }
    }

    // --- Helper Methods ---

    private void PickRandomWanderDirection()
    {
        // Flip a coin: 50% chance to go Right (1,0), 50% chance to go Left (-1,0)
        moveDirection = (Random.value > 0.5f) ? Vector2.right : Vector2.left;
    }

    private void SetNextWanderTime()
    {
        // Distorts the interval slightly so multiple salmon look naturally unsynchronized
        _nextWanderInterval = _wanderInterval + Random.Range(-_wanderVariance, _wanderVariance);
    }
}