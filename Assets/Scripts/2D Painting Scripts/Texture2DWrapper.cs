using UnityEngine;

/*
* Used only by ClearCanvas with original attempt
* to optimize draw calls so they don't happen as often
* 
* (Intending to keep this around, in case I need to convert
* back to Texture2D when player is finished with their art)
*/

public class Texture2DWrapper 
{
    Texture2D linkedTexture;
    Color[] PixelData;

    int linkedTextureWidth;
    int linkedTextureHeight;

    // meant to limit amount of times we are updating
    public bool isDirty { get; private set; } = true;

    // constructor - creates a Texture2DWrapper
    public Texture2DWrapper(Texture2D texture)
    {
        linkedTexture = texture;

        linkedTextureWidth = linkedTexture.width;
        linkedTextureHeight = linkedTexture.height;

        PixelData = linkedTexture.GetPixels();
    }

    // sets colors of pixels of texture
    public void SetPixel(int pix_X, int pix_Y, Color tColor) // used in ClearCanvas (1)
    {
        PixelData[pix_X + (pix_Y * linkedTextureWidth)] = tColor;
        isDirty = true;
    }


    // displays changes on texture
    public void Apply()
    {
        if (isDirty)
        {
            isDirty = false;
            linkedTexture.SetPixels(PixelData);

            linkedTexture.Apply();
        }
        
    }

}
