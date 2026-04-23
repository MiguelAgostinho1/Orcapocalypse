using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public enum ControlMode { Classic, Target }

    [Header("Movement Settings")]
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _waterLevel = -0.08f;
    [SerializeField] private float _gravityInAir = 20f;
    [SerializeField] private float _flipSpeed = 5f;
    [SerializeField] private float _flipDelay = 0.5f;

    [Header("Sprite Settings")]
    [SerializeField] private Sprite _idleSprite;
    [SerializeField] private Sprite _surgeSprite;

    [Header("Testing Settings")]
    [SerializeField] private ControlMode _currentMode = ControlMode.Classic;
    [SerializeField] private TextMeshProUGUI _modeDisplayText;

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

    // Surging boolean to help with attack logic in other scripts
    public bool IsSurging => _isSurging;

    float _orcaHeight;
    private float _stunTimer;

    public void Stun(float duration)
    {
        _stunTimer = Time.time + duration;

        // Kill all physical momentum
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = Vector2.zero;
        }

        // Clear the input buffers so it doesn't "remember" old movement
        _smoothMovementInput = Vector2.zero;
        _movementInputSmoothVelocity = Vector2.zero;

        // Cancel Surge Visuals immediately if stunned mid-surge
        _isSurging = false;
        if (_spriteRenderer != null && _idleSprite != null)
        {
            _spriteRenderer.sprite = _idleSprite;
        }

        // Restart the shake if one is already running
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeSprite(duration));
    }

    private System.Collections.IEnumerator ShakeSprite(float duration)
    {
        float elapsed = 0f;
        float shakeIntensity = 15f; // Degrees of rotation
        float shakeSpeed = 20f;     // How fast it wobbles

        while (elapsed < duration)
        {
            // Sine wave for a smoother "back and forth" dizzy look
            float tilt = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;

            _spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, tilt);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset rotation back to perfectly aligned
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
        // Toggle mode with the 'M' key for quick testing
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            _currentMode = (_currentMode == ControlMode.Classic) ? ControlMode.Target : ControlMode.Classic;
            UpdateUI();
            Debug.Log("Switched to: " + _currentMode);
        }

        // Sprite Reset Handler
        if (_isSurging && Time.time >= _spriteResetTimer)
        {
            _spriteRenderer.sprite = _idleSprite;
            _isSurging = false;
        }
    }

    private void UpdateUI()
    {
        if (_modeDisplayText != null)
        {
            string modeName = (_currentMode == ControlMode.Classic) ? "Classic (Keyboard/Gamepad)" : "Target (Mouse)";

            // Rich Text to make the instruction smaller
            string instruction = "\n<size=60%>Press 'M' to Toggle</color></size>";

            _modeDisplayText.text = "Control Mode: " + modeName + instruction;
        }
    }

    private void Start()
    {
        UpdateUI(); // Set the initial text
    }

    private void HandleVisuals()
    {
        if (_rigidbody.linearVelocity.magnitude > 0.1f)
        {
            // 1. Smooth Rotation
            float angle = Mathf.Atan2(_rigidbody.linearVelocity.y, _rigidbody.linearVelocity.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.fixedDeltaTime * 10f);

            // 2. Flip Logic with Timer
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

            // 3. Smooth Roll (Scale)
            float targetYScale = _isFlipped ? -_initialScale : _initialScale;
            float smoothedY = Mathf.Lerp(transform.localScale.y, targetYScale, Time.fixedDeltaTime * _flipSpeed);
            transform.localScale = new Vector3(_initialScale, smoothedY, 1f);
        }
        else
        {
            // Handle "Self-Righting" when idle
            Quaternion targetRotation = Quaternion.Euler(0, 0, _isFlipped ? 180 : 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 2f);
        }
    }

    private void FixedUpdate()
    {
        if (Time.time < _stunTimer)
        {
            // While stunned, do nothing.
            return;
        }

        if (transform.position.y <= _waterLevel)
        {
            // --- 1. Transition ---
            if (_rigidbody.gravityScale != 0)
            {
                _smoothMovementInput = new Vector2(_rigidbody.linearVelocity.x / _speed, _divePull / _speed);
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _divePull);
                if (transform.position.y + _orcaHeight * 2 < _waterLevel) _rigidbody.gravityScale = 0;
                return;
            }

            // --- 2. Normal Swimming ---
            Vector2 finalInput = Vector2.zero;

            if (_currentMode == ControlMode.Classic)
            {
                finalInput = _movementInput;
            }
            else if (_currentMode == ControlMode.Target)
            {
                finalInput = GetMouseInput();
            }

            // Apply movement
            _smoothMovementInput = Vector2.SmoothDamp(_smoothMovementInput, finalInput, ref _movementInputSmoothVelocity, 0.1f);
            _rigidbody.linearVelocity = _smoothMovementInput * _speed;
        }
        else
        {
            // --- 3. AIR PHYSICS ---
            _rigidbody.gravityScale = _gravityInAir;

            float horizontalVel = 0f;

            if (_currentMode == ControlMode.Classic)
            {
                // WASD/Joystick input
                horizontalVel = _movementInput.x * _speed;
            }
            else if (_currentMode == ControlMode.Target)
            {
                // Mouse Position: If the mouse is to the right of the orca, move right
                Vector2 mouseDir = GetMouseInput();
                horizontalVel = mouseDir.x * _speed;
            }

            _rigidbody.linearVelocity = new Vector2(horizontalVel, _rigidbody.linearVelocity.y);
        }

        // Apply rotations to ensure the sprite is facing the right way
        HandleVisuals();
    }

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

    private Vector2 GetMouseInput()
    {
        // --- 1. Get Mouse Position in World Space ---
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = 10f;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        // --- 2. Direction from Orca to Mouse ---
        Vector2 directionToMouse = (Vector2)worldMousePos - (Vector2)transform.position;

        // --- 3. Deadzone: If the mouse is right on top of the orca, don't move ---
        // Prevents the orca from "shaking" when it reaches the cursor
        if (directionToMouse.magnitude < 0.5f)
        {
            return Vector2.zero;
        }

        return directionToMouse.normalized;
    }
}