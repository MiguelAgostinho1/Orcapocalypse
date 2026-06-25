using System;
using System.Collections.Generic;
using UnityEngine;

public class GestureParser : MonoBehaviour
{
    public enum AttackType { None, Surge, TailWhip, Spiral }
    public enum Sectors { East = 0, NorthEast = 1, North = 2, NorthWest = 3, West = 4, SouthWest = 5, South = 6, SouthEast = 7, Neutral = 99 }

    // Events that the UI and Combat scripts will listen to
    public event Action<PlayerAbility> OnAbilityMatched;
    public event Action OnGestureTimeout;

    [Header("Leeway & Deadzones")]
    [Tooltip("The minimum joystick distance (0 to 1) required to register an input. Prevents stick drift and accidental micro-movements.")]
    [Range(0f, 0.5f)] public float innerDeadzone = 0.25f;
    [Tooltip("Adds extra degrees to the active sector's boundaries. Prevents the input from rapidly flickering back and forth if the player holds the stick exactly on the line between two sectors.")]
    [Range(0f, 20f)] public float sectorLeeway = 10f;

    [Header("Sequence Logic")]
    public AbilityDatabase abilityDatabase;
    public int bufferSize = 15;
    public float gestureTimeout = 0.5f;
    [SerializeField] private float _pendingExecutionDelay = 0.2f;

    // Tracking Data
    private List<Sectors> _inputBuffer = new List<Sectors>();
    private Sectors _lastRecordedSector = Sectors.Neutral;
    private PlayerAbility _pendingAbility = null;
    private float _pendingMatchTime;
    private float _lastInputTime;
    private float _gestureStartTime;

    public void ProcessInput(Vector2 rawInput)
    {
        HandlePendingChamber();

        // Expire old gestures
        if (_inputBuffer.Count > 0 && Time.time - _gestureStartTime > gestureTimeout)
        {
            ClearGesture(true);
            return;
        }

        Sectors activeSector = Sectors.Neutral;

        if (rawInput.magnitude > innerDeadzone)
        {
            float angle = GetNormalizedAngle(rawInput.y, rawInput.x);
            activeSector = (Sectors)CalculateSector(angle);
            _lastInputTime = Time.time;
        }
        else if (Time.time - _lastInputTime > 0.1f)
        {
            // Stick released
            if (_pendingAbility != null)
            {
                ExecuteAttack(_pendingAbility);
            }
            else
            {
                ClearGesture(false);
            }
        }

        // Record Sector Changes
        if (activeSector != _lastRecordedSector)
        {
            if (_inputBuffer.Count == 0 && activeSector != Sectors.Neutral)
            {
                _gestureStartTime = Time.time;
            }

            _lastRecordedSector = activeSector;
            _inputBuffer.Add(activeSector);

            if (_inputBuffer.Count > bufferSize)
                _inputBuffer.RemoveAt(0);

            CheckForGestures();
        }
    }

    private void HandlePendingChamber()
    {
        if (_pendingAbility == null) return;

        if (Time.time - _pendingMatchTime > _pendingExecutionDelay)
        {
            ExecuteAttack(_pendingAbility);
        }
    }

    private void CheckForGestures()
    {
        if (Time.time - _lastInputTime > gestureTimeout)
        {
            ClearGesture(false);
            return;
        }

        if (abilityDatabase.allAbilities.Length== 0) return;

        PlayerAbility matchedAbility = null;
        foreach (var ability in abilityDatabase.allAbilities)
        {
            if (IsPatternMatched(ability.GetRequiredSequence()))
            {
                matchedAbility = ability;
                break;
            }
        }

        if (matchedAbility != null)
        {
            if (IsPrefixOfLongerSequence(matchedAbility.GetRequiredSequence()))
            {
                _pendingAbility = matchedAbility;
                _pendingMatchTime = Time.time;
            }
            else
            {
                ExecuteAttack(matchedAbility);
            }
        }
    }

    private void ExecuteAttack(PlayerAbility ability)
    {
        // Broadcast the success out to the UI and Combat scripts!
        OnAbilityMatched?.Invoke(ability);

        _inputBuffer.Clear();
        _pendingAbility = null;
    }

    private void ClearGesture(bool triggerTimeoutEvent)
    {
        _inputBuffer.Clear();
        _pendingAbility = null;

        if (triggerTimeoutEvent) OnGestureTimeout?.Invoke();
    }

    private bool IsPatternMatched(Sectors[] sequence)
    {
        if (_inputBuffer.Count != sequence.Length) return false;
        for (int i = 0; i < sequence.Length; i++)
        {
            int bufferIndex = _inputBuffer.Count - sequence.Length + i;
            if (_inputBuffer[bufferIndex] != sequence[i]) return false;
        }
        return true;
    }

    private bool IsPrefixOfLongerSequence(Sectors[] shortSequence)
    {
        Sectors[] requiredSequence;
        foreach (var ability in abilityDatabase.allAbilities)
        {
            requiredSequence = ability.GetRequiredSequence();
            if (requiredSequence.Length > shortSequence.Length)
            {
                bool matchesPrefix = true;
                for (int i = 0; i < shortSequence.Length; i++)
                {
                    if (requiredSequence[i] != shortSequence[i])
                    {
                        matchesPrefix = false;
                        break;
                    }
                }
                if (matchesPrefix) return true;
            }
        }
        return false;
    }

    private float GetNormalizedAngle(float y, float x)
    {
        float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        return (angle < 0) ? angle + 360 : angle;
    }

    private int CalculateSector(float angle)
    {
        int idealSector = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;
        if (_lastRecordedSector == Sectors.Neutral) return idealSector;

        float currentSectorCenter = (int)_lastRecordedSector * 45f;
        float angleDelta = Mathf.Abs(Mathf.DeltaAngle(angle, currentSectorCenter));

        if (angleDelta < (22.5f + sectorLeeway)) return (int)_lastRecordedSector;

        return idealSector;
    }
}