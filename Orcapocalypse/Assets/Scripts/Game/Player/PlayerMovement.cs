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

    private Rigidbody2D _rigidbody;
    private Vector2 _movementInput;
    private Vector2 _smoothMovementInput;
    private Vector2 _movementInputSmoothVelocity;
    private float _divePull = -2f;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0; 
    }

    private void FixedUpdate()
    {
        float orcaHeight = GetComponent<Collider2D>().bounds.extents.y;
        float orcaTopY = transform.position.y + orcaHeight;

        if (transform.position.y <= _waterLevel)
        {
            // --- BREACH BOOST ---
            if (_movementInput.y > 0.1f && orcaTopY + orcaHeight * 2 >= _waterLevel)
            {
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _breachJumpForce);
                _rigidbody.gravityScale = _gravityInAir;
                return;
            }

            // --- 1. TRANSITION FROM AIR TO WATER ---
            // We use an 'if' here to handle the plunge
            if (_rigidbody.gravityScale != 0)
            {
                // Kill the current air-momentum so SmoothDamp starts from a clean slate
                _smoothMovementInput = new Vector2(_rigidbody.linearVelocity.x / _speed, _divePull / _speed);

                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x, _divePull);

                if (transform.position.y + orcaHeight * 3 < _waterLevel)
                {
                    _rigidbody.gravityScale = 0;
                }

                // We 'return' here to skip Normal Swimming for this frame
                // This prevents the SmoothDamp "hiccup"
                return;
            }

            // --- 2. NORMAL SWIMMING ---
            _smoothMovementInput = Vector2.SmoothDamp(_smoothMovementInput, _movementInput, ref _movementInputSmoothVelocity, 0.1f);
            _rigidbody.linearVelocity = _smoothMovementInput * _speed;
        }
        else
        {
            // --- 3. AIR PHYSICS ---
            _rigidbody.gravityScale = _gravityInAir;
            float horizontalVel = _movementInput.x * _speed;
            _rigidbody.linearVelocity = new Vector2(horizontalVel, _rigidbody.linearVelocity.y);
        }
    }

    private void OnMove(InputValue inputValue)
    {
        _movementInput = inputValue.Get<Vector2>();
    }
}