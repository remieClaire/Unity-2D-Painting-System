using UnityEngine;

[CreateAssetMenu(fileName = "BrushScriptableObject", menuName = "Scriptable Objects/BrushScriptableObject")]
public class BrushScriptableObject : ScriptableObject
{
    public string DisplayName;
    public Texture2D BrushTexture;
    public float BrushThickness;

    public int Width { get; private set; } = -1;
    public int Height { get; private set; } = -1;

}
