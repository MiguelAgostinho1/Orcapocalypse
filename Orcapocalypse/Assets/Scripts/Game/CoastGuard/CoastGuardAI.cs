using UnityEngine;

public class CoastGuardAI : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Hunt,
        Search
    }

    public enum MoveDirection { Left = -1, Right = 1 }

    [Header("State Tracking")]
    public State currentState = State.Patrol;

    [Header("Search Settings")]
    public float searchDuration = 5f;
    private float _searchTimer = 0f;

    [Header("References")]
    public Transform orca; // Reference to the player's position

    [Header("Floating Settings")]
    [SerializeField] private float _bobAmplitude = 0.05f;
    [SerializeField] private float _bobFrequency = 1.5f;

    [Header("Movement & Patrol Settings")]
    [SerializeField] private MoveDirection _currentDirection = MoveDirection.Right;
    public float patrolSpeed = 3f;
    public float huntSpeed = 6f;
    public float normalPatrolDistance = 20f;
    public float searchPatrolDistance = 5f;

    [Header("Detection Settings")]
    public float detectionRadius = 10f; // How close the boat needs to be to spot the Orca
    public float loseInterestRadius = 15f; // Distance before the boat gives up the hunt entirely
    public float stealthYLevel = -4f; // The Y-axis value the Orca must dive below to hide

    private float _startYSettle;
    private float _timer;
    private float _startX; // The boat's original starting X position
    private float _lastKnownX; // Where the boat lost sight of the Orca
    private float _currentWaypointX; // The specific point it is currently driving towards

    void Start()
    {
        _startYSettle = transform.position.y;
        _startX = transform.position.x;

        UpdateFacing();
    }

    void Update()
    {
        float previousX = transform.position.x;

        ProcessStateLogic();
        UpdateMovementDirection(previousX);
        ApplyBobbingEffect();
    }

    // --- State Logic ---

    private void PatrolUpdate()
    {
        PatrolArea(_startX, normalPatrolDistance, patrolSpeed);

        // Transition: Patrol -> Hunt
        if (IsOrcaFound())
        {
            ChangeState(State.Hunt);
        }
    }

    private void HuntUpdate()
    {
        // Aggressively follow the Orca's X position
        Vector2 targetPosition = new Vector2(orca.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, huntSpeed * Time.deltaTime);

        // Transition: Hunt -> Search
        if (HasOrcaDisappeared())
        {
            ChangeState(State.Search);
        }
    }

    private void SearchUpdate()
    {
        // Pace back and forth in a tight area where the Orca was last seen
        PatrolArea(_lastKnownX, searchPatrolDistance, patrolSpeed);

        // Transition 1: Search -> Hunt
        if (IsOrcaFound())
        {
            ChangeState(State.Hunt);
            return; // Exit early so we don't trigger the timer transition
        }

        // Transition 2: Search -> Patrol
        _searchTimer += Time.deltaTime;
        if (_searchTimer >= searchDuration)
        {
            ChangeState(State.Patrol); // Will automatically drive back to startX
        }
    }

    // --- State Management ---

    private void ChangeState(State newState)
    {
        currentState = newState;

        // Handle entry/reset logic for specific states
        if (newState == State.Search)
        {
            _searchTimer = 0f; // Reset the timer every time we enter the Search state
            _lastKnownX = orca.position.x; // Lock in the X position where the Orca escaped
            _currentWaypointX = 0f; // Force the patrol logic to pick a new waypoint
        }
        else if (newState == State.Patrol)
        {
            _currentWaypointX = 0f; // Reset the waypoint when returning to patrol
        }
    }

    // --- Patrol Movement Logic ---
    private void UpdateFacing()
    {
        // Flip is true if moving Right, false if moving Left
        float xRotation = (_currentDirection == MoveDirection.Right) ? 1f : -1f;
        transform.localScale = new Vector3(xRotation, 1f, 1f);
    }

    private void PatrolArea(float centerPosition, float patrolRadius, float speed)
    {
        float leftBound = centerPosition - patrolRadius;
        float rightBound = centerPosition + patrolRadius;

        // If there's no valid waypoint yet (e.g., just changed states), pick the closest one
        if (_currentWaypointX != leftBound && _currentWaypointX != rightBound)
        {
            _currentWaypointX = (transform.position.x < centerPosition) ? rightBound : leftBound;
        }

        // Move horizontally towards the waypoint (keeping current Y position)
        Vector2 targetPosition = new Vector2(_currentWaypointX, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // If it has reached the waypoint, flip the target to the other side
        if (Mathf.Abs(transform.position.x - _currentWaypointX) < 0.1f)
        {
            _currentWaypointX = (_currentWaypointX == leftBound) ? rightBound : leftBound;
        }
    }

    // --- Update Helpers ---
    private void ProcessStateLogic()
    {
        switch (currentState)
        {
            case State.Patrol:
                PatrolUpdate();
                break;
            case State.Hunt:
                HuntUpdate();
                break;
            case State.Search:
                SearchUpdate();
                break;
        }
    }

    private void UpdateMovementDirection(float previousX)
    {
        // Compare the new position to the old position to determine direction
        if (transform.position.x > previousX)
        {
            _currentDirection = MoveDirection.Right;
        }
        else if (transform.position.x < previousX)
        {
            _currentDirection = MoveDirection.Left;
        }

        // Apply the visual flip 
        UpdateFacing();
    }

    private void ApplyBobbingEffect()
    {
        _timer += Time.deltaTime;
        float newYOffset = Mathf.Sin(_timer * _bobFrequency) * _bobAmplitude;

        // Combine the X from the AI movement with the Y from the bobbing
        transform.position = new Vector2(transform.position.x, _startYSettle + newYOffset);
    }

    // --- Condition Checks (Sensors) ---
    private bool IsOrcaFound()
    {
        // 1. Check if the Coast Guard is close enough
        float distanceToOrca = Vector2.Distance(transform.position, orca.position);
        bool inRange = distanceToOrca <= detectionRadius;

        // 2. Check if the Orca is above the stealth depth
        bool isExposed = orca.position.y > stealthYLevel;

        // The Orca is only found if it is BOTH in range AND exposed near the surface
        return inRange && isExposed;
    }

    private bool HasOrcaDisappeared()
    {
        float distanceToOrca = Vector2.Distance(transform.position, orca.position);

        // 1. Check if the orca has dived deep enough to hide from the Coast Guard
        bool isHidden = orca.position.y <= stealthYLevel;

        // 2. Check if the orca swam far enough away that the Coast Guard gives up the hunt
        bool outOfRange = distanceToOrca > loseInterestRadius;

        // The Orca disappears if it dives deep enough OR if it outruns the boat
        return isHidden || outOfRange;
    }

    // --- Debugging Gizmos ---
    private void OnDrawGizmos()
    {
        // Draw the Detection Radius (Red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw the Lose Interest Radius (Yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseInterestRadius);

        // Draw the Stealth Y-Level Line (Blue)
        // This draws a horizontal line across your scene view at the stealth depth
        Gizmos.color = Color.blue;
        Vector3 leftPoint = new Vector3(-100f, stealthYLevel, 0f);
        Vector3 rightPoint = new Vector3(100f, stealthYLevel, 0f);
        Gizmos.DrawLine(leftPoint, rightPoint);
    }
}