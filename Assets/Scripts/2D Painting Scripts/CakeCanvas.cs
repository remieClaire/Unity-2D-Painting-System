using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CakeCanvas : MonoBehaviour
{
    // ---brush object vars---
    Color cColor;
    BrushScriptableObject cBrush;
    
    // ---canvas info vars---
    int canvasWidthInPixels;
    int canvasHeightInPixels;

    [SerializeField] int PixelsPerMetre = 100; // the larger this is, the more precise we can paint
    
    // ---paintable texture vars---
    private Sprite cakeSprite;
    private SpriteRenderer cakeSpriteRenderer;
    private Texture2DWrapper paintableCakeTexture;
    [SerializeField] Color texColor; 

    // ---raycasting & mouse input vars---
    RaycastHit2D hitResults;

    [SerializeField] float raycastDistance = 10.0f;
    [SerializeField] LayerMask cakeCanvasLayerMask = ~0; // means all layers will be selected

    bool canExtend = false;

    // ---shader & shader-related vars---
    public ComputeShader computeShader;
    public RenderTexture renderTexture;

    // ---drawing fixing related vars---
    Vector2 oldMousePos;
    Vector2 oldTexCoord;

    float maxDist = 0.0001f;

    // ---button-related functions---
    // activates certain actions depending on buttons clicked
    void OnEnable()
    {
        DrawButtonBehavior.onSize += SelectBrush;
        DrawButtonBehavior.onColor += SetColor;

        DrawButtonBehavior.onUndo += CallUndo;
        DrawButtonBehavior.onRedo += CallRedo;
    }

    // ---game loop---
    // canvas configuration
    void Start()
    {
        // initialization of general use vars
        cakeSprite = GetComponent<SpriteRenderer>().sprite;
        cakeSpriteRenderer = GetComponent<SpriteRenderer>();

        // get correct bounds due to having a non-square canvas
        canvasWidthInPixels = Mathf.CeilToInt(cakeSpriteRenderer.bounds.size.x * cakeSpriteRenderer.transform.localScale.x * PixelsPerMetre);
        canvasHeightInPixels = Mathf.CeilToInt(cakeSpriteRenderer.bounds.size.y * cakeSpriteRenderer.transform.localScale.y * PixelsPerMetre);

        // created our paintable texture
        Texture2D localTexture = new Texture2D(canvasWidthInPixels, canvasHeightInPixels, TextureFormat.ARGB32, false);
        
        // apply paintable texture over cake
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetTexture("_MainTex", localTexture);
        cakeSpriteRenderer.SetPropertyBlock(block);

        // convert our texture2D to a texture2Dwrapper to optimize pixel calls
        paintableCakeTexture = new Texture2DWrapper(localTexture);

        // clear the canvas (prevents cake from spawning with gray texture)
        CanvasCommand.Instance.AddCommand(new ClearCanvas(texColor, paintableCakeTexture, canvasWidthInPixels, canvasHeightInPixels));
        paintableCakeTexture.Apply();

        // create render texture dest for shader (the texture we edit!)
        renderTexture = new RenderTexture(cakeSprite.texture.width, cakeSprite.texture.height, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

    }

    // checks for drawing input
    void Update()
    {
        if (cBrush != null)
        {
            // stores the left mouse button as an action that we can check the status of
            InputAction leftClickAction = InputSystem.actions.FindAction("Click");
            
            // if mouse was simply clicked (indicates player starting a new brush stroke)
            if (leftClickAction.WasPressedThisFrame())
            {
                // grab initial mouse position
                oldMousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                // convert initial mouse position to tex coordinates
                oldTexCoord = UVConvert(oldMousePos);
            }

            // if mouse is currently drawing
            if (leftClickAction.IsPressed())
            {
                UpdateDrawing();
            }
            // if player is done drawing brush stroke
            else if (leftClickAction.WasReleasedThisFrame())
            {
                // gives signal to create a new draw command rather than extend old one
                canExtend = false;
                // update texture to display our new edits
                StartCoroutine(UpdateCanvas());   
            }
        }
            
    }

    // ---canvas drawing functions---
    // gets data needed for painting
    private void UpdateDrawing()
    {
        Ray drawingRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        hitResults = Physics2D.GetRayIntersection(drawingRay, raycastDistance, cakeCanvasLayerMask);

        // if cake was clicked
        if (hitResults)
        {
            // get texture coordinates where player clicked
            Vector2 texCoord = UVConvert(hitResults.point);

            // apply paint (via command pattern)
            if (canExtend && CanvasCommand.Instance.recentCommand is DrawCommand)
            {
                float dist = (texCoord - oldTexCoord).sqrMagnitude;

                // if there is a noticeable gap between pixels
                if (dist > maxDist)
                {
                    // fill gap with more pixels
                    StartCoroutine(ClosePixelGaps(dist, texCoord));
                }
                else
                {
                    // draw like normal
                    DrawCommand drawCmd = (CanvasCommand.Instance.recentCommand as DrawCommand);
                    drawCmd.Extend(texCoord);
                    drawCmd.SetExtendable(canExtend);
                    drawCmd.Execute();
                }
                
            }
            else
            {
                CanvasCommand.Instance.AddCommand(new DrawCommand
                    (canvasWidthInPixels, 
                    canvasHeightInPixels, 
                    texCoord, 
                    cBrush, 
                    paintableCakeTexture, 
                    cColor, 
                    canExtend, 
                    computeShader,
                    renderTexture
                ));
                canExtend = true;
            }


        }
       
    }

    // makes up for skips between strokes if player draws too fast
    IEnumerator ClosePixelGaps(float dist, Vector2 texCoord)
    {
        int step = Mathf.CeilToInt(dist / maxDist);
        step *= 2;
    
        for (int i = 1; i < step; i++)
        {
            DrawCommand drawCmd = (CanvasCommand.Instance.recentCommand as DrawCommand);
            drawCmd.Extend(Vector2.Lerp(oldTexCoord, texCoord, (float)i / step));
            drawCmd.SetExtendable(canExtend);
            drawCmd.Execute();
        }
        
        oldTexCoord = texCoord;
        yield return null;
    }

    // displays changes we made painting
    IEnumerator UpdateCanvas()
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetTexture("_MainTex", renderTexture);
        cakeSpriteRenderer.SetPropertyBlock(block);

        yield return null;
    }

    // converts clicks to texture coordinates
    private Vector2 UVConvert(Vector2 clickPos)
    {
        float ppu = cakeSprite.pixelsPerUnit;

        // local position on sprite in pixel unit
        Vector2 localPos = transform.InverseTransformPoint(clickPos) * ppu;

        // get the texture coordinate
        // get pivot point of sprite first
        Vector2 texSpacePivot = new Vector2(cakeSprite.rect.x, cakeSprite.rect.y) + cakeSprite.pivot;
        // apply pivot to sprite's local position
        Vector2 texSpaceCoord = texSpacePivot + localPos;

        Texture2D tex = cakeSprite.texture;

        // convert from pixels to UV coords
        Vector2 UV = texSpaceCoord;
        UV.x /= tex.width;
        UV.y /= tex.height;

        return UV;
    }

    // sets brush
    private void SelectBrush(BrushScriptableObject brush)
    {
        cBrush = brush;

        // setting scale of brush
        // int scaledBrushWidth = Mathf.RoundToInt(cBrush.BrushTexture.width * cBrush.BrushThickness);
        // int scaledBrushHeight = Mathf.RoundToInt(cBrush.BrushTexture.height * cBrush.BrushThickness);

        // cBrush.SetScale(scaledBrushWidth, scaledBrushHeight);
    }
    
    // sets color
    private void SetColor(Color color)
    {
        cColor = color;
    }

    private void CallUndo()
    {
        // set render texture active to do operations on
        RenderTexture.active = renderTexture;
        CanvasCommand.Instance.UndoCommand();
    }

    private void CallRedo()
    {
        // set render texture active to do operations on
        RenderTexture.active = renderTexture;
        CanvasCommand.Instance.RedoCommand();
    }

}
