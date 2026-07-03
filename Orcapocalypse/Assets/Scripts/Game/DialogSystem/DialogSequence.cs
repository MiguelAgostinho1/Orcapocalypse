using UnityEngine;
using System.Collections.Generic;

// This attribute adds a custom option to your right-click "Create" menu in Unity!
[CreateAssetMenu(fileName = "New Dialog Sequence", menuName = "Dialog System/Dialog Sequence")]
public class DialogSequence : ScriptableObject
{
    public List<DialogLine> lines = new List<DialogLine>();
}
