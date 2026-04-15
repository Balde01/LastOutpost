using System;
using System.Collections;
using Unity.AI.Navigation;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class IntroManager : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private CinemachineThirdPersonFollow _thirdPersonFollow;

    [Header("Tracking Targets")]
    [SerializeField] private Transform _introTrackingTarget;
    [SerializeField] private Transform _gameTrackingTarget;

    [Header("Intro Camera Settings")]
    [SerializeField] private Vector3 _introShoulderOffset = new(0.5f, 1.5f, -0.5f);
    [SerializeField] private float _introVerticalArmLength = 2f;
    [SerializeField] private float _introCameraDistance = 3f;

    [Header("Gameplay Camera Settings")]
    [SerializeField] private Vector3 _gameShoulderOffset = new(0.5f, 1.5f, -0.5f);
    [SerializeField] private float _gameVerticalArmLength = 2f;
    [SerializeField] private float _gameCameraDistance = 3f;

    [Header("Scene Systems")]
    [SerializeField] private MetroGate _metroGate;

    [Header("Agents")]
    [SerializeField] private NavMeshAgent _introNavMeshAgent;
    [SerializeField] private NavMeshAgent _gameNavMeshAgent;

    [Header("Player Settings")]
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private string _speedParameterName = "Speed";

    [Header("Positions")]
    [SerializeField] private Transform _introStartPosition;
    [SerializeField] private Transform _introEndPosition;
    [SerializeField] private Transform _playerStartPosition;

    [Header("Intro Timing")]
    [SerializeField] private float _introNavigationSpeed = 3.5f;
    [SerializeField] private float _introReturnNavigationSpeed = 5f;
    [SerializeField] private float _gameNavMeshAgentSpeed = 3f;
    [SerializeField] private float _arrivalThreshold = 0.15f;
    [SerializeField] private float _postArrivalDelay = 0.5f;

    private Action _onIntroComplete;
    private bool _isIntroRunning;

    private void Start()
    {
        CacheGameplayCameraSettings();
        StartCoroutine(RunIntroSequence());
    }

    private void CacheGameplayCameraSettings()
    {
        if (_thirdPersonFollow == null)
        {
            Debug.LogError("[IntroManager] CinemachineThirdPersonFollow is not assigned.");
            return;
        }

        _gameCameraDistance = _thirdPersonFollow.CameraDistance;
        _gameShoulderOffset = _thirdPersonFollow.ShoulderOffset;
        _gameVerticalArmLength = _thirdPersonFollow.VerticalArmLength;
    }

    private IEnumerator RunIntroSequence()
    {
        if (!ValidateReferences())
        {
            yield break;
        }

        _isIntroRunning = true;

        SetupIntroCamera();
        PrepareIntroActor();
        PrepareGameplayAgent();

        if (_metroGate != null)
        {
            _metroGate.OpenGate();
        }

        yield return MoveAgentTo(_introNavMeshAgent, _introStartPosition.position, _introNavigationSpeed, false);
        yield return MoveAgentTo(_introNavMeshAgent, _introEndPosition.position, _introNavigationSpeed, false);
        yield return MoveAgentTo(_introNavMeshAgent, _introStartPosition.position, _introReturnNavigationSpeed, false);
        yield return MoveAgentTo(_gameNavMeshAgent, _introStartPosition.position, _gameNavMeshAgentSpeed, true);

        yield return new WaitForSeconds(_postArrivalDelay);

        TransitionToGameplay();

        _isIntroRunning = false;
        Debug.Log("[IntroManager] Intro sequence completed, transitioned to gameplay.");
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (_cinemachineCamera == null)
        {
            Debug.LogError("[IntroManager] CinemachineCamera is not assigned.");
            isValid = false;
        }

        if (_thirdPersonFollow == null)
        {
            Debug.LogError("[IntroManager] CinemachineThirdPersonFollow is not assigned.");
            isValid = false;
        }

        if (_introTrackingTarget == null)
        {
            Debug.LogError("[IntroManager] Intro tracking target is not assigned.");
            isValid = false;
        }

        if (_gameTrackingTarget == null)
        {
            Debug.LogError("[IntroManager] Game tracking target is not assigned.");
            isValid = false;
        }

        if (_introNavMeshAgent == null)
        {
            Debug.LogError("[IntroManager] Intro NavMeshAgent is not assigned.");
            isValid = false;
        }

        if (_gameNavMeshAgent == null)
        {
            Debug.LogError("[IntroManager] Game NavMeshAgent is not assigned.");
            isValid = false;
        }

        if (_introStartPosition == null)
        {
            Debug.LogError("[IntroManager] Intro start position is not assigned.");
            isValid = false;
        }

        if (_introEndPosition == null)
        {
            Debug.LogError("[IntroManager] Intro end position is not assigned.");
            isValid = false;
        }

        if (_playerStartPosition == null)
        {
            Debug.LogError("[IntroManager] Player start position is not assigned.");
            isValid = false;
        }

        if (_playerAnimator == null)
        {
            Debug.LogError("[IntroManager] Player Animator is not assigned.");
            isValid = false;
        }

        return isValid;
    }

    private void SetupIntroCamera()
    {
        _thirdPersonFollow.ShoulderOffset = _introShoulderOffset;
        _thirdPersonFollow.VerticalArmLength = _introVerticalArmLength;
        _thirdPersonFollow.CameraDistance = _introCameraDistance;

        _cinemachineCamera.Follow = _introTrackingTarget;
        _cinemachineCamera.LookAt = _introTrackingTarget;
    }

    private void RestoreGameplayCamera()
    {
        _thirdPersonFollow.ShoulderOffset = _gameShoulderOffset;
        _thirdPersonFollow.VerticalArmLength = _gameVerticalArmLength;
        _thirdPersonFollow.CameraDistance = _gameCameraDistance;

        _cinemachineCamera.Follow = _gameTrackingTarget;
        _cinemachineCamera.LookAt = _gameTrackingTarget;
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
            Debug.LogError($"[IntroManager] Agent {agent.name} is not on a NavMesh.");
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