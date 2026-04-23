using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public List<PlayerAbility> abilities;
    private PlayerMovement _movement;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Dictionary<FlickInputUI.AttackType, float> _cooldowns = new Dictionary<FlickInputUI.AttackType, float>();

    void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

    public void OnGestureRecognized(FlickInputUI.AttackType type)
    {
        PlayerAbility ability = abilities.Find(a => a.attackType == type);

        if (ability != null && IsOffCooldown(type))
        {
            ability.Activate(_movement, _rb, _sr);
            _cooldowns[type] = Time.time + ability.cooldown;
        }
    }

    bool IsOffCooldown(FlickInputUI.AttackType type) =>
        !_cooldowns.ContainsKey(type) || Time.time >= _cooldowns[type];
}
