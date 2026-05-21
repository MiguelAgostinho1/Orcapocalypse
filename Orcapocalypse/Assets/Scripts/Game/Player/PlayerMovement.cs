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
    [SerializeField] private Sprite _surgeSprite;

    private Rigidbody2D _rigidbody;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _movementInput;
    private Vector2 _smoothMovementInput;
    private Vector2 _movementInputSmoothVelocity;
    private readonly float _divePull = -2f;
    private float _spriteResetTimer;
    private bool _isSurging;
    private float _flipTimer;
    private bool _isFlipped;
    private float _initialScale;
    private Coroutine _shakeCoroutine;
    private bool IsStunned => Time.time < _stunTimer;

    public bool IsSurging => _isSurging;

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

        _isSurging = false;
        if (_spriteRenderer != null && _idleSprite != null)
        {
            _spriteRenderer.sprite = _idleSprite;
        }

        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeSprite(duration));
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

    private void Update()
    {
        // Sprite Reset Handler
        if (_isSurging && Time.time >= _spriteResetTimer)
        {
            _spriteRenderer.sprite = _idleSprite;
            _isSurging = false;
        }
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
                if (!IsStunned)
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

        HandleVisuals();
    }

    private bool IsUnderWater() => transform.position.y <= _waterLevel;

    public Vector2 GetMovementInput() => _movementInput;

    public void ResetSmoothDamp(Vector2 newVelocity)
    {
        _smoothMovementInput = newVelocity / _speed;
    }

    public void SetSurgeVisuals(Sprite attackSprite, float duration)
    {
        if (_spriteRenderer != null && attackSprite != null)
        {
            _isSurging = true;
            _spriteRenderer.sprite = attackSprite;
            _spriteResetTimer = Time.time + duration;
        }
    }

    private void OnMove(InputValue inputValue)
    {
        _movementInput = inputValue.Get<Vector2>();
    }
}