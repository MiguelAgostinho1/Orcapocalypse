using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadHaptics : MonoBehaviour
{
    public static GamepadHaptics Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void VibrateDamage()
    {
        // Strong, short rumble for damage feedback
        StartCoroutine(Rumble(1f, 1f, 0.5f));
    }

    public void VibrateSuccess()
    {
        // Gentle, short rumble for successful actions
        StartCoroutine(Rumble(0f, 0.5f, 0.3f));
    }

    private IEnumerator Rumble(float lowFreq, float highFreq, float duration)
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) yield break;

        gamepad.SetMotorSpeeds(lowFreq, highFreq);

        yield return new WaitForSecondsRealtime(duration);

        gamepad.SetMotorSpeeds(0f, 0f);
    }

    // Safety: Stop vibration if the game is paused or quit
    private void OnDisable()
    {
        Gamepad.current?.SetMotorSpeeds(0f, 0f);
    }
}
