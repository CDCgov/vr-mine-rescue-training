using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ModalYesNoCancel : UIDialogResult
{

    public TMPro.TMP_Text LabelField;
    public TMPro.TMP_Text YesButtonText;
    public TMPro.TMP_Text NoButtonText;

    public Button YesButton;
    public Button NoButton;
    public Button CancelButton;

    //private DialogResult _result = DialogResult.None;

    private static ModalYesNoCancel _instance;

    public static void ShowDialog(string labelText, string yesText, string noText, DialogResult defaultResult, DialogResultCallback callback)
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<ModalYesNoCancel>(true);
        }

        if (_instance == null)
        {
            callback?.Invoke(defaultResult);
            return;
        }

        _instance.ShowDialog(labelText, yesText, noText, callback);
    }

    public void ShowDialog(string labelText, string yesText, string noText, DialogResultCallback callback)
    {
        InitializeDialog(labelText, yesText, noText);
        this.ShowDialog(callback);
    }

    private void InitializeDialog(string labelText, string yesText, string noText)
    {
        if (LabelField != null)
            LabelField.text = labelText;
        if (YesButtonText != null)
            YesButtonText.text = yesText;
        if (NoButtonText != null)
            NoButtonText.text = noText;

        gameObject.SetActive(true);
    }

    private void Start()
    {
        if (YesButton != null)
            YesButton.onClick.AddListener(OnYesButton);
        if (NoButton != null)
            NoButton.onClick.AddListener(OnNoButton);
        if (CancelButton != null)
            CancelButton.onClick.AddListener(OnCancelButton);
    }

    private void OnCancelButton()
    {
        //_result = DialogResult.Cancel;
        SetDialogResult(DialogResult.Cancel);
    }

    private void OnNoButton()
    {
        //_result = DialogResult.No;
        SetDialogResult(DialogResult.No);
    }

    private void OnYesButton()
    {
        //_result = DialogResult.Yes;
        SetDialogResult(DialogResult.Yes);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SetDialogResult(DialogResult.Cancel);
        //_result = DialogResult.Cancel;
    }

}
