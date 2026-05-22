using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public AbilityDatabase abilityDatabase;
    private PlayerMovement _movement;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Dictionary<GestureParser.AttackType, float> _cooldowns = new Dictionary<GestureParser.AttackType, float>();

    void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

    public void OnGestureRecognized(GestureParser.AttackType type)
    {
        PlayerAbility ability = System.Array.Find(abilityDatabase.allAbilities, a => a.attackType == type);

        if (ability != null && IsOffCooldown(type))
        {
            ability.Activate(_movement, _rb, _sr);
            _cooldowns[type] = Time.time + ability.cooldown;
        }
    }

    bool IsOffCooldown(GestureParser.AttackType type) =>
        !_cooldowns.ContainsKey(type) || Time.time >= _cooldowns[type];
}
