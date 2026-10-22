using UnityEngine;
using System.Collections.Generic;

public class DrawCommand : CanvasCommand
{
    // ---drawing variables---
    int canvasWidth;
    int canvasHeight;

    BrushScriptableObject activeBrush;
    Color activeColor;

    // Texture2DWrapper paintableTexture; // (related to Texture2DWrapper)

    List<Vector2> UVPoints = new();

    bool isExtendable;

    // ---undo variables---
    int kernelID;
    ComputeShader drawShader;
    RenderTexture paintableRenderTexture;

    // ---drawing functions---
    // creates a draw command
    public DrawCommand(
        int width,
        int height,
        Vector2 UV, 
        BrushScriptableObject brush, 
        Texture2DWrapper texture,
        Color color,
        bool extend,
        ComputeShader shader,
        RenderTexture renderTex
    ) 
    {
        UVPoints.Add(UV); // adds points we've clicked over

        canvasWidth = width;
        canvasHeight = height;

        activeBrush = brush;
        activeColor = color;

        // paintableTexture = texture;

        isExtendable = extend;

        drawShader = shader;
        kernelID = drawShader.FindKernel("CSDraw");

        paintableRenderTexture = renderTex;
    }

    // gathers next consecutive pixels into this existing draw cmd
    public void Extend(Vector2 UV)
    {
        UVPoints.Add(UV);
    }

    public void Execute()
    {
        
        if (isExtendable)
        {
            // draw the last added pixel
            DrawShader(UVPoints[^1]);
        }
        else 
        {
            foreach (var point in UVPoints)
            {
                DrawShader(point);
            }
        }

    }

    public void Undo()
    {
        foreach (var point in UVPoints)
        {
            DrawShader(point);
        }
    }

    private void DrawShader(Vector2 texCoord)
    {
        // convert texture coordinate to pixel coordinate
        float tex_X = Mathf.FloorToInt(canvasWidth * texCoord.x);
        float tex_Y = Mathf.FloorToInt(canvasHeight * texCoord.y);

        Vector2 inPixelCoord = new Vector2(tex_X, tex_Y);
        
        // send variables to shader
        drawShader.SetVector("PixelCoord", inPixelCoord);
        drawShader.SetTexture(kernelID, "ResultTexture", paintableRenderTexture);
        drawShader.SetFloat("DrawSize", activeBrush.BrushTexture.width * activeBrush.BrushThickness);
        drawShader.SetVector("DrawColor", activeColor);

        drawShader.SetTexture(kernelID, "DrawTex", activeBrush.BrushTexture);

        int threadGroupX = Mathf.CeilToInt(canvasWidth / 8.0f);
        int threadGroupY = Mathf.CeilToInt(canvasHeight / 8.0f);

        drawShader.Dispatch(kernelID, threadGroupX, threadGroupY, 1);
    }
    
    public void SetExtendable(bool extend)
    {
        isExtendable = extend;
    }

}
