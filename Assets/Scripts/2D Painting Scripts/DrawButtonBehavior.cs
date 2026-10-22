using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DrawButtonBehavior : MonoBehaviour
{

    // ***VARIABLES***
    [SerializeField] BrushScriptableObject SBrush;
    [SerializeField] BrushScriptableObject MBrush;
    [SerializeField] BrushScriptableObject LBrush;


    // ***FUNCTIONS***
    // ---delegate for brush features---
    public delegate void OnBrushSelection(BrushScriptableObject Brush);
    public static OnBrushSelection onSize;
    
    // ---delegate for colors---
    public delegate void OnColorSelection(Color color);
    public static OnColorSelection onColor;

    // ---delegate for undo/redo
    public delegate void OnDrawSelection();
    public static OnDrawSelection onUndo;
    public static OnDrawSelection onRedo;
    
    public void GetSmallBrush()
    {
        onSize?.Invoke(SBrush);
    }

    public void GetMediumBrush()
    {
        onSize?.Invoke(MBrush);
    }
    
    public void GetLargeBrush()
    {
        onSize?.Invoke(LBrush);
    }

    public void ChangeColor(int buttonId)
    {
        switch (buttonId)
        {
            case 1:
                onColor?.Invoke(new Color(0.99f, 0.486f, 0.415f, 1f));
                break;
            case 2:
                onColor?.Invoke(new Color(0.99f, 0.603f, 0.278f, 1f));
                break;
            case 3:
                onColor?.Invoke(new Color(1f, 0.815f, 0.313f, 1f));
                break;
            case 4:
                onColor?.Invoke(new Color(0.796f, 1f, 0.368f, 1f));
                break;
            case 5:
                onColor?.Invoke(new Color(0.615f, 1f, 0.815f, 1f));
                break;
            case 6:
                onColor?.Invoke(new Color(0.615f, 0.976f, 1f, 1f));
                break;
            case 7:
                onColor?.Invoke(new Color(0.568f, 0.592f, 1f, 1f));
                break;
            case 8:
                onColor?.Invoke(new Color(0.803f, 0.564f, 1f, 1f));
                break;
            case 9:
                onColor?.Invoke(new Color(0.99f, 0.631f, 0.945f, 1f));
                break;
            default:
                Debug.Log("ERROR: INVALID COLOR");
                break;

        }
        
    }

    public void UndoBrushStroke()
    {
        onUndo?.Invoke();
    }

    public void RedoBrushStroke()
    {
        onRedo?.Invoke();
    }


}
