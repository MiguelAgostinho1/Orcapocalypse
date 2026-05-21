using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GestureParser))]
public class FlickInputUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInput playerInput;
    public RectTransform handle;
    public UILineDrawer trailLine;
    public PlayerCombat playerCombat;

    private GestureParser _parser;
    private InputAction _flickAction;
    private float _maxRadius;

    [Header("Visuals")]
    public Color successColor = Color.cyan;
    public Color normalColor = new Color(74f / 255f, 0f / 255f, 214f / 255f);
    public float trailThickness = 4f;
    public float successDisplayDuration = 0.3f;

    private bool _isShowingSuccess = false;
    private float _successTimer;
    private Vector2 _rawInput;

    void Awake()
    {
        _parser = GetComponent<GestureParser>();
        _flickAction = playerInput.actions["AttackInput"];
        InitializeDimensions();
    }

    void OnEnable()
    {
        _parser.OnAbilityMatched += HandleSuccessVisuals;
        _parser.OnGestureTimeout += ResetTrail;
    }

    void OnDisable()
    {
        _parser.OnAbilityMatched -= HandleSuccessVisuals;
        _parser.OnGestureTimeout -= ResetTrail;
    }

    void Update()
    {
        ReadInput();

        if (_isShowingSuccess)
        {
            if (Time.time - _successTimer > successDisplayDuration)
            {
                _isShowingSuccess = false;
                ResetTrail();
            }
            return;
        }

        // Only draw the trail if we are outside the deadzone
        if (_rawInput.magnitude > _parser.innerDeadzone)
        {
            trailLine.AddPoint(handle.anchoredPosition);
        }
        else if (_rawInput.magnitude == 0)
        {
            // Clear visual trail when stick drops to zero
            trailLine.Clear();
        }

        // Feed the raw data to the Brain
        _parser.ProcessInput(_rawInput);
    }

    private void ReadInput()
    {
        Vector2 input = _flickAction.ReadValue<Vector2>();
        _rawInput = Vector2.ClampMagnitude(input, 1f);
        handle.anchoredPosition = _rawInput * _maxRadius;
    }

    private void InitializeDimensions()
    {
        RectTransform parentRect = GetComponent<RectTransform>();
        float visualParentRadius = (parentRect.rect.width / 2f) * parentRect.localScale.x;
        float visualDotRadius = (handle.rect.width / 2f) * handle.localScale.x;
        _maxRadius = visualParentRadius - visualDotRadius;

        ResetTrail();
    }

    private void ResetTrail()
    {
        trailLine.color = normalColor;
        trailLine.thickness = trailThickness;
        trailLine.Clear();
    }

    // Triggered by the Event from GestureParser
    private void HandleSuccessVisuals(PlayerAbility ability)
    {
        _isShowingSuccess = true;
        _successTimer = Time.time;

        // Visual "Pop"
        trailLine.color = successColor;

        // Calculate Arrow Direction
        GestureParser.Sectors finalSector = ability.requiredSequence[ability.requiredSequence.Length - 1];
        Vector2 arrowDirection = GetVectorFromSector(finalSector);

        // Draw perfect sequence
        trailLine.DrawPerfectLine(ability.requiredSequence, _maxRadius, arrowDirection);

        // Execute dependencies
        GamepadHaptics.Instance.VibrateSuccess();
        GameStateManager.Instance.SetAttackExecuted(ability);

        if (playerCombat != null)
        {
            playerCombat.OnGestureRecognized(ability.attackType);
        }
    }

    private Vector2 GetVectorFromSector(GestureParser.Sectors sector)
    {
        if (sector == GestureParser.Sectors.Neutral) return Vector2.zero;
        float angle = (int)sector * 45f;
        return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
    }
}