using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TutorialBoat : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    [Tooltip("Ocean Depth Configuration")]
    [SerializeField] private OceanConfig _oceanConfig;
    [Tooltip("How strongly the water pushes the boat up.")]
    [SerializeField] private float _floatForce = 15f;
    [Tooltip("Pulls the center of mass down to make the boat self-right. Negative values push it lower.")]
    [SerializeField] private float _centerOfMassOffsetY = -1.5f;
    [Tooltip("How aggressively the boat snaps to perfectly horizontal while submerged.")]
    [SerializeField] private float _horizontalSnapStrength = 0.5f;

    [Header("Physics Drag (Friction)")]
    [Tooltip("Drag applied when flying in the air.")]
    [SerializeField] private float _airDrag = 0.5f;
    [SerializeField] private float _airAngularDrag = 0.5f;

    [Tooltip("Drag applied when submerged to simulate water resistance.")]
    [SerializeField] private float _waterDrag = 3f;
    [SerializeField] private float _waterAngularDrag = 4f;

    [Header("Level Boundary Elasticity")]
    [Tooltip("How much velocity is kept when bouncing off a wall. 1.0 means perfect elastic bounce!")]
    [Range(0f, 1.5f)]
    [SerializeField] private float _wallBounciness = 0.8f;

    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        _rb.centerOfMass = new Vector2(0, _centerOfMassOffsetY);
    }

    private void FixedUpdate()
    {
        HandleBuoyancy();
        HandleWallBoundaries();
    }

    private void HandleBuoyancy()
    {
        // Calculate how deep the boat's center is below the water surface
        float depth = _oceanConfig.waterLevel - transform.position.y;

        if (depth > 0)
        {
            // UNDERWATER STATE
            _rb.linearDamping = _waterDrag;
            _rb.angularDamping = _waterAngularDrag;

            // Apply an upward force that gets stronger the deeper the boat sinks
            // We multiply by Physics2D.gravity.magnitude to counteract gravity naturally
            float buoyancyMultiplier = depth * _floatForce;
            Vector2 uplift = Vector2.up * (buoyancyMultiplier * Physics2D.gravity.magnitude);
            _rb.AddForce(uplift);

            // 2. The Horizontal Snap 
            // If the local Y axis is positive, we are mostly right-side up. Target is 0.
            // If it is negative, we are mostly upside down. Target is 180.
            float targetAngle = (transform.up.y >= 0f) ? 0f : 180f;

            // Get the shortest rotational distance to our target (-180 to 180 degrees)
            float angleDifference = Mathf.DeltaAngle(_rb.rotation, targetAngle);

            // Apply a corrective twist. Because _waterAngularDrag is high (4f), 
            // this torque won't cause the boat to spin out of control; it acts like a smooth spring.
            _rb.AddTorque(angleDifference * _horizontalSnapStrength);
        }
        else
        {
            // AIRBORNE STATE
            _rb.linearDamping = _airDrag;
            _rb.angularDamping = _airAngularDrag;
        }
    }

    /// <summary>
    /// Keeps the boat inside the level and makes it satisfyingly bounce back if launched out of bounds.
    /// </summary>
    private void HandleWallBoundaries()
    {
        Vector3 pos = transform.position;
        Vector2 vel = _rb.linearVelocity;
        bool didBounce = false;

        // Left wall hit
        if (pos.x <= _oceanConfig.minXBound && vel.x < 0f)
        {
            pos.x = _oceanConfig.minXBound;
            vel.x = -vel.x * _wallBounciness; // Reverse horizontal speed

            // Add a little extra chaotic spin on impact for comedic effect
            _rb.AddTorque(-vel.x * 2f, ForceMode2D.Impulse);
            didBounce = true;
        }
        // Right wall hit
        else if (pos.x >= _oceanConfig.maxXBound && vel.x > 0f)
        {
            pos.x = _oceanConfig.maxXBound;
            vel.x = -vel.x * _wallBounciness; // Reverse horizontal speed

            _rb.AddTorque(-vel.x * 2f, ForceMode2D.Impulse);
            didBounce = true;
        }

        if (didBounce)
        {
            transform.position = pos;
            _rb.linearVelocity = vel;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(_oceanConfig.minXBound, _oceanConfig.waterLevel, 0), new Vector3(_oceanConfig.maxXBound, _oceanConfig.waterLevel, 0));

        // Draw visual boundary walls for the boat
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(_oceanConfig.minXBound, _oceanConfig.waterLevel - 10f, 0), new Vector3(_oceanConfig.minXBound, _oceanConfig.waterLevel + 15f, 0));
        Gizmos.DrawLine(new Vector3(_oceanConfig.maxXBound, _oceanConfig.waterLevel - 10f, 0), new Vector3(_oceanConfig.maxXBound, _oceanConfig.waterLevel + 15f, 0));

        if (Application.isPlaying && _rb != null)
        {
            Gizmos.color = Color.red;
            Vector3 centerOfMassWorld = transform.TransformPoint(_rb.centerOfMass);
            Gizmos.DrawWireSphere(centerOfMassWorld, 0.2f);
        }
    }
}