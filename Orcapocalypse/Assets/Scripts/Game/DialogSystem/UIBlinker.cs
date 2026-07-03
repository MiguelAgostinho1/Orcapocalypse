using UnityEngine;
using UnityEngine.UI; // Needed to access UI components like Image and Text

// This forces Unity to make sure whatever we attach this to has a UI component
[RequireComponent(typeof(Graphic))]
public class UIBlinker : MonoBehaviour
{
    [Header("Blink Settings")]
    public float blinkSpeed = 8f; // Higher is faster
    [Range(0f, 1f)] public float minAlpha = 0.2f; // How faded it gets (0 = totally invisible)
    [Range(0f, 1f)] public float maxAlpha = 1.0f; // How solid it gets (1 = fully visible)

    private Graphic uiElement;

    private void Awake()
    {
        // Grab the Image or Text component on this GameObject
        uiElement = GetComponent<Graphic>();
    }

    private void Update()
    {
        if (uiElement != null)
        {
            // Use a Sine wave to smoothly transition a number back and forth
            float wave = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;

            // Apply that wave to our min and max alpha settings
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, wave);

            // Set the new color transparency
            Color newColor = uiElement.color;
            newColor.a = currentAlpha;
            uiElement.color = newColor;
        }
    }
}