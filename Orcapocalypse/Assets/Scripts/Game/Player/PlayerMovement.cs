using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _waterLevel = -0.08f;
    [SerializeField] private float _gravityInAir = 20f;
    [SerializeField] private float _flipSpeed = 5f;
    [SerializeField] private float _flipDelay = 0.5f;

    [Header("Sprite Settings")]
    [SerializeField] private Sprite _idleSprite;

    [Header("Level Boundaries")]
    [Tooltip("The leftmost X coordinate the Orca can travel to.")]
    [SerializeField] private float _minXBound = -82f;
    [Tooltip("The rightmost X coordinate the Orca can travel to.")]
    [SerializeField] private float _maxXBound = 82f;
    [Tooltip("The bottom ocean floor Y coordinate the Orca cannot pass.")]
    [SerializeField] private float _minYBound = -40f;

    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _movementInput;
    private Vector2 _smoothMovementInput;
    private Vector2 _movementInputSmoothVelocity;
    private readonly float _divePull = -2f;
    private float _flipTimer;
    private bool _isFlipped;
    private float _initialScale;
    private Coroutine _shakeCoroutine;
    private bool IsStunned => Time.time < _stunTimer;
    private float _abilityLockTimer;
    private bool IsAbilityLocked => Time.time < _abilityLockTimer;


    private float _orcaHeight;
    private float _stunTimer;

    public void Stun(float duration)
    {
        _stunTimer = Time.time + duration;

        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }

        _smoothMovementInput = Vector2.zero;
        _movementInputSmoothVelocity = Vector2.zero;

        if (_spriteRenderer != null && _idleSprite != null)
        {
            _spriteRenderer.sprite = _idleSprite;
        }

        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeSprite(duration));
    }

    public void LockMovementForAbility(float duration) 
    {
        _abilityLockTimer = Time.time + duration;
        _smoothMovementInput = Vector2.zero; // Clear the joystick buffer
    }

    public void ResetToIdleVisuals()
    {
        if (_spriteRenderer != null && _idleSprite != null && !IsStunned)
        {
            _spriteRenderer.sprite = _idleSprite;
        }
    }

    public Vector2 GetFacingDirection()
    {
        // Since you already perfectly track _isFlipped in your movement script,
        // we can just use that to know which way the Orca is looking!
        return _isFlipped ? Vector2.left : Vector2.right;
    }

    private IEnumerator ShakeSprite(float duration)
    {
        float elapsed = 0f;
        float shakeIntensity = 15f;
        float shakeSpeed = 20f;

        while (elapsed < duration)
        {
            float tilt = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            _spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, tilt);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _spriteRenderer.transform.localRotation = Quaternion.identity;
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody.gravityScale = 0;
        _initialScale = transform.localScale.x;
        _orcaHeight = GetComponent<PolygonCollider2D>().bounds.extents.y;
    }

    private void HandleVisuals()
    {
        if (IsStunned) return;

        if (_rigidbody.linearVelocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(_rigidbody.linearVelocity.y, _rigidbody.linearVelocity.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.fixedDeltaTime * 10f);

            bool movingLeft = _rigidbody.linearVelocity.x < -0.1f;
            if (movingLeft != _isFlipped)
            {
                _flipTimer += Time.fixedDeltaTime;
                if (_flipTimer >= _flipDelay)
                {
                    _isFlipped = movingLeft;
                    _flipTimer = 0;
                }
            }
            else { _flipTimer = 0; }

            float targetYScale = _isFlipped ? -_initialScale : _initialScale;
            float smoothedY = Mathf.Lerp(transform.localScale.y, targetYScale, Time.fixedDeltaTime * _flipSpeed);
            transform.localScale = new Vector3(_initialScale, smoothedY, 1f);
        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(0, 0, _isFlipped ? 180 : 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 2f);
        }
    }

    private void FixedUpdate()
    {
        // Handle environmental physics (Water vs Air)
        if (IsUnderWater())
        {
            if (_rigidbody.gravityScale != 0)
            {
                // Transition logic
                _smoothMovementInput = new Vector2(_rigidbody.linearVelocity.x / _speed, _divePull / _speed);
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _divePull);
                if (transform.position.y + _orcaHeight * 2 < _waterLevel) _rigidbody.gravityScale = 0;
            }
            else
            {
                // Only apply movement if NOT stunned
                if (!IsStunned && !IsAbilityLocked)
                {
                    _smoothMovementInput = Vector2.SmoothDamp(_smoothMovementInput, _movementInput, ref _movementInputSmoothVelocity, 0.1f);
                    _rigidbody.linearVelocity = _smoothMovementInput * _speed;
                }
            }
        }
        else
        {
            // Air Physics - Always apply gravity when in air
            _rigidbody.gravityScale = _gravityInAir;

            // Only allow air-steering if NOT stunned
            if (!IsStunned)
            {
                float horizontalVel = _movementInput.x * _speed;
                _rigidbody.linearVelocity = new Vector2(horizontalVel, _rigidbody.linearVelocity.y);
            }
        }

        HandleBoundaries();

        HandleVisuals();
    }

    /// <summary>
    /// Checks level bounds, clamps position, and zeroes velocity vectors when hitting boundaries.
    /// </summary>
    private void HandleBoundaries()
    {
        Vector2 velocity = _rigidbody.linearVelocity;
        Vector3 position = transform.position;
        bool boundaryHit = false;

        // X Axis: Left Boundary
        if (position.x <= _minXBound && velocity.x < 0f)
        {
            position.x = _minXBound;
            velocity.x = 0f;
            _smoothMovementInput.x = 0f; // Prevents SmoothDamp from storing backward drift
            boundaryHit = true;
        }
        // X Axis: Right Boundary
        else if (position.x >= _maxXBound && velocity.x > 0f)
        {
            position.x = _maxXBound;
            velocity.x = 0f;
            _smoothMovementInput.x = 0f;
            boundaryHit = true;
        }

        // Y Axis: Ocean Floor Boundary (Bottom)
        if (position.y <= _minYBound && velocity.y < 0f)
        {
            position.y = _minYBound;
            velocity.y = 0f;
            _smoothMovementInput.y = 0f; // Prevents SmoothDamp from sticking down
            boundaryHit = true;
        }

        // Apply modifications synchronously if a boundary was crossed
        if (boundaryHit)
        {
            transform.position = position;
            _rigidbody.linearVelocity = velocity;
        }
    }

    private bool IsUnderWater() => transform.position.y <= _waterLevel;

    public Vector2 GetMovementInput() => _movementInput;

    public void ResetSmoothDamp(Vector2 newVelocity)
    {
        _smoothMovementInput = newVelocity / _speed;
    }

    private void OnMove(InputValue inputValue)
    {
        _movementInput = inputValue.Get<Vector2>();
    }

    /// <summary>
    /// Draws visual boundary lines in the Unity Scene View when the Player is selected.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Define the vertical height for the wall gizmos (extends well above water level)
        float topGizmoY = _waterLevel + 15f;

        // --- 1. WALLS & FLOOR (Red) ---
        Gizmos.color = Color.red;

        // Left Boundary Wall
        Gizmos.DrawLine(new Vector3(_minXBound, _minYBound, 0f), new Vector3(_minXBound, topGizmoY, 0f));

        // Right Boundary Wall
        Gizmos.DrawLine(new Vector3(_maxXBound, _minYBound, 0f), new Vector3(_maxXBound, topGizmoY, 0f));

        // Ocean Floor (Spans directly between your left and right walls)
        Gizmos.DrawLine(new Vector3(_minXBound, _minYBound, 0f), new Vector3(_maxXBound, _minYBound, 0f));


        // --- 2. WATER LEVEL (Blue) ---
        Gizmos.color = Color.cyan;

        // Surface line to show where air physics take over
        Gizmos.DrawLine(new Vector3(_minXBound, _waterLevel, 0f), new Vector3(_maxXBound, _waterLevel, 0f));


        // --- 3. DIRECTIONAL HINTS (Optional) ---
        // Draws small visual ticks at the player's level pointing inward to show the safe zone
        Gizmos.color = Color.yellow;
        float currentY = transform.position.y;

        // Only draw the tracking ticks if the player is within a reasonable vertical range
        if (currentY > _minYBound && currentY < topGizmoY)
        {
            Gizmos.DrawLine(new Vector3(_minXBound, currentY, 0f), new Vector3(_minXBound + 2f, currentY, 0f));
            Gizmos.DrawLine(new Vector3(_maxXBound, currentY, 0f), new Vector3(_maxXBound - 2f, currentY, 0f));
        }
    }
}