using UnityEngine;

public class Shark : Fish
{
    public enum SharkState
    {
        Patrol,
        Avoidance,
        DefensiveAggression,
        TonicImmobility,
        Consumed
    }

    [Header("FSM State Matrix")]
    [SerializeField] private SharkState _sharkState = SharkState.Patrol;

    [Header("Detection Radii")]
    [Tooltip("Condition 1: Distance to enter Avoidance state.")]
    [SerializeField] private float _outerRadius = 8f;
    [Tooltip("Condition 2: Distance to trigger Defensive Aggression if cornered.")]
    [SerializeField] private float _innerRadius = 3.5f;

    [Header("Movement Speeds per State")]
    [SerializeField] private float _patrolSpeed = 2f;
    [SerializeField] private float _avoidanceSpeed = 4.5f;
    [SerializeField] private float _defensiveAggressionSpeed = 6f;

    [Header("Combat Settings")]
    [SerializeField] private float _damageToPlayer = 20f;
    [Tooltip("Condition 4: How long the shark stays paralyzed before recovering to Avoidance.")]
    [SerializeField] private float _tonicImmobilityDuration = 5f;
    [Tooltip("Prevents instant execution. The minimum time (in seconds) the shark must remain immobilized before the player can eat it.")]
    [SerializeField] private float _consumptionGraceWindow = 0.4f;
    [SerializeField] private float _flipDeadzoneX = 0.5f;

    [Header("Wander Configurations")]
    [SerializeField] private float _wanderInterval = 3f;

    private float _wanderTimer;
    private float _tonicTimer;
    private float _gracePeriodTimer;

    protected override void InitializeFish()
    {
        _sharkState = SharkState.Patrol;
        ResetWander();
    }

    protected override void HandleStateUpdate()
    {
        if (playerTransform == null)
        {
            FallbackToPatrol();
            return;
        }

        float distanceToOrca = Vector2.Distance(transform.position, playerTransform.position);

        // Core FSM Execution Loop
        switch (_sharkState)
        {
            case SharkState.Patrol:
                HandlePatrolState(distanceToOrca);
                break;

            case SharkState.Avoidance:
                HandleAvoidanceState(distanceToOrca);
                break;

            case SharkState.DefensiveAggression:
                HandleDefensiveAggressionState(distanceToOrca);
                break;

            case SharkState.TonicImmobility:
                HandleTonicImmobilityState();
                break;

            case SharkState.Consumed:
                // Do nothing, handled by destruction sequences
                break;
        }
    }

    // State Logic Handlers

    private void HandlePatrolState(float distanceToOrca)
    {
        // Transition Check 1: Orca enters Outer Radius -> Avoidance
        if (distanceToOrca <= _outerRadius)
        {
            TransitionToState(SharkState.Avoidance);
            return;
        }

        // Behavior: Casual horizontal wandering
        _wanderTimer += Time.deltaTime;
        if (_wanderTimer >= _wanderInterval)
        {
            ResetWander();
        }
    }

    private void HandleAvoidanceState(float distanceToOrca)
    {
        // Transition Check: Orca leaves Outer Radius -> Patrol
        if (distanceToOrca > _outerRadius)
        {
            TransitionToState(SharkState.Patrol);
            return;
        }

        // Transition Check 2: Orca penetrates Inner Radius -> Defensive Aggression
        if (distanceToOrca <= _innerRadius)
        {
            TransitionToState(SharkState.DefensiveAggression);
            return;
        }

        // Behavior: Avoidance-oriented pattern (Flee horizontally away from Orca)
        CalculateHorizontalDirection(awayFromPlayer: true);
    }

    private void HandleDefensiveAggressionState(float distanceToOrca)
    {
        // Transition Check: Orca backs off outside Inner Radius -> Return to Avoidance
        if (distanceToOrca > _innerRadius && distanceToOrca <= _outerRadius)
        {
            TransitionToState(SharkState.Avoidance);
            return;
        }
        else if (distanceToOrca > _outerRadius)
        {
            TransitionToState(SharkState.Patrol);
            return;
        }

        // Behavior: Charge horizontally TOWARDS the player
        CalculateHorizontalDirection(awayFromPlayer: false);
    }

    private void HandleTonicImmobilityState()
    {
        if (_gracePeriodTimer > 0f)
        {
            _gracePeriodTimer -= Time.deltaTime;
        }

        // Condition 4: Stun Timer Expired loop
        _tonicTimer -= Time.deltaTime;
        if (_tonicTimer <= 0f)
        {
            Debug.Log("Stun Timer Expired! Shark recovering to Avoidance state.");
            transform.rotation = Quaternion.identity; // Snap right-side up
            TransitionToState(SharkState.Avoidance);
        }
    }

    // Movement & Directives Mapping

    private void CalculateHorizontalDirection(bool awayFromPlayer)
    {
        float deltaX = playerTransform.position.x - transform.position.x;

        if (Mathf.Abs(deltaX) > _flipDeadzoneX)
        {
            float directionSign = Mathf.Sign(deltaX);
            // Invert the vector sign if we are actively avoiding the Orca
            moveDirection = new Vector2(awayFromPlayer ? -directionSign : directionSign, 0f);
        }
    }

