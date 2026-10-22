using UnityEngine;

public class ClearCanvas : CanvasCommand
{
    Color clearToColor;
    Texture2DWrapper paintableTexture;

    int canvasWidth;
    int canvasHeight;

    public ClearCanvas(Color color, Texture2DWrapper texture, int width, int height) 
    {
        clearToColor = color;
        paintableTexture = texture;

        canvasWidth = width;
        canvasHeight = height;

    }

    public void Execute()
    { 
        // edit paintable texture (fill color)
        for (int y = 0; y < canvasHeight; y++)
        {
            for (int x = 0; x < canvasWidth; x++)
            {
                paintableTexture.SetPixel(x, y, clearToColor);
            }
        }
    }

    public void Undo()
    {
        Execute();
    }
}
