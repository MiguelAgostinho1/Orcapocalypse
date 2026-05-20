using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class FlickInputUI : MonoBehaviour
{
    public enum AttackType { None, Surge, TailWhip, Spiral }
    public enum Sectors { East = 0, NorthEast = 1, North = 2, NorthWest = 3, West = 4, SouthWest = 5, South = 6, SouthEast = 7, Neutral = 99 }

    [Header("Input")]
    public PlayerInput playerInput;

    [Header("UI References")]
    public RectTransform handle; 
    public UILineDrawer trailLine;

    [Header("Gesture Logic")]
    public List<Sectors> inputBuffer = new List<Sectors>();
    public int bufferSize = 15;
    private Sectors lastRecordedSector = Sectors.Neutral;
    public float gestureTimeout = 0.5f; // Time allowed between inputs

    [Header("Leeway & Deadzones")]
    [Range(0f, 0.5f)]
    public float innerDeadzone = 0.25f; // Stick must move this far from center to count
    [Range(0f, 20f)]
    public float sectorLeeway = 10f;

    [Header("Ability Settings")]
    public PlayerAbility[] abilityLibrary;

    [Header("Sequence Chamber Settings")]
    [SerializeField] private float _pendingExecutionDelay = 0.2f; // Window to check for extensions
    private PlayerAbility _pendingAbility = null;
    private float _pendingMatchTime;

    [Header("Gestures Success Visuals")]
    public Color successColor = Color.cyan;
    public Color normalColor = new Color(74f / 255f, 0f / 255f, 214f / 255f);
    public float trailThickness = 4f;
    private bool isShowingSuccess = false;
    private float successTimer;
    public float successDisplayDuration = 0.3f;

    [Header("Debug Data")]
    public Vector2 rawInput;
    public float currentAngle;
    public Sectors currentSector = Sectors.Neutral;

    private InputAction flickAction;
    private float maxRadius;
    private float lastInputTime;
    private float gestureStartTime;

    public PlayerCombat playerCombat;


    void Start()
    {
        flickAction = playerInput.actions["AttackInput"];
        
        InitializeDimensions();
        ResetTrailVisuals();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputMovement();

        if (isShowingSuccess)
        {
            UpdateSuccessState();
            return;
        }

        UpdateGestureTracking();
        HandlePendingChamber();

        // if (activeSector != Sectors.Neutral) Debug.Log($"Angle: {currentAngle:F1} | Sector: {activeSector}");
    }

    private void InitializeDimensions()
    {
        RectTransform parentRect = GetComponent<RectTransform>();
        float visualParentRadius = (parentRect.rect.width / 2f) * parentRect.localScale.x;
        float visualDotRadius = (handle.rect.width / 2f) * handle.localScale.x;
        maxRadius = visualParentRadius - visualDotRadius;
    }

    private void HandleInputMovement()
    {
        Vector2 input = flickAction.ReadValue<Vector2>();
        rawInput = Vector2.ClampMagnitude(input, 1f);
        handle.anchoredPosition = rawInput * maxRadius;
    }

    private void UpdateSuccessState()
    {
        if (Time.time - successTimer > successDisplayDuration)
        {
            inputBuffer.Clear();
            isShowingSuccess = false;
            ResetTrailVisuals();
            trailLine.Clear();
        }
    }

    private void UpdateGestureTracking()
    {
        if (inputBuffer.Count > 0 && Time.time - gestureStartTime > gestureTimeout)
        {
            Debug.Log($"Gesture Expired: {Time.time - gestureStartTime:F2}s");
            inputBuffer.Clear();
            lastRecordedSector = Sectors.Neutral;
            trailLine.Clear();
            return;
        }

        Sectors activeSector = Sectors.Neutral;

        if (rawInput.magnitude > innerDeadzone)
        {
            float angle = GetNormalizedAngle(rawInput.y, rawInput.x);
            activeSector = (Sectors)CalculateSector(angle);

            trailLine.AddPoint(handle.anchoredPosition);
            lastInputTime = Time.time;
        }
        else if (Time.time - lastInputTime > 0.1f)
        {
            // If they let go of the stick, fire whatever is waiting in the chamber immediately
            if (_pendingAbility != null)
            {
                ExecutePendingAttack(_pendingAbility);
            }
            else
            {
                inputBuffer.Clear();
                trailLine.Clear();
            }
        }

        currentSector = activeSector;

        // Record changes in sector
        if (activeSector != lastRecordedSector)
        {
            if (inputBuffer.Count == 0 && activeSector != Sectors.Neutral)
            {
                gestureStartTime = Time.time;
            }

            lastRecordedSector = activeSector;
            inputBuffer.Add(activeSector);

            if (inputBuffer.Count > bufferSize)
                inputBuffer.RemoveAt(0);

            CheckForGestures();
        }
    }

    private void ResetTrailVisuals()
    {
        trailLine.color = normalColor;
        trailLine.thickness = trailThickness;
    }

    private void HandlePendingChamber()
    {
        if (_pendingAbility == null) return;

        // If the player waits too long without completing a longer gesture, default to the pending one
        if (Time.time - _pendingMatchTime > _pendingExecutionDelay)
        {
            Debug.Log($"Chamber timed out. Executing default move: {_pendingAbility.attackType}");
            ExecutePendingAttack(_pendingAbility);
        }
    }

    void CheckForGestures()
    {
        if (Time.time - lastInputTime > gestureTimeout)
        {
            inputBuffer.Clear();
            _pendingAbility = null;
            return;
        }

        if (abilityLibrary.Length <= 0) return;

        PlayerAbility matchedAbility = null;

        // 1. Find if the current buffer matches ANY ability
        foreach (var ability in abilityLibrary)
        {
            if (IsPatternMatched(ability.requiredSequence))
            {
                matchedAbility = ability;
                break;
            }
        }

        // 2. Evaluate if we should execute or hold it
        if (matchedAbility != null)
        {
            // If this sequence is the baseline for a LONGER move, hold it in the chamber
            if (IsPrefixOfLongerSequence(matchedAbility.requiredSequence))
            {
                _pendingAbility = matchedAbility;
                _pendingMatchTime = Time.time;
                Debug.Log($"Holding {matchedAbility.attackType} in chamber... checking for extensions.");
            }
            else
            {
                // This is already the longest possible move or has no extensions, execute immediately!
                ExecutePendingAttack(matchedAbility);
            }
        }
    }

    // Checks if the completed short sequence matches the beginning of a longer sequence in the library
    private bool IsPrefixOfLongerSequence(Sectors[] shortSequence)
    {
        foreach (var ability in abilityLibrary)
        {
            if (ability.requiredSequence.Length > shortSequence.Length)
            {
                bool matchesPrefix = true;
                for (int i = 0; i < shortSequence.Length; i++)
                {
                    if (ability.requiredSequence[i] != shortSequence[i])
                    {
                        matchesPrefix = false;
                        break;
                    }
                }
                if (matchesPrefix) return true; // Found a longer move that starts with this sequence
            }
        }
        return false;
    }

    // Consolidates the execution and clears all tracking data cleanly
    private void ExecutePendingAttack(PlayerAbility ability)
    {
        ExecuteAttack(ability.attackType, ability.requiredSequence);
        GamepadHaptics.Instance.VibrateSuccess();

        // Reset inputs entirely
        inputBuffer.Clear();
        lastRecordedSector = Sectors.Neutral;
        _pendingAbility = null;
    }

    bool IsPatternMatched(Sectors[] sequence)
    {
        if (inputBuffer.Count != sequence.Length) return false;

        for (int i = 0; i < sequence.Length; i++)
        {
            int bufferIndex = inputBuffer.Count - sequence.Length + i;
            if (inputBuffer[bufferIndex] != sequence[i]) return false;
        }

        return true;
    }

    void ExecuteAttack(AttackType type, Sectors[] sequence)
    {
        Debug.Log($"Exectued {type}");

        isShowingSuccess = true;
        successTimer = Time.time;

        // Visual "Pop"
        trailLine.color = successColor;

        // Calculate the Arrow Direction for Visual Feedback
        Sectors finalSector = sequence[sequence.Length - 1];
        Vector2 arrowDirection = GetVectorFromSector(finalSector);

        // Draw the "Ideal" version of the move
        trailLine.DrawPerfectLine(sequence, maxRadius, arrowDirection);

        if (playerCombat != null)
        {
            playerCombat.OnGestureRecognized(type);
        }
    }

    // Helper to turn a Sector enum back into a Directional Vector
    private Vector2 GetVectorFromSector(Sectors sector)
    {
        if (sector == Sectors.Neutral) return Vector2.zero;
        float angle = (int)sector * 45f;
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    float GetNormalizedAngle(float y, float x)
    {
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        return (angle < 0) ? angle + 360 : angle;
    }

    int CalculateSector(float angle)
    {
        // 1. Calculate the sector
        int idealSector = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;

        // 2. If the right stick was neutral (no sector selected), just return the ideal one
        if (lastRecordedSector == Sectors.Neutral) return idealSector;

        // 3. Calculate the center angle of the sector the player is currently "holding"
        float currentSectorCenter = (int)lastRecordedSector * 45f;

        // 4. Find the difference between the right stick position and the center of the current sector
        float angleDelta = Mathf.Abs(Mathf.DeltaAngle(angle, currentSectorCenter));

        // 5. If the right stick is within (22.5 + leeway), stay in the current sector.
        if (angleDelta < (22.5f + sectorLeeway))
        {
            return (int)lastRecordedSector;
        }

        // 6. The movement was intentional enough to switch
        return idealSector;
    }
}
