using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayPauseButtonHandler : MonoBehaviour
{
    public SessionPlaybackControl SessionPlaybackControl;

    public Image PlaySymbolImage;
    Button _playButton;
    //bool _isPlaying = false;

    //public bool IsPlaying
    //{
    //    get { return _isPlaying; }
    //    set
    //    {
    //        _isPlaying = value;
    //        SetSprite();
    //    }
    //}
    public Sprite PlaySprite;
    public Sprite PauseSprite;
    // Start is called before the first frame update
    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);

        _playButton = GetComponent<Button>();

        SessionPlaybackControl.PlaybackSpeedChanged += OnPlaybackSpeedChanged;

        SetSprite();
    }

    private void OnPlaybackSpeedChanged()
    {
        SetSprite();
    }

    public void OnClickBehavior()
    {
        //_isPlaying = !_isPlaying;
        //SetSprite();
    }

    void SetSprite()
    {
        if (SessionPlaybackControl.IsPlaying)
        {
            PlaySymbolImage.sprite = PauseSprite;
            PlaySymbolImage.color = Color.gray;
        }
        else
        {
            PlaySymbolImage.sprite = PlaySprite;
            PlaySymbolImage.color = Color.green;
        }
    }
}
