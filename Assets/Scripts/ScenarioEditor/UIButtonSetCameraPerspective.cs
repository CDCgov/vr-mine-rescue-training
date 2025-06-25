using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSetCameraPerspective : MonoBehaviour
{
    public ScenarioEditorCamera ScenarioEditorCamera;
    public bool TopDownPerspective;
    public bool AutoFindCamera = false;
    private Button _button;
    // Start is called before the first frame update
    void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
        if (AutoFindCamera && ScenarioEditorCamera == null)
        {
            ScenarioEditorCamera = (ScenarioEditorCamera)FindObjectOfType(typeof(ScenarioEditorCamera));
        }
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (ScenarioEditorCamera == null)
            return;

        //ScenarioEditorCamera.SetCameraPerspective(TopDownPerspective);

    }
}
