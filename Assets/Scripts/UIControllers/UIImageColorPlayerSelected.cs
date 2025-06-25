using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIImageColorPlayerSelected : MonoBehaviour, ISelectedPlayerView
{
    //public UIBtnSelectPlayer SourcePlayer;
    public Color SelectedColor;
    public Color NormalColor;
    public string SelectedPlayerVar = "SELECTED_PLAYER";

    private UIContextData _context;
    private Image _image;
    private PlayerRepresentation _player;

    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
        _context = transform.GetComponentInParent<UIContextData>();

        _context.ContextDataChanged += OnContextDataChanged;
        _image.color = NormalColor;

        OnContextDataChanged(null);
    }

    private void OnContextDataChanged(string obj)
    {
        if (_context == null || _image == null || SelectedPlayerVar == null || SelectedPlayerVar.Length <= 0 )
            return;

        PlayerRepresentation player = _context.GetVariable(SelectedPlayerVar) as PlayerRepresentation;
        if (player == _player)
        {
            _image.color = SelectedColor;
        }
        else
        {
            _image.color = NormalColor;
        }
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        if (_player == null)
            _player = player;
    }

    //public void SetPlayer(PlayerRepresentation player)
    //{
    //    if (_image == null)
    //        return;

    //    if (player == SourcePlayer.PlayerToSelect)
    //    {
    //        _image.color = SelectedColor;
    //    }
    //    else
    //    {
    //        _image.color = NormalColor;
    //    }
    //}

}
