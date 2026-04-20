using System;
using System.Collections;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroUI : MonoBehaviour
{
    [Foldout("References"), SerializeField] private Image _fadeImage;
    [Foldout("References"), SerializeField] private Image _studioImage;
    [Foldout("References"), SerializeField] private TextMeshProUGUI _initializingText;
    [Foldout("References"), SerializeField] private Image _initializingBackground;
    [Foldout("References"), SerializeField] private AudioManager _audioManager;

    [Foldout("Text"), SerializeField] private string _initializingMessage = "INITIALIZING...";

    [Header("Timeline (in seconds)")]
    [Foldout("Timeline"), Min(0f), SerializeField] private float _prewarmDuration = 1f;
    [Foldout("Timeline"), Min(0f), SerializeField] private float _studioStartTime = 0f;
    [Foldout("Timeline"), Min(0f), SerializeField] private float _studioEndTime = 2f;
    [Foldout("Timeline"), Min(0f), SerializeField] private float _initializingStartTime = 2f;
    [Foldout("Timeline"), Min(0f), SerializeField] private float _initializingEndTime = 4f;
    [Foldout("Timeline"), Min(0f), SerializeField] private float _handoffBlackStartTime = 4f;
    [Foldout("Timeline"), Min(0f), SerializeField] private float _handoffBlackEndTime = 5f;

    [Foldout("Fade"), Min(0.01f), SerializeField] private float _fadeDuration = 0.35f;

    [Foldout("Debug"), SerializeField] private bool _playOnStart = true;

    public event Action OnBootVisualsFinished;
    public event Action OnIntroUIFinished;

    private Coroutine _bootSequenceCoroutine;
    private LTDescr _fadeTween;
    private bool _bootVisualsEventSent;
    private bool _introFinishedEventSent;

    private void Awake()
    {
        InitializeVisualState();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }
        if (_playOnStart)
        {
            StartCoroutine(PreWarmAndStart());
        }
    }

    private IEnumerator PreWarmAndStart() 
    { 
        yield return new WaitForSeconds(_prewarmDuration);
        PlayBootSequence();
    }

    private bool ValidateReferences()
    {
        return ValidatorReferences.Validate(this,
            (_fadeImage, nameof(_fadeImage)),
            (_studioImage, nameof(_studioImage)),
            (_initializingText, nameof(_initializingText)),
            (_audioManager, nameof(_audioManager)),
            (_initializingBackground, nameof(_initializingBackground))
        );
    }

    private void InitializeVisualState()
    {
        if (_studioImage != null)
        {
            _studioImage.gameObject.SetActive(false);
        }

        if (_initializingText != null)
        {
            _initializingText.gameObject.SetActive(false);
            _initializingText.text = _initializingMessage;
        }

        if (_initializingBackground != null)
        {
            _initializingBackground.gameObject.SetActive(false);
        }

        SetFadeAlpha(1f);
    }

    [Button("Play Boot Sequence")]
    public void PlayBootSequence()
    {
        if (!ValidateReferences())
        {
            return;
        }

        StopCurrentSequence();

        _bootVisualsEventSent = false;
        _introFinishedEventSent = false;

        InitializeVisualState();
        _audioManager.PlayFullIntro();

        _bootSequenceCoroutine = StartCoroutine(BootSequence());
    }

    private IEnumerator BootSequence()
    {
        while (_audioManager.IsIntroPlaying())
        {
            float currentTime = _audioManager.GetIntroTime();

            UpdateStudioVisual(currentTime);
            UpdateInitializingVisual(currentTime);
            UpdateBlackScreen(currentTime);
            DispatchEvents(currentTime);

            if (currentTime >= _handoffBlackEndTime)
            {
                break;
            }

            yield return null;
        }

        HideAllVisuals();
        SetFadeAlpha(1f);

        if (!_bootVisualsEventSent)
        {
            _bootVisualsEventSent = true;
            OnBootVisualsFinished?.Invoke();
        }

        if (!_introFinishedEventSent)
        {
            _introFinishedEventSent = true;
            OnIntroUIFinished?.Invoke();
        }

        _bootSequenceCoroutine = null;
    }

    private void UpdateStudioVisual(float currentTime)
    {
        bool shouldShow = currentTime >= _studioStartTime && currentTime < _studioEndTime;
        _studioImage.gameObject.SetActive(shouldShow);

        if (!shouldShow)
        {
            return;
        }

        float localTime = currentTime - _studioStartTime;
        float segmentDuration = _studioEndTime - _studioStartTime;
        float fadeOutStart = Mathf.Max(0f, segmentDuration - _fadeDuration);

        if (localTime < _fadeDuration)
        {
            float alpha = Mathf.Clamp01(localTime / _fadeDuration);
            SetFadeAlpha(1f - alpha);
        }
        else if (localTime >= fadeOutStart)
        {
            float alpha = Mathf.Clamp01((localTime - fadeOutStart) / _fadeDuration);
            SetFadeAlpha(alpha);
        }
        else
        {
            SetFadeAlpha(0f);
        }
    }

    private void UpdateInitializingVisual(float currentTime)
    {
        bool shouldShow = currentTime >= _initializingStartTime && currentTime < _initializingEndTime;

        _initializingText.gameObject.SetActive(shouldShow);
        _initializingBackground.gameObject.SetActive(shouldShow);

        if (!shouldShow)
        {
            return;
        }

        _initializingText.text = _initializingMessage;

        float localTime = currentTime - _initializingStartTime;
        float segmentDuration = _initializingEndTime - _initializingStartTime;
        float fadeOutStart = Mathf.Max(0f, segmentDuration - _fadeDuration);

        if (localTime < _fadeDuration)
        {
            float alpha = Mathf.Clamp01(localTime / _fadeDuration);
            SetFadeAlpha(1f - alpha);
        }
        else if (localTime >= fadeOutStart)
        {
            float alpha = Mathf.Clamp01((localTime - fadeOutStart) / _fadeDuration);
            SetFadeAlpha(alpha);
        }
        else
        {
            SetFadeAlpha(0f);
        }
    }

    private void UpdateBlackScreen(float currentTime)
    {
        bool isBlackSegment = currentTime >= _handoffBlackStartTime && currentTime < _handoffBlackEndTime;

        if (isBlackSegment)
        {
            HideAllVisuals();
            SetFadeAlpha(1f);
        }
    }

    private void DispatchEvents(float currentTime)
    {
        if (!_bootVisualsEventSent && currentTime >= _handoffBlackStartTime)
        {
            _bootVisualsEventSent = true;
            OnBootVisualsFinished?.Invoke();
        }

        if (!_introFinishedEventSent && currentTime >= _handoffBlackEndTime)
        {
            _introFinishedEventSent = true;
            OnIntroUIFinished?.Invoke();
        }
    }

    public void FadeInOut(float blackHoldDuration)
    {
        StartCoroutine(FadeInOutRoutine(blackHoldDuration));
    }

    public void FadeToBlack(float duration)
    {
        StartCoroutine(FadeTo(1f, duration));
    }

    public void FadeFromBlack(float duration)
    {
        StartCoroutine(FadeTo(0f, duration));
    }

    private IEnumerator FadeInOutRoutine(float blackHoldDuration)
    {
        yield return FadeTo(1f, _fadeDuration);
        yield return new WaitForSeconds(blackHoldDuration);
        yield return FadeTo(0f, _fadeDuration);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (_fadeImage == null)
        {
            yield break;
        }

        if (_fadeTween != null)
        {
            LeanTween.cancel(_fadeImage.gameObject);
            _fadeTween = null;
        }

        bool isComplete = false;
        float startAlpha = _fadeImage.color.a;

        _fadeTween = LeanTween.value(_fadeImage.gameObject, startAlpha, targetAlpha, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate(SetFadeAlpha)
            .setOnComplete(() =>
            {
                _fadeTween = null;
                isComplete = true;
            });

        while (!isComplete)
        {
            yield return null;
        }
    }

    private void HideAllVisuals()
    {
        if (_studioImage != null)
        {
            _studioImage.gameObject.SetActive(false);
        }

        if (_initializingText != null)
        {
            _initializingText.gameObject.SetActive(false);
        }

        if (_initializingBackground != null)
        {
            _initializingBackground.gameObject.SetActive(false);
        }
    }

    private void SetFadeAlpha(float alpha)
    {
        if (_fadeImage == null)
        {
            return;
        }

        Color color = _fadeImage.color;
        color.a = alpha;
        _fadeImage.color = color;
    }

    [Button("Stop Current Sequence")]
    public void StopCurrentSequence()
    {
        if (_bootSequenceCoroutine != null)
        {
            StopCoroutine(_bootSequenceCoroutine);
            _bootSequenceCoroutine = null;
        }

        if (_fadeImage != null)
        {
            LeanTween.cancel(_fadeImage.gameObject);
        }

        _fadeTween = null;
        HideAllVisuals();
        SetFadeAlpha(1f);
    }
}