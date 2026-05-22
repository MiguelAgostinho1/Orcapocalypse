using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Player/AbilityDatabase")]
public class AbilityDatabase : ScriptableObject
{
    public PlayerAbility[] allAbilities;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // The database handles auto-collecting all moves in the project
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:PlayerAbility");
        allAbilities = new PlayerAbility[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
            allAbilities[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<PlayerAbility>(path);
        }
    }
#endif
}