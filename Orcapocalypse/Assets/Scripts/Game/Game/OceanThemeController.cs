using UnityEngine;
using static UnityEngine.Rendering.STP;

[ExecuteAlways]
public class OceanThemeController : MonoBehaviour
{
    [Header("Level Configuration")]
    [SerializeField] private OceanConfig _oceanConfig;
    [SerializeField] private Renderer _backgroundQuadRenderer;

    [Header("Seabed Renderers")]
    [SerializeField] private SpriteRenderer _frontSeabedRenderer;
    [SerializeField] private SpriteRenderer _backSeabedRenderer;

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
        if (_oceanConfig == null || _backgroundQuadRenderer == null) return;

        // 3. Use sharedMaterial in Edit Mode to avoid Unity's "material leak" warning
        Material targetMaterial = Application.isPlaying ? _backgroundQuadRenderer.material : _backgroundQuadRenderer.sharedMaterial;

        if (targetMaterial == null) return;

        // Send colors
        targetMaterial.SetColor(SkyTopColorID, _oceanConfig.skyTopColor);
        targetMaterial.SetColor(SkyBottomColorID, _oceanConfig.skyBottomColor);
        targetMaterial.SetColor(SeaSurfaceColorID, _oceanConfig.seaSurfaceColor);
        targetMaterial.SetColor(SeaMidColorID, _oceanConfig.seaMidColor);
        targetMaterial.SetColor(SeaDeepColorID, _oceanConfig.seaDeepColor);

        // Send bounds
        targetMaterial.SetFloat(LevelTopYID, _oceanConfig.TopYBound);
        targetMaterial.SetFloat(WaterSurfaceYID, _oceanConfig.waterLevel);
        targetMaterial.SetFloat(LevelBottomYID, _oceanConfig.oceanFloorY);

        // Seabed Tints
        if (_frontSeabedRenderer != null) _frontSeabedRenderer.color = _oceanConfig.frontSeabedTint;
        if (_backSeabedRenderer != null) _backSeabedRenderer.color = _oceanConfig.backSeabedTint;

        Debug.Log($"Applied Ocean Theme: Surface Y = {_oceanConfig.waterLevel}, Sky Top = {_oceanConfig.skyTopColor}");
    }
}