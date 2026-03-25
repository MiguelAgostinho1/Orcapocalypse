using UnityEngine;

public class YachtMovement : MonoBehaviour
{
    public enum MoveDirection { Left = -1, Right = 1 }

    [Header("Floating Settings")]
    [SerializeField] private float _bobAmplitude = 0.05f;
    [SerializeField] private float _bobFrequency = 1.5f;

    [Header("Patrol Settings")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _screenPadding = 1f;
    [SerializeField] private MoveDirection _currentDirection = MoveDirection.Left;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private float _startYSettle;
    private float _timer;
    private float _minX, _maxX;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _startYSettle = transform.position.y;

        // Calculate Screen Bounds
        float cameraDistance = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        _minX = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, cameraDistance)).x + _screenPadding;
        _maxX = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, cameraDistance)).x - _screenPadding;

        UpdateFacing();
    }

    void FixedUpdate()
    {
        _timer += Time.fixedDeltaTime;
        HandleBoundaries();

        // Bobbing logic
        float newYOffset = Mathf.Sin(_timer * _bobFrequency) * _bobAmplitude;

        // Movement logic: Cast Enum to int for the math
        float moveStep = (int)_currentDirection * _moveSpeed * Time.fixedDeltaTime;
        float nextX = _rb.position.x + moveStep;

        _rb.MovePosition(new Vector2(nextX, _startYSettle + newYOffset));
    }

    private void HandleBoundaries()
    {
        if (transform.position.x >= _maxX && _currentDirection == MoveDirection.Right)
        {
            _currentDirection = MoveDirection.Left;
            UpdateFacing();
        }
        else if (transform.position.x <= _minX && _currentDirection == MoveDirection.Left)
        {
            _currentDirection = MoveDirection.Right;
            UpdateFacing();
        }
    }

    private void UpdateFacing()
    {
        // Since boat faces Left by default:
        // Flip is true if moving Right, false if moving Left
        _spriteRenderer.flipX = (_currentDirection == MoveDirection.Right);
    }
}
