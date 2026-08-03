using UnityEngine;

[CreateAssetMenu(fileName = "OceanConfig", menuName = "Config/OceanData")]
public class OceanConfig : ScriptableObject
{
    [Header("Sky Gradient")]
    public Color skyTopColor = new Color(0.4f, 0.7f, 1.0f);
    public Color skyBottomColor = new Color(0.7f, 0.9f, 1.0f);

    [Header("Sea Gradient")]
    public Color seaSurfaceColor = new Color(0.0f, 0.7f, 0.85f);
    public Color seaMidColor = new Color(0.0f, 0.35f, 0.65f);
    public Color seaDeepColor = new Color(0.01f, 0.08f, 0.25f);

    [Header("Seabed Tints")]
    public Color frontSeabedTint = Color.white;
    public Color backSeabedTint = new Color(0.5f, 0.5f, 0.7f);

    [Header("Ocean Properties")]
    public float waterLevel = -0.08f;
    public float minXBound = -82f;
    public float maxXBound = 82f;
    public float TopYBound = 10f;
    public float oceanFloorY = -40f;
}