using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public abstract class Fish : MonoBehaviour
{
    public enum FishState { Idle, Fleeing, Eaten }

    [Header("Base Nutrition Settings")]
    [Tooltip("How much health this fish restores to the Orca when consumed.")]
    [SerializeField] protected float nutritionValue = 20f;

    [Header("Base Movement Settings")]
    [SerializeField] protected float baseMoveSpeed = 3f;
    [SerializeField] protected float fleeMoveSpeed = 6f;
    [SerializeField] protected float turnSpeed = 15f;
    [Tooltip("Check this if your raw sprite image naturally faces LEFT. Uncheck if it faces RIGHT.")]
    [SerializeField] protected bool spriteFacesLeftByDefault = true;

    [Header("Player Detection")]
    [Tooltip("Distance at which this fish notices the Orca and reacts.")]
    [SerializeField] protected float detectionRadius = 5f;

    [Header("Level Boundaries (Universal)")]
    [Tooltip("The leftmost X coordinate this fish can travel to.")]
    [SerializeField] protected float minXBound = -82f;
    [Tooltip("The rightmost X coordinate this fish can travel to.")]
    [SerializeField] protected float maxXBound = 82f;

    // Shared Component References
    protected Rigidbody2D rb;
    protected Collider2D fishCollider;
    protected SpriteRenderer spriteRenderer;
    protected Transform playerTransform;

    // State Tracking
    protected FishState currentState = FishState.Idle;
    protected Vector2 moveDirection;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        fishCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Fail-safe setups: ensures mechanics work out of the box without manual inspector tweaking
        if (fishCollider != null) fishCollider.isTrigger = true;
        if (rb != null) rb.gravityScale = 0f;

    }

    protected virtual void Start()
    {
        // Automatically attempt to locate the player via tag if not explicitly set
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        InitializeFish();
    }

    protected virtual void Update()
    {
        if (currentState == FishState.Eaten) return;

        HandleDetection();
        HandleStateUpdate();
    }

    protected virtual void FixedUpdate()
    {
        if (currentState == FishState.Eaten)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        ExecuteMovement();
    }

    /// <summary>
    /// Virtual initialization method for sub-classes to run custom setup logic.
    /// </summary>
    protected virtual void InitializeFish() { }

    /// <summary>
    /// Checks if the fish has reached a boundary, clamps its position, 
    /// and triggers the reaction behavior.
    /// </summary>
    protected void HandleUniversalBoundaries()
    {
        if (transform.position.x <= minXBound && moveDirection.x < 0f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            transform.position = new Vector3(minXBound, transform.position.y, transform.position.z);

            OnBoundaryHit(Vector2.right); // Pass the rebound direction (Right)
        }
        else if (transform.position.x >= maxXBound && moveDirection.x > 0f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            transform.position = new Vector3(maxXBound, transform.position.y, transform.position.z);

            OnBoundaryHit(Vector2.left); // Pass the rebound direction (Left)
        }
    }

    /// <summary>
    /// Dictates how a fish reacts to hitting a wall. 
    /// Generic fish (like Salmon) will simply turn around by default.
    /// </summary>
    protected virtual void OnBoundaryHit(Vector2 reboundDirection)
    {
        moveDirection = reboundDirection;
    }


    /// <summary>
    /// Tracks distance to the player and handles transitioning between Idle and Fleeing states.
    /// </summary>
    protected virtual void HandleDetection()
    {
        if (playerTransform == null) return;

        // Track horizontal distance to player
        float distanceToPlayer = Mathf.Abs(transform.position.x - playerTransform.position.x);

        if (distanceToPlayer <= detectionRadius && currentState != FishState.Fleeing)
        {
            currentState = FishState.Fleeing;
            OnFleeStart();
        }
        else if (distanceToPlayer > detectionRadius * 1.3f && currentState == FishState.Fleeing)
        {
            currentState = FishState.Idle;
            OnFleeEnd();
        }
    }

    /// <summary>
    /// Executes the actual velocity and rotation changes on the Rigidbody2D.
    /// </summary>
    protected virtual void ExecuteMovement()
    {
        float currentSpeed = (currentState == FishState.Fleeing) ? fleeMoveSpeed : baseMoveSpeed;

        // Force movement strictly on the X axis
        rb.linearVelocity = new Vector2(moveDirection.x * currentSpeed, 0f);

        HandleUniversalBoundaries();

        // Sprite Renderer flipping logic
        if (moveDirection.x > 0f)
        {
            // Moving Right: flip if image faces left naturally
            spriteRenderer.flipX = spriteFacesLeftByDefault;
        }
        else if (moveDirection.x < 0f)
        {
            // Moving Left: don't flip if image faces left naturally
            spriteRenderer.flipX = !spriteFacesLeftByDefault;
        }
    }

    /// <summary>
    /// Triggers when the Orca collides with the fish's physical body.
    /// </summary>
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == FishState.Eaten) return;

        if (other.CompareTag("Player"))
        {
            HealthController health = other.GetComponent<HealthController>();
            if (health != null && !health.isInvincible && health.RemainingHealthPercentage < 1f)
            {
                OnEaten(health);
            }
        }
    }

    /// <summary>
    /// Core logic for consumption. Can be overridden to handle unique audio, score events, or particle splashes.
    /// </summary>
    protected virtual void OnEaten(HealthController playerHealth)
    {
        currentState = FishState.Eaten;
        fishCollider.enabled = false;

        playerHealth.AddHealth(nutritionValue);

        // Default behavior: instant object pool return or destruction.
        Destroy(gameObject);
    }

    // Abstract or virtual state hooks for easy sub-class overrides
    protected virtual void HandleStateUpdate() { }
    protected virtual void OnFleeStart() { }
    protected virtual void OnFleeEnd() { }

    // Utility visualization tool for designing levels in the scene view
    protected virtual void OnDrawGizmosSelected()
    {
        // Draw a horizontal line representing detection boundary
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(minXBound, transform.position.y - 5f, 0f), new Vector3(minXBound, transform.position.y + 5f, 0f));
        Gizmos.DrawLine(new Vector3(maxXBound, transform.position.y - 5f, 0f), new Vector3(maxXBound, transform.position.y + 5f, 0f));
    }
}