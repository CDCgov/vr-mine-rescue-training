using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorImageController : MonoBehaviour
{
    public static CursorImageController instance { get; private set; }

    private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] Vector2 hotSpot = Vector2.zero;
    // Start is called before the first frame update
    public enum CursorImage
    {
        Arrow,
        HandPointing,
        HandPointingPressed,
        HandOpen,
        HandClosed,
        Resize_EW,
        Resize_NS,
        Resize_NW_SE,
        Resize_NE_SW,
        Caret,
        Invalid,
    }

    public CursorImage cursorState;

    [SerializeField] Texture2D arrowTexture;
    [SerializeField] Texture2D handPointingTexture;
    [SerializeField] Texture2D handPointingPressedTexture;

    [SerializeField] Texture2D handOpenTexture;
    [SerializeField] Texture2D handClosedTexture;

    [SerializeField] Texture2D resize_EW_Texture;
    [SerializeField] Texture2D resize_NS_Texture;
    [SerializeField] Texture2D resize_NW_SE_Texture;
    [SerializeField] Texture2D resize_NE_SW_Texture;
    [SerializeField] Texture2D invalidTexture;

    [SerializeField] Texture2D caretTexture;
    
    public WindowManipulationZone.ManipulationZone queuedManipulationZone;
    public WindowManipulationZone.ManipulationZone currentManipulationZone;
    public WindowManipulationZone.ManipulationZone lastManipulationZone;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        ChangeCursorImage(CursorImage.Arrow);
    }


    // Change the cursor image from an enum
    public void ChangeCursorImage(CursorImage cursorImage)
    {
        cursorState = cursorImage;
        Texture2D selectedImage = null;
        
        if(cursorImage == CursorImage.Arrow)
        {
            hotSpot = Vector2.zero;
        }
        else
        {
            hotSpot = new Vector2(16,16);
        }


        switch (cursorImage)
        {
            case CursorImage.Arrow:
                selectedImage = null;
                break;
            case CursorImage.HandPointing:
                selectedImage = handPointingTexture;
                break;
            case CursorImage.HandPointingPressed:
                selectedImage = handPointingPressedTexture;
                break;
            case CursorImage.HandOpen:
                selectedImage = handOpenTexture;
                break;
            case CursorImage.HandClosed:
                selectedImage = handClosedTexture;
                break;
            case CursorImage.Resize_EW:
                selectedImage = resize_EW_Texture;
                break;
            case CursorImage.Resize_NS:
                selectedImage = resize_NS_Texture;
                break;
            case CursorImage.Resize_NW_SE:
                selectedImage = resize_NW_SE_Texture;
                break;
            case CursorImage.Resize_NE_SW:
                selectedImage = resize_NE_SW_Texture;
                break;
            case CursorImage.Caret:
                selectedImage = caretTexture;
                break;
            case CursorImage.Invalid:
                selectedImage = invalidTexture;
                break;
        }

        Cursor.SetCursor(selectedImage, hotSpot, cursorMode);
    }
}
