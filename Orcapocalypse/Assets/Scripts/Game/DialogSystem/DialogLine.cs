using UnityEngine;

// System.Serializable makes this struct visible in the Unity Inspector
[System.Serializable]
public struct DialogLine
{
    public string speakerName;
    public Sprite speakerPortrait;

    public enum PortraitPosition { Left, Right, None }
    public PortraitPosition portraitPlacement;

    [TextArea(3, 5)]
    public string text;

    public AudioClip voiceBlip;
}
