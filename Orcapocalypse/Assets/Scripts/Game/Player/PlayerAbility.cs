using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Player/PlayerAbility")]
public class PlayerAbility : ScriptableObject
{
    [Header("Ability Settings")]
    [SerializeField] private string abilityName;
    [SerializeField] private GestureParser.AttackType attackType;
    [SerializeField] private GestureParser.Sectors[] requiredSequence;
    [SerializeField] private int damage = 10;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private Sprite abilitySprite;

    [Header("Physics Payload")]
    [SerializeField] private float forceMagnitude = 15f;
    
    public enum PhysicsBehavior { DynamicDash, AbsoluteDirection, KillMomentum }
    [SerializeField] private PhysicsBehavior physicsBehavior;

    [SerializeField] private Vector2 absoluteDirection = Vector2.zero; 

    public string GetAbilityName() => abilityName;
    public int GetDamage() => damage;

    public GestureParser.AttackType GetAttackType() => attackType;

    public GestureParser.Sectors[] GetRequiredSequence() => requiredSequence;

    public float GetDuration() => duration;

    public PhysicsBehavior GetPhysicsBehavior() => physicsBehavior;
    
    public void Activate(PlayerMovement movement, Rigidbody2D rb, SpriteRenderer sr)
    {
        if (abilitySprite != null) sr.sprite = abilitySprite; 

        // Pass the movement reference directly to evaluate layout rules dynamically
        ApplyMovementForce(rb, movement);
    }

    private void ApplyMovementForce(Rigidbody2D rb, PlayerMovement movement)
    {
        Debug.Log($"Activating {abilityName} with physics behavior: {physicsBehavior}");
        switch (physicsBehavior)
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
                rb.AddForce(dashDir.normalized * forceMagnitude, ForceMode2D.Impulse);
                break;

            case PhysicsBehavior.AbsoluteDirection:
                Debug.Log("Applying Absolute Direction Force");
                // Always goes in a strict inspector-defined direction (e.g. Deep Dive)
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(absoluteDirection.normalized * forceMagnitude, ForceMode2D.Impulse);
                break;

            case PhysicsBehavior.KillMomentum:
                Debug.Log("Killing Momentum");
                // STRICTLY stop the character. No forces, no movement.
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }
}