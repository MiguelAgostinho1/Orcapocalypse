using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _waterLevel = -0.08f;
    [SerializeField] private float _gravityInAir = 20f;

    [Header("Surge Settings")]
    [SerializeField] private float _surgeForce = 15f;
    [SerializeField] private float _surgeCooldown = 2f;

    [Header("Testing Settings")]
    [SerializeField] private ControlMode _currentMode = ControlMode.Classic;
    [SerializeField] private TextMeshProUGUI _modeDisplayText;

    private Rigidbody2D _rigidbody;
    private Vector2 _movementInput;
    private Vector2 _smoothMovementInput;
    private Vector2 _movementInputSmoothVelocity;
    private readonly float _divePull = -2f;
    private float _surgeTimer;

    public enum ControlMode { Classic, Target }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0; 
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

    private void FixedUpdate()
    {
        float orcaHeight = GetComponent<Collider2D>().bounds.extents.y;

        if (transform.position.y <= _waterLevel)
        {
            // --- 1. Transition ---
            if (_rigidbody.gravityScale != 0)
            {
                _smoothMovementInput = new Vector2(_rigidbody.linearVelocity.x / _speed, _divePull / _speed);
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _divePull);
                if (transform.position.y + orcaHeight * 3 < _waterLevel) _rigidbody.gravityScale = 0;
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

            // Rotate to face velocity
            if (_rigidbody.linearVelocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(_rigidbody.linearVelocity.y, _rigidbody.linearVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        else
        {
            // --- 3. AIR PHYSICS ---
            _rigidbody.gravityScale = _gravityInAir;

            float horizontalVel = 0f;

            if (_currentMode == ControlMode.Classic)
            {
                // WASD/Joystick keys
                horizontalVel = _movementInput.x * _speed;
            }
            else if (_currentMode == ControlMode.Target)
            {
                // Mouse Position: If the mouse is to the right of the orca, move right
                Vector2 mouseDir = GetMouseInput();
                horizontalVel = mouseDir.x * _speed;
            }

            _rigidbody.linearVelocity = new Vector2(horizontalVel, _rigidbody.linearVelocity.y);

            // Rotate to face the direction of the jump arc
            if (_rigidbody.linearVelocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(_rigidbody.linearVelocity.y, _rigidbody.linearVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private void OnJump(InputValue inputValue)
    {
        // Only allow surge if cooldown is over and the orca is underwater
        if (inputValue.isPressed && Time.time >= _surgeTimer && transform.position.y <= _waterLevel)
        {
            // --- 1. Get the direction (if no input, surge forward/right) ---
            Vector2 surgeDir = _movementInput.normalized;
            if (surgeDir == Vector2.zero) surgeDir = transform.right;

            // --- 2. Apply the boost ---
            _rigidbody.linearVelocity = surgeDir * _surgeForce;

            // --- 3. Reset the internal smooth damp so it doesn't "fight" the surge ---
            _smoothMovementInput = _rigidbody.linearVelocity / _speed;

            // --- 4. Set cooldown ---
            _surgeTimer = Time.time + _surgeCooldown;
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