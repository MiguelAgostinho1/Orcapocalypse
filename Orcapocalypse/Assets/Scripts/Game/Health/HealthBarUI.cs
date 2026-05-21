using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField]
    private Image _healthBarForegroundImage;

    private void UpdateHealthBar(HealthController healthController)
    {
        _healthBarForegroundImage.fillAmount = healthController.RemainingHealthPercentage;
    }
}
