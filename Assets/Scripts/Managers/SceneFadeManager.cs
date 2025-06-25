using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using System.Threading.Tasks;

public class SceneFadeManager : SceneManagerBase
{
    public static SceneFadeManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<SceneFadeManager>("SceneFadeManager");
    }

    public AnimationCurve FadeOutCurve;
    public AnimationCurve FadeInCurve;
    public float DefaultFadeDuration = 1.5f;
    public bool FadeAudio = true;

    private LiftGammaGain _liftGammaGain;


    // Start is called before the first frame update
    void Start()
    {
        _liftGammaGain = GetGainOverride();

        Util.DontDestroyOnLoad(gameObject);
    }

    public async Task FadeOut(float duration = -1)
    {
        await Fade(duration, FadeOutCurve, 0);
    }



    public async Task FadeIn(float duration = -1)
    {
        await Fade(duration, FadeInCurve, 1);
    }

    private async Task Fade(float duration, AnimationCurve curve, float finalValue)
    {
        if (duration < 0)
            duration = DefaultFadeDuration;

        try
        {
            var startTime = Time.time;

            while (true)
            {
                var elapsed = Time.time - startTime;
                var progress = elapsed / duration;
                var val = Mathf.Clamp(curve.Evaluate(progress), 0, 1);
                //Debug.Log($"Fade Progress : {val:F1}");

                if (progress >= 1)
                    break;

                var gain = val - 1.0f;
                _liftGammaGain.gain.value = new Vector4(1, 1, 1, gain);

                if (FadeAudio)
                    SetAudioGain(val);

                await Task.Yield();
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Error during fade {ex.Message}");
        }

        _liftGammaGain.gain.value = new Vector4(1, 1, 1, finalValue - 1.0f);

        if (FadeAudio)
            SetAudioGain(finalValue);
    }

    private void SetAudioGain(float val)
    {
        //if (val <= 0 && !AudioListener.pause)
        //    AudioListener.pause = true;
        //if (val > 0 && AudioListener.pause)
        //    AudioListener.pause = false;

        AudioListener.volume = val;
    }

    private LiftGammaGain GetGainOverride()
    {
        LiftGammaGain liftGammaGain;

        var vol = gameObject.GetComponent<Volume>();
        if (vol == null)
            vol = gameObject.AddComponent<Volume>();

        vol.isGlobal = true;

        if (vol.profile == null)
        {
            var volProfile = (VolumeProfile)ScriptableObject.CreateInstance(typeof(VolumeProfile));
            vol.profile = volProfile;
        }

        if (!vol.profile.TryGet<LiftGammaGain>(out liftGammaGain))
        {
            liftGammaGain = vol.profile.Add<LiftGammaGain>();
        }

        liftGammaGain.lift.overrideState = false;
        liftGammaGain.gamma.overrideState = false;
        liftGammaGain.gain.overrideState = true;

        return liftGammaGain;
    }
}
