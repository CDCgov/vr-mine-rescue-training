using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnResetSceneObjects : MonoBehaviour
{
    public SceneObjectResetManager SceneObjectResetManager;

    public ObjectResetCategory RespawnCategory;

    private Button _button;

    // Start is called before the first frame update
    void Start()
    {
        if (SceneObjectResetManager == null)
            SceneObjectResetManager = SceneObjectResetManager.GetDefault(gameObject);

        _button = GetComponent<Button>();

        _button.onClick.AddListener(() =>
        {
            SceneObjectResetManager.ResetObjects(RespawnCategory);
        });
    }
}
