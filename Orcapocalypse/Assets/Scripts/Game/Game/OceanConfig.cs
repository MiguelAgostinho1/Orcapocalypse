using UnityEngine;

[CreateAssetMenu(fileName = "OceanConfig", menuName = "Config/OceanData")]
public class OceanConfig : ScriptableObject
{
    public float waterLevel = -0.08f;
    public float minXBound = -82f;
    public float maxXBound = 82f;
    public float oceanFloorY = -40f;
}