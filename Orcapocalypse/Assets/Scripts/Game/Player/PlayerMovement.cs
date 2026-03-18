using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _waterLevel = -0.08f;
    [SerializeField] private float _gravityInAir = 20f;
    [SerializeField] private float _breachJumpForce = 1f;

    [Header("Surge Settings")]
    [SerializeField] private float _surgeForce = 15f;
    [SerializeField] private float _surgeCooldown = 2f;

    private Rigidbody2D _rigidbody;
    private Vector2 _movementInput;
    private Vector2 _smoothMovementInput;
    private Vector2 _movementInputSmoothVelocity;
    private readonly float _divePull = -2f;
    private float _surgeTimer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0; 
    }

    private void FixedUpdate()
    {
        float orcaHeight = GetComponent<Collider2D>().bounds.extents.y;

        if (transform.position.y <= _waterLevel)
        {
            // --- 1. Transition from Water to Air ---
            // Handle the plunge
            if (_rigidbody.gravityScale != 0)
            {
                // Kill the current air-momentum so SmoothDamp starts from a clean slate
                _smoothMovementInput = new Vector2(_rigidbody.linearVelocity.x / _speed, _divePull / _speed);

                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _divePull);

                if (transform.position.y + orcaHeight * 3 < _waterLevel)
                {
                    _rigidbody.gravityScale = 0;
                }

                // Return here to skip Normal Swimming for this frame. Prevents the SmoothDamp "hiccup"
                return;
            }

            // --- 2. Normal Swimming ---
            _smoothMovementInput = Vector2.SmoothDamp(_smoothMovementInput, _movementInput, ref _movementInputSmoothVelocity, 0.1f);
            _rigidbody.linearVelocity = _smoothMovementInput * _speed;
        }
        else
        {
            // --- 3. Air Physics ---
            _rigidbody.gravityScale = _gravityInAir;
            float horizontalVel = _movementInput.x * _speed;
            _rigidbody.linearVelocity = new Vector2(horizontalVel, _rigidbody.linearVelocity.y);
        }
    }

    private void OnJump(InputValue inputValue)
    {
        // Only allow surge if cooldown is over and we are underwater
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
}