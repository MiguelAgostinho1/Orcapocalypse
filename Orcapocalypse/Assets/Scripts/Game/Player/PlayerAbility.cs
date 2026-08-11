using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Player/PlayerAbility")]
public class PlayerAbility : ScriptableObject
{
    [Header("Ability Settings")]
    [SerializeField]
    private string _abilityName;
    [SerializeField]
    private GestureParser.AttackType _attackType;
    [SerializeField]
    private GestureParser.Sectors[] _requiredSequence;
    [SerializeField]
    private int _damage = 10;
    [SerializeField]
    private float _duration = 0.5f;
    [SerializeField]
    private Sprite _abilitySprite;

    [Header("Physics Payload")]
    [SerializeField]
    private float _forceMagnitude = 15f;
    
    public enum PhysicsBehavior { DynamicDash, AbsoluteDirection, KillMomentum }
    [SerializeField]
    private PhysicsBehavior _physicsBehavior;

    [SerializeField]
    private Vector2 _absoluteDirection = Vector2.zero; 

    public string GetAbilityName() => _abilityName;
    public int GetDamage() => _damage;

    public GestureParser.AttackType GetAttackType() => _attackType;

    public GestureParser.Sectors[] GetRequiredSequence() => _requiredSequence;

    public float GetDuration() => _duration;

    public PhysicsBehavior GetPhysicsBehavior() => _physicsBehavior;
    
    public void Activate(PlayerMovement movement, Rigidbody2D rb, SpriteRenderer sr)
    {
        if (_abilitySprite != null) sr.sprite = _abilitySprite; 

        // Pass the movement reference directly to evaluate layout rules dynamically
        ApplyMovementForce(rb, movement);
    }

    private void ApplyMovementForce(Rigidbody2D rb, PlayerMovement movement)
    {
        Debug.Log($"Activating {_abilityName} with physics behavior: {_physicsBehavior}");
        switch (_physicsBehavior)
        {
            case PhysicsBehavior.DynamicDash:
                Debug.Log("Applying Dynamic Dash Force");
                // 1. Grab where the player is currently steering with the stick
                Vector2 dashDir = movement.GetMovementInput();
                
                // 2. If the stick is neutral, fallback to whichever way the Orca is facing
                if (dashDir.sqrMagnitude < 0.01f)
                {
                    dashDir = movement.GetFacingDirection();
                }
                
                // 3. Launch! (This handles Up, Down, Left, Right, and diagonals)
                rb.linearVelocity = Vector2.zero; 
                rb.AddForce(dashDir.normalized * _forceMagnitude, ForceMode2D.Impulse);
                break;

            case PhysicsBehavior.AbsoluteDirection:
                Debug.Log("Applying Absolute Direction Force");
                // Always goes in a strict inspector-defined direction (e.g. Deep Dive)
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(_absoluteDirection.normalized * _forceMagnitude, ForceMode2D.Impulse);
                break;

            case PhysicsBehavior.KillMomentum:
                Debug.Log("Killing Momentum");
                // STRICTLY stop the character. No forces, no movement.
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }
}