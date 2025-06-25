using UnityEngine.UI;
using UnityEngine;

public class ScenarioEditorCanvasSwapper : MonoBehaviour
{
    public CanvasGroup startingCanvas;
    CanvasGroup currentCanvas;

    private void Awake()
    {
        currentCanvas = startingCanvas;
        if(currentCanvas == null)
        {
            currentCanvas = FindObjectOfType<CanvasGroup>();
        }
    }
    public void MoveToNewCanvasGroup(CanvasGroup group)
    {
        currentCanvas.alpha = 0f;
        currentCanvas.interactable = false;
        currentCanvas.blocksRaycasts = false;

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        currentCanvas = group;
    }
}
