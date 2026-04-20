using System;
using System.Collections;
using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class IntroManager : MonoBehaviour
{
    [Foldout("References"), SerializeField] private CinemachineCamera _cinemachineCamera;
    [Foldout("References"), SerializeField] private CinemachineThirdPersonFollow _thirdPersonFollow;
    [Foldout("References"), SerializeField] private IntroUI _introUI;
    [Foldout("References"), SerializeField] private AudioManager _audioManager;
    [Foldout("References"), SerializeField] private MetroGate _metroGate;

    [Foldout("Tracking Targets"), SerializeField] private Transform _introTrackingTarget;
    [Foldout("Tracking Targets"), SerializeField] private Transform _gameTrackingTarget;
    [Foldout("Tracking Targets"), SerializeField] private Transform _zombieLaughShotPoint;

    [Foldout("Zombie"), SerializeField] private Animator _zombieType3Animator;
    [Foldout("Zombie"), SerializeField] private string _zombieLaughTriggerName = "Laugh";

    [Foldout("Agents"), SerializeField] private NavMeshAgent _introNavMeshAgent;
    [Foldout("Agents"), SerializeField] private NavMeshAgent _gameNavMeshAgent;

    [Foldout("Player"), SerializeField] private Animator _playerAnimator;
    [Foldout("Player"), SerializeField] private string _speedParameterName = "Speed";

    [Foldout("Positions"), SerializeField] private Transform _introStartPosition;
    [Foldout("Positions"), SerializeField] private Transform _introEndPosition;
    [Foldout("Positions"), SerializeField] private Transform _playerStartPosition;
    [Foldout("Positions"), SerializeField] private Transform _playerEndPosition;

    [Foldout("Camera Intro"), SerializeField] private Vector3 _introShoulderOffset = new(0.5f, 1.5f, -0.5f);
    [Foldout("Camera Intro"), SerializeField] private float _introVerticalArmLength = 2f;
    [Foldout("Camera Intro"), SerializeField] private float _introCameraDistance = 3f;

    [Foldout("Camera Gameplay"), SerializeField] private Vector3 _gameShoulderOffset = new(0.5f, 1.5f, -0.5f);
    [Foldout("Camera Gameplay"), SerializeField] private float _gameVerticalArmLength = 2f;
    [Foldout("Camera Gameplay"), SerializeField] private float _gameCameraDistance = 3f;

    [Foldout("Camera Laugh"), SerializeField] private Vector3 _laughShoulderOffset = new(0.2f, 1.2f, -0.3f);
    [Foldout("Camera Laugh"), SerializeField] private float _laughVerticalArmLength = 1.4f;
    [Foldout("Camera Laugh"), SerializeField] private float _laughCameraDistance = 2f;

    [Foldout("Navigation"), SerializeField] private float _introNavigationSpeed = 3.5f;
    [Foldout("Navigation"), SerializeField] private float _introReturnNavigationSpeed = 5f;
    [Foldout("Navigation"), SerializeField] private float _gameNavMeshAgentSpeed = 3f;
    [Foldout("Navigation"), SerializeField] private float _arrivalThreshold = 0.15f;
    [Foldout("Navigation"), SerializeField] private float _postArrivalDelay = 0.5f;

    [Foldout("Timeline"), SerializeField] private float _blackout1Start = 19f;
    [Foldout("Timeline"), SerializeField] private float _blackout1Duration = 3f;

    [Foldout("Timeline"), SerializeField] private float _blackout2Start = 23f;
    [Foldout("Timeline"), SerializeField] private float _blackout2Duration = 2f;

    [Foldout("Timeline"), SerializeField] private float _blackout3Start = 27f;
    [Foldout("Timeline"), SerializeField] private float _blackout3Duration = 1f;

    [Foldout("Timeline"), SerializeField] private float _blackout4Start = 32f;
    [Foldout("Timeline"), SerializeField] private float _blackout4Duration = 3f;

    [Foldout("Timeline"), SerializeField] private float _laughStart = 36f;
    [Foldout("Timeline"), SerializeField] private float _laughDuration = 3f;

    [Foldout("Effects"), SerializeField] private float _fadeDuration = 0.3f;

    private Action _onIntroComplete;
    private bool _isIntroRunning;
    private bool _timelineEventsRunning;

    private void OnEnable()
    {
        if (_introUI != null)
        {
            _introUI.OnIntroUIFinished += HandleIntroUIFinished;
        }
    }

    private void OnDisable()
    {
        if (_introUI != null)
        {
            _introUI.OnIntroUIFinished -= HandleIntroUIFinished;
        }
    }

    private void Start()
    {
        CacheGameplayCameraSettings();
    }

    private bool ValidateReferences()
    {
        return ValidatorReferences.Validate(this,
            (_cinemachineCamera, nameof(_cinemachineCamera)),
            (_thirdPersonFollow, nameof(_thirdPersonFollow)),
            (_introUI, nameof(_introUI)),
            (_audioManager, nameof(_audioManager)),
            (_introTrackingTarget, nameof(_introTrackingTarget)),
            (_gameTrackingTarget, nameof(_gameTrackingTarget)),
            (_zombieLaughShotPoint, nameof(_zombieLaughShotPoint)),
            (_zombieType3Animator, nameof(_zombieType3Animator)),
            (_introNavMeshAgent, nameof(_introNavMeshAgent)),
            (_gameNavMeshAgent, nameof(_gameNavMeshAgent)),
            (_playerAnimator, nameof(_playerAnimator)),
            (_introStartPosition, nameof(_introStartPosition)),
            (_introEndPosition, nameof(_introEndPosition)),
            (_playerStartPosition, nameof(_playerStartPosition))
        );
    }

    private void CacheGameplayCameraSettings()
    {
        if (_thirdPersonFollow == null)
        {
            Debug.LogError("[IntroManager] CinemachineThirdPersonFollow is not assigned.", this);
            return;
        }

        _gameCameraDistance = _thirdPersonFollow.CameraDistance;
        _gameShoulderOffset = _thirdPersonFollow.ShoulderOffset;
        _gameVerticalArmLength = _thirdPersonFollow.VerticalArmLength;
    }

    private void HandleIntroUIFinished()
    {
        if (_isIntroRunning)
        {
            return;
        }

        StartCoroutine(RunIntroSequence());
    }

    private IEnumerator RunIntroSequence()
    {
        if (!ValidateReferences())
        {
            yield break;
        }

        _isIntroRunning = true;
        _timelineEventsRunning = true;

        SetupIntroCamera();
        PrepareIntroActor();
        PrepareGameplayAgent();

        if (_metroGate != null)
        {
            _metroGate.OpenGate();
        }
        StartCoroutine(FadeFromBlack(_fadeDuration));
        StartCoroutine(RunTimelineEvents());

        yield return MoveAgentTo(_introNavMeshAgent, _introStartPosition.position, _introNavigationSpeed, false);
        yield return MoveAgentTo(_introNavMeshAgent, _introEndPosition.position, _introNavigationSpeed, false);
        yield return MoveAgentTo(_introNavMeshAgent, _playerEndPosition.position, _introReturnNavigationSpeed, false);
        yield return MoveAgentTo(_gameNavMeshAgent, _playerEndPosition.position, _gameNavMeshAgentSpeed, true);

        _timelineEventsRunning = false;

        yield return new WaitForSeconds(_postArrivalDelay);

        TransitionToGameplay();

        _isIntroRunning = false;
        Debug.Log("[IntroManager] Intro sequence completed, transitioned to gameplay.", this);
    }

    private IEnumerator RunTimelineEvents()
    {
        yield return WaitUntilAudioTime(_blackout1Start);
        if (_timelineEventsRunning) yield return PlayBlackoutOnly(_blackout1Duration);

        yield return WaitUntilAudioTime(_blackout2Start);
        if (_timelineEventsRunning) yield return PlayBlackoutOnly(_blackout2Duration);

        yield return WaitUntilAudioTime(_blackout3Start);
        if (_timelineEventsRunning) yield return PlayBlackoutOnly(_blackout3Duration);

        yield return WaitUntilAudioTime(_blackout4Start);
        if (_timelineEventsRunning) yield return PlayBlackoutOnly(_blackout4Duration);

        yield return WaitUntilAudioTime(_laughStart);
        if (_timelineEventsRunning) yield return PlayLaughShot(_laughDuration);
    }

    private void SetupIntroCamera()
    {
        ApplyCameraSettings(_introShoulderOffset, _introVerticalArmLength, _introCameraDistance);
        Focus(_introTrackingTarget);
    }

    private void RestoreGameplayCamera()
    {
        ApplyCameraSettings(_gameShoulderOffset, _gameVerticalArmLength, _gameCameraDistance);
        Focus(_gameTrackingTarget);
    }

    private void SetupLaughCamera()
    {
        ApplyCameraSettings(_laughShoulderOffset, _laughVerticalArmLength, _laughCameraDistance);
        Focus(_zombieLaughShotPoint);
    }

    private void ApplyCameraSettings(Vector3 shoulderOffset, float verticalArmLength, float cameraDistance)
    {
        _thirdPersonFollow.ShoulderOffset = shoulderOffset;
        _thirdPersonFollow.VerticalArmLength = verticalArmLength;
        _thirdPersonFollow.CameraDistance = cameraDistance;
    }

    private void Focus(Transform target)
    {
        _cinemachineCamera.Follow = target;
        _cinemachineCamera.LookAt = target;
    }

    private IEnumerator FadeFromBlack(float duration)
    {
        _introUI.FadeFromBlack(_fadeDuration);
        yield return new WaitForSeconds(_fadeDuration);
    }

    private IEnumerator PlayBlackoutOnly(float duration)
    {
        _introUI.FadeToBlack(_fadeDuration);
        yield return new WaitForSeconds(_fadeDuration);

        yield return new WaitForSeconds(duration);

        _introUI.FadeFromBlack(_fadeDuration);
        yield return new WaitForSeconds(_fadeDuration);
    }

    private IEnumerator PlayLaughShot(float duration)
    {
        _introUI.FadeToBlack(_fadeDuration);
        yield return new WaitForSeconds(_fadeDuration);

        _zombieType3Animator.SetTrigger(_zombieLaughTriggerName);
        SetupLaughCamera();

        _introUI.FadeFromBlack(_fadeDuration);
        yield return new WaitForSeconds(duration);

        _introUI.FadeToBlack(_fadeDuration);
        yield return new WaitForSeconds(_fadeDuration);

        SetupIntroCamera();

        _introUI.FadeFromBlack(_fadeDuration);
        yield return new WaitForSeconds(_fadeDuration);
    }

    private void PrepareIntroActor()
    {
        _introNavMeshAgent.enabled = true;
        _introNavMeshAgent.isStopped = false;
        _introNavMeshAgent.ResetPath();

        if (_introNavMeshAgent.isOnNavMesh)
        {
            _introNavMeshAgent.Warp(_introStartPosition.position);
        }
    }

    private void PrepareGameplayAgent()
    {
        _gameNavMeshAgent.enabled = true;
        _gameNavMeshAgent.isStopped = false;
        _gameNavMeshAgent.ResetPath();

        if (_gameNavMeshAgent.isOnNavMesh)
        {
            _gameNavMeshAgent.Warp(_playerStartPosition.position);
        }

        _playerAnimator.SetFloat(_speedParameterName, 0f);
    }

    private IEnumerator MoveAgentTo(NavMeshAgent agent, Vector3 destination, float speed, bool updateAnimator)
    {
        if (agent == null)
        {
            yield break;
        }

        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"[IntroManager] Agent {agent.name} is not on a NavMesh.", this);
            yield break;
        }

        agent.speed = speed;
        agent.isStopped = false;
        agent.ResetPath();
        agent.SetDestination(destination);

        while (true)
        {
            if (updateAnimator && _playerAnimator != null)
            {
                _playerAnimator.SetFloat(_speedParameterName, agent.velocity.magnitude);
            }

            if (!agent.pathPending)
            {
                bool reachedDestination =
                    agent.remainingDistance <= agent.stoppingDistance + _arrivalThreshold;

                bool stoppedMoving =
                    !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f;

                if (reachedDestination && stoppedMoving)
                {
                    break;
                }
            }

            yield return null;
        }

        if (updateAnimator && _playerAnimator != null)
        {
            _playerAnimator.SetFloat(_speedParameterName, 0f);
        }
    }

    private IEnumerator WaitUntilAudioTime(float targetTime)
    {
        while (_audioManager.IsIntroPlaying() && _audioManager.GetIntroTime() < targetTime)
        {
            yield return null;
        }
    }

    private void TransitionToGameplay()
    {
        if (_introNavMeshAgent != null && _introNavMeshAgent.enabled)
        {
            _introNavMeshAgent.isStopped = true;
            _introNavMeshAgent.ResetPath();
            _introNavMeshAgent.enabled = false;
        }

        if (_gameNavMeshAgent != null && _gameNavMeshAgent.enabled)
        {
            _gameNavMeshAgent.isStopped = true;
            _gameNavMeshAgent.ResetPath();
            _gameNavMeshAgent.transform.LookAt(_introEndPosition);
        }

        if (_playerAnimator != null)
        {
            _playerAnimator.SetFloat(_speedParameterName, 0f);
        }

        RestoreGameplayCamera();
        _onIntroComplete?.Invoke();
    }

    public void RegisterOnIntroComplete(Action callback)
    {
        _onIntroComplete += callback;
    }

    public void UnregisterOnIntroComplete(Action callback)
    {
        _onIntroComplete -= callback;
    }

    public bool IsIntroRunning()
    {
        return _isIntroRunning;
    }
}