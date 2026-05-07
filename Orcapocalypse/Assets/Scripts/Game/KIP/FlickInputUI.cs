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

    [Header("Ability Settings")]
    public PlayerAbility[] abilityLibrary;

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

        if (rawInput.magnitude > 0.2f)
        {
            float angle = GetNormalizedAngle(rawInput.y, rawInput.x);
            activeSector = (Sectors)CalculateSector(angle);

            trailLine.AddPoint(handle.anchoredPosition);
            lastInputTime = Time.time;
        }
        else if (Time.time - lastInputTime > 0.1f)
        {
            inputBuffer.Clear();
            trailLine.Clear();
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

    void CheckForGestures()
    {
        if (Time.time - lastInputTime > gestureTimeout)
        {
            inputBuffer.Clear();
            return;
        }

        if (abilityLibrary.Length <= 0)
        {
            Debug.LogWarning("No gesture patterns defined in the ability library.");
            return;
        }

        foreach (var ability in abilityLibrary)
        {
            if (IsPatternMatched(ability.requiredSequence))
            {
                ExecuteAttack(ability.attackType, ability.requiredSequence);
                GamepadHaptics.Instance.VibrateSuccess();
                inputBuffer.Clear();
                break;
            }
        }
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
        return Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;
    }
}
