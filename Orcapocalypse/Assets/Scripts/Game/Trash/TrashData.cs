using UnityEngine;

[CreateAssetMenu(fileName = "NewTrash", menuName = "Ocean/TrashData")]
public class TrashData : ScriptableObject
{
    public string trashName;
    public Sprite visualSprite;
    public float damageAmount = 10f;
    public float sinkSpeed = 2f;
    public float rotationSpeed = 20f;
    public float baseScale = 0.3f;
}