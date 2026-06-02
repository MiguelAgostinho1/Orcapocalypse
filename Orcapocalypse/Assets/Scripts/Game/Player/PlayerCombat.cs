using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public AbilityDatabase abilityDatabase;
    private PlayerMovement _movement;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Dictionary<GestureParser.AttackType, float> _cooldowns = new Dictionary<GestureParser.AttackType, float>();
    
    // Tracks the currently active ability sequence
    private Coroutine _activeAbilityCoroutine;

    public PlayerAbility CurrentActiveAbility { get; private set; }

    void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

    public void OnGestureRecognized(GestureParser.AttackType type, string abilityName)
    {
        PlayerAbility ability = System.Array.Find(abilityDatabase.allAbilities, a => a.GetAttackType() == type && a.GetAbilityName() == abilityName);

        if (ability != null && IsOffCooldown(type))
        {
            // If the player somehow executes a move while another is running, cut the old one short
            if (_activeAbilityCoroutine != null) StopCoroutine(_activeAbilityCoroutine);

            // Start the orchestrated ability timeline
            CurrentActiveAbility = ability;
            _activeAbilityCoroutine = StartCoroutine(ExecuteAbilityRoutine(ability));
            
            _cooldowns[type] = Time.time + ability.GetDuration();
        }
    }

    private IEnumerator ExecuteAbilityRoutine(PlayerAbility ability)
    {
        // 1. Tell the movement script to stop overwriting our velocity
        _movement.LockMovementForAbility(ability.GetDuration());

        // 2. Pass control to the scriptable object to apply its custom physics and sprite
        ability.Activate(_movement, _rb, _sr);

        // 3. Wait for the exact duration of this specific move
        yield return new WaitForSeconds(ability.GetDuration());

        // 4. The move is done! Tell PlayerMovement to restore its default idle visual state
        _movement.ResetToIdleVisuals();
        CurrentActiveAbility = null;
        _activeAbilityCoroutine = null;
    }

    bool IsOffCooldown(GestureParser.AttackType type) =>
        !_cooldowns.ContainsKey(type) || Time.time >= _cooldowns[type];
}