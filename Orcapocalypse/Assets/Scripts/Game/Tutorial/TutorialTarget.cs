using System;
using UnityEngine;

public class TutorialTarget : MonoBehaviour
{
    public enum TargetType { Ball, Boat }

    [Header("Target Type")]
    public TargetType myType;

    public static event Action<TargetType> OnTargetHit;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
        {
            if (other.collider.TryGetComponent(out PlayerCombat playerCombat))
            {
                if (playerCombat.CurrentActiveAbility != null)
                {
                    // Broadcast the event
                    OnTargetHit?.Invoke(myType);
                }
            }
        }
    }
}