using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Player/Ability")]
public class PlayerAbility : ScriptableObject
{
    [Header("Ability Signature")]
    public string abilityName;
    public FlickInputUI.AttackType attackType;
    public FlickInputUI.Sectors[] requiredSequence;

    [Header("Physics")]
    public float damage = 15f;
    public float cooldown = 1f;

    [Header("Visuals")]
    public Sprite surgeSprite;
    public float spriteDuration = 0.5f;

    public virtual void Activate(PlayerMovement movement, Rigidbody2D rb, SpriteRenderer sr)
    {
        // 1. Water Check (so the player doesn't surge through the air)
        if (movement.transform.position.y > -0.08f) return;

        // 2. Default logic for a "Surge" style move
        Vector2 dir = movement.GetMovementInput();
        if (dir == Vector2.zero) dir = movement.transform.right;

        rb.linearVelocity = dir * damage;
        movement.ResetSmoothDamp(rb.linearVelocity);

        // 3. Trigger the Visuals
        movement.SetSurgeVisuals(surgeSprite, spriteDuration);
    }
}