    protected override void ExecuteMovement()
    {
        if (_sharkState == SharkState.TonicImmobility || _sharkState == SharkState.Consumed)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float targetSpeed = _patrolSpeed;
        if (_sharkState == SharkState.Avoidance) targetSpeed = _avoidanceSpeed;
        if (_sharkState == SharkState.DefensiveAggression) targetSpeed = _defensiveAggressionSpeed;

        // Apply basic calculated horizontal speed matrix 
        rb.linearVelocity = moveDirection * targetSpeed;

        // Delegate the physical clamping checks to the base Fish engine
        HandleUniversalBoundaries();

        // --- VISUAL FLIPPING ---
        // Start with our intended movement direction
        float visualFacingX = moveDirection.x;

        // FIX: If we are trying to flee (Avoidance) but are pinned against a boundary,
        // force the sprite to face the player instead of staring blankly into the wall.
        if (_sharkState == SharkState.Avoidance)
        {
            bool pinnedLeft = (transform.position.x <= minXBound && moveDirection.x < 0f);
            bool pinnedRight = (transform.position.x >= maxXBound && moveDirection.x > 0f);

            if (pinnedLeft || pinnedRight)
            {
                // Invert the visual facing direction so it eyes the player defensively
                visualFacingX = -moveDirection.x;
            }
        }

        // Apply the flip using our determined visual facing value
        if (visualFacingX > 0f)
        {
            spriteRenderer.flipX = spriteFacesLeftByDefault;
        }
        else if (visualFacingX < 0f)
        {
            spriteRenderer.flipX = !spriteFacesLeftByDefault;
        }
    }

    /// <summary>
    /// Custom override dictating how the Shark uniquely handles boundaries compared to simple fish.
    /// </summary>
    protected override void OnBoundaryHit(Vector2 reboundDirection)
    {
        // If casually wandering, turn around nicely like normal fish
        if (_sharkState == SharkState.Patrol)
        {
            moveDirection = reboundDirection;
            _wanderTimer = 0f;
        }
        // If in Avoidance or DefensiveAggression, do NOT turn around! 
        // We stay pinned facing our target direction so the player can corner us.
    }

    // Combat Interceptions & Collisions

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (_sharkState == SharkState.Consumed) return;

        if (other.CompareTag("Player"))
        {
            HealthController health = other.GetComponent<HealthController>();
            PlayerCombat playerCombat = other.GetComponent<PlayerCombat>();

            if (health != null)
            {
                // STEP 2 (Condition 5): If already immobilized, subsequent Orca collision consumes the liver!
                if (_sharkState == SharkState.TonicImmobility)
                {
                    if (_gracePeriodTimer <= 0f)
                    {
                        Debug.Log("Condition 5 Met: Orca Consumes Shark's Liver!");
                        _sharkState = SharkState.Consumed;
                        OnEaten(health);
                    }
                    else
                    {
                        // Ignore continuous frame overlaps from the original flipping hit
                        Debug.Log("Shark is immune to immediate consumption during initial hit frames.");
                    }
                    return;
                }

                // STEP 1 (Condition 3): Check for a tactical Strike from Below while active
                bool isBelowShark = other.transform.position.y < transform.position.y;
                bool isAttacking = playerCombat != null && playerCombat.CurrentActiveAbility != null;

                if (_sharkState == SharkState.DefensiveAggression && isBelowShark && isAttacking)
                {
                    // Condition 3 Met -> Enter Tonic Immobility
                    TriggerTonicImmobility();
                }
                else
                {
                    // Punish standard collisions inside the active combat zones
                    Debug.Log("Orca struck an active defensive shark zone. Dealing damage.");
                    health.TakeDamage(_damageToPlayer);
                }
            }
        }
    }

    private void TriggerTonicImmobility()
    {
        _sharkState = SharkState.TonicImmobility;
        _tonicTimer = _tonicImmobilityDuration;
        _gracePeriodTimer = _consumptionGraceWindow;
        rb.linearVelocity = Vector2.zero;

        // Visual feedback: Flip upside down (180 degrees around Z axis) to signify immobility
        transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        Debug.Log("Condition 3 Met: Hit from Below! Entering Tonic Immobility state.");
    }

    // Helper & Utility Routines

    private void TransitionToState(SharkState newState)
    {
        _sharkState = newState;
        if (newState == SharkState.Patrol) ResetWander();
    }

    private void ResetWander()
    {
        _wanderTimer = 0f;
        moveDirection = (Random.value > 0.5f) ? Vector2.right : Vector2.left;
    }

    private void FallbackToPatrol()
    {
        if (_sharkState != SharkState.Patrol && _sharkState != SharkState.TonicImmobility && _sharkState != SharkState.Consumed)
        {
            TransitionToState(SharkState.Patrol);
        }
    }

    // Draws the FSM radius boundaries directly inside the Unity scene window for real-time design adjustments
    override protected void OnDrawGizmosSelected()
    {
        // Outer Radius (Green)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _outerRadius);

        // Inner Radius (Red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _innerRadius);
    }
}