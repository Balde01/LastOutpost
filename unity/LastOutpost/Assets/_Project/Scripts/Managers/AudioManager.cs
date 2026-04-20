using NaughtyAttributes;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Foldout("Intro Audio"), SerializeField] private AudioSource _voiceSource;
    [Foldout("Intro Audio"), SerializeField] private AudioClip _fullIntroClip;

    private bool ValidateReferences()
    {
        return ValidatorReferences.Validate(this,
            (_voiceSource, nameof(_voiceSource)),
            (_fullIntroClip, nameof(_fullIntroClip))
        );
    }

    public void PlayFullIntro()
    {
        if (!ValidateReferences())
        {
            return;
        }

        if (_voiceSource.isPlaying && _voiceSource.clip == _fullIntroClip)
        {
            return;
        }

        _voiceSource.clip = _fullIntroClip;
        _voiceSource.Play();
    }

    public void StopFullIntro()
    {
        if (_voiceSource == null)
        {
            return;
        }

        _voiceSource.Stop();
    }

    public float GetIntroTime()
    {
        if (_voiceSource == null)
        {
            return 0f;
        }

        return _voiceSource.time;
    }

    public bool IsIntroPlaying()
    {
        return _voiceSource != null && _voiceSource.isPlaying;
    }
}