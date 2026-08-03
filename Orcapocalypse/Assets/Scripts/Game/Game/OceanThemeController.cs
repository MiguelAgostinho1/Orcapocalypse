using UnityEngine;
using static UnityEngine.Rendering.STP;

[ExecuteAlways]
public class OceanThemeController : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField] private OceanConfig oceanConfig;
    [SerializeField] private Renderer backgroundQuadRenderer;

    [Header("Seabed Renderers")]
    [SerializeField] private SpriteRenderer frontSeabedRenderer;
    [SerializeField] private SpriteRenderer backSeabedRenderer;

    private static readonly int SeaDeepColorID = Shader.PropertyToID("_SeaDeepColor");
    private static readonly int SeaMidColorID = Shader.PropertyToID("_SeaMidColor");
    private static readonly int SeaSurfaceColorID = Shader.PropertyToID("_SeaSurfaceColor");
    private static readonly int SkyBottomColorID = Shader.PropertyToID("_SkyBottomColor");
    private static readonly int SkyTopColorID = Shader.PropertyToID("_SkyTopColor");

    private static readonly int LevelTopYID = Shader.PropertyToID("_LevelTopY");
    private static readonly int WaterSurfaceYID = Shader.PropertyToID("_WaterSurfaceY");
    private static readonly int LevelBottomYID = Shader.PropertyToID("_LevelBottomY");

    private void OnEnable()
    {
        ApplyTheme();
    }

    private void Start()
    {
        ApplyTheme();
    }

    // 2. Called in the Editor whenever you tweak values in the Inspector or swap the Config asset
    private void OnValidate()
    {
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        if (oceanConfig == null || backgroundQuadRenderer == null) return;

        // 3. Use sharedMaterial in Edit Mode to avoid Unity's "material leak" warning
        Material targetMaterial = Application.isPlaying ? backgroundQuadRenderer.material : backgroundQuadRenderer.sharedMaterial;

        if (targetMaterial == null) return;

        // Send colors
        targetMaterial.SetColor(SkyTopColorID, oceanConfig.skyTopColor);
        targetMaterial.SetColor(SkyBottomColorID, oceanConfig.skyBottomColor);
        targetMaterial.SetColor(SeaSurfaceColorID, oceanConfig.seaSurfaceColor);
        targetMaterial.SetColor(SeaMidColorID, oceanConfig.seaMidColor);
        targetMaterial.SetColor(SeaDeepColorID, oceanConfig.seaDeepColor);

        // Send bounds
        targetMaterial.SetFloat(LevelTopYID, oceanConfig.TopYBound);
        targetMaterial.SetFloat(WaterSurfaceYID, oceanConfig.waterLevel);
        targetMaterial.SetFloat(LevelBottomYID, oceanConfig.oceanFloorY);

        // Seabed Tints
        if (frontSeabedRenderer != null) frontSeabedRenderer.color = oceanConfig.frontSeabedTint;
        if (backSeabedRenderer != null) backSeabedRenderer.color = oceanConfig.backSeabedTint;

        Debug.Log($"Applied Ocean Theme: Surface Y = {oceanConfig.waterLevel}, Sky Top = {oceanConfig.skyTopColor}");
    }
}