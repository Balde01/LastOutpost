using UnityEngine;
using UnityEngine.AI;
using NaughtyAttributes;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Foldout("References"), SerializeField] private Transform _cameraTransform;
    [Foldout("References"), SerializeField] private Animator _animator;
    [Foldout("References"), SerializeField] private IntroManager _introManager;
    [Foldout("References"), SerializeField] private FpsCamera _fpsCamera;
    [Foldout("References"), SerializeField] private NavMeshAgent _navMeshAgent;

    [Foldout("Movement"), SerializeField] private float _moveSpeed = 4f;
    [Foldout("Movement"), SerializeField] private float _rotationSpeed = 10f;
    [Foldout("Movement"), SerializeField] private string _speedParameterName = "Speed";

    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;
    private bool _playerMovementEnabled;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerMovementEnabled = false;
    }

    private void OnEnable()
    {
        if (_introManager != null)
        {
            _introManager.RegisterOnIntroComplete(EnablePlayerMovement);
        }
    }

    private void OnDisable()
    {
        if (_introManager != null)
        {
            _introManager.UnregisterOnIntroComplete(EnablePlayerMovement);
        }
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        PrepareForIntro();
    }

    private bool ValidateReferences()
    {
        return ValidatorReferences.Validate(this,
            (_rigidbody, nameof(_rigidbody)),
            (_cameraTransform, nameof(_cameraTransform)),
            (_animator, nameof(_animator)),
            (_introManager, nameof(_introManager)),
            (_fpsCamera, nameof(_fpsCamera)),
            (_navMeshAgent, nameof(_navMeshAgent))
        );
    }

    private void Update()
    {
        if (!_playerMovementEnabled)
        {
            _moveDirection = Vector3.zero;
            UpdateAnimatorSpeed(0f);
            return;
        }

        ReadMovementInput();
    }

    private void FixedUpdate()
    {
        if (!_playerMovementEnabled)
        {
            StopRigidbodyMovement();
            return;
        }

        MovePlayer();
        RotatePlayer();
    }

    private void ReadMovementInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
        else if (Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;

        if (Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
        else if (Input.GetKey(KeyCode.DownArrow)) vertical = -1f;

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.sqrMagnitude <= 0.01f)
        {
            _moveDirection = Vector3.zero;
            UpdateAnimatorSpeed(0f);
            return;
        }

        Vector3 cameraForward = _cameraTransform.forward;
        Vector3 cameraRight = _cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        _moveDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

        UpdateAnimatorSpeed(1f);
    }

    private void MovePlayer()
    {
        if (_moveDirection.sqrMagnitude <= 0.01f)
        {
            StopHorizontalMovement();
            return;
        }

        Vector3 targetPosition = _rigidbody.position + _moveDirection * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(targetPosition);
    }

    private void RotatePlayer()
    {
        if (_moveDirection.sqrMagnitude <= 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
        Quaternion smoothRotation = Quaternion.Slerp(
            _rigidbody.rotation,
            targetRotation,
            _rotationSpeed * Time.fixedDeltaTime
        );

        _rigidbody.MoveRotation(smoothRotation);
    }

    private void PrepareForIntro()
    {
        _playerMovementEnabled = false;

        _rigidbody.isKinematic = true;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        if (_navMeshAgent != null)
        {
            _navMeshAgent.enabled = true;
        }

        if (_fpsCamera != null)
        {
            _fpsCamera.DisableInput();
        }

        UpdateAnimatorSpeed(0f);
    }

    public void EnablePlayerMovement()
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();
            _navMeshAgent.enabled = false;
        }

        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        _playerMovementEnabled = true;

        if (_fpsCamera != null)
        {
            _fpsCamera.EnableInput();
        }
    }

    public void DisablePlayerMovement()
    {
        _playerMovementEnabled = false;
        _moveDirection = Vector3.zero;

        StopRigidbodyMovement();
        UpdateAnimatorSpeed(0f);

        if (_fpsCamera != null)
        {
            _fpsCamera.DisableInput();
        }
    }

    private void StopHorizontalMovement()
    {
        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.x = 0f;
        velocity.z = 0f;
        _rigidbody.linearVelocity = velocity;
    }

    private void StopRigidbodyMovement()
    {
        if (_rigidbody == null || _rigidbody.isKinematic)
        {
            return;
        }

        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.x = 0f;
        velocity.z = 0f;
        _rigidbody.linearVelocity = velocity;
    }

    private void UpdateAnimatorSpeed(float value)
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetFloat(_speedParameterName, value);
    }
}