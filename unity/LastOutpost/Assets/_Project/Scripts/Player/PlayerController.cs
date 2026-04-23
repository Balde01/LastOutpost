using NaughtyAttributes;

using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour

{

    [Foldout("References"), SerializeField] private Transform _cameraTransform;

    [Foldout("References"), SerializeField] private Animator _animator;

    [Foldout("References"), SerializeField] private IntroManager _introManager;
    [Foldout("References"), SerializeField] private FpsCamera _fpsCamera;

    [Foldout("Movement"), SerializeField] private float _moveSpeed = 4f;

    [Foldout("Movement"), SerializeField] private float _rotationSpeed = 10f;

    [Foldout("Movement"), SerializeField] private string _speedParameterName = "Speed";

    [Foldout("Gravity"), SerializeField] private float _gravity = -9.81f;

    private CharacterController _characterController;

    private Vector3 _velocity;

    private bool _playerMovementEnabled;

    private void Awake()

    {

        _characterController = GetComponent<CharacterController>();

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

        DisablePlayerMovement();

    }

    private void Update()

    {

        if (!_playerMovementEnabled || _fpsCamera.IsInputEnabled())

        {

            //UpdateAnimatorSpeed(0f);

            //ApplyGravity();

            return;

        }

        HandleMovement();

        ApplyGravity();

    }

    private bool ValidateReferences()

    {

        return ValidatorReferences.Validate(this,

            (_characterController, nameof(_characterController)),

            (_cameraTransform, nameof(_cameraTransform)),

            (_animator, nameof(_animator)),

            (_introManager, nameof(_introManager))

        );

    }

    private void HandleMovement()

    {

        float horizontal = 0f;

        float vertical = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))

        {

            horizontal = -1f;

        }

        else if (Input.GetKey(KeyCode.RightArrow))

        {

            horizontal = 1f;

        }

        if (Input.GetKey(KeyCode.UpArrow))

        {

            vertical = 1f;

        }

        else if (Input.GetKey(KeyCode.DownArrow))

        {

            vertical = -1f;

        }

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.sqrMagnitude <= 0.01f)

        {

            UpdateAnimatorSpeed(0f);

            return;

        }

        Vector3 cameraForward = _cameraTransform.forward;

        Vector3 cameraRight = _cameraTransform.right;

        cameraForward.y = 0f;

        cameraRight.y = 0f;

        cameraForward.Normalize();

        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

        _characterController.Move(moveDirection * _moveSpeed * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);

        UpdateAnimatorSpeed(1f);

    }

    private void ApplyGravity()

    {

        if (_characterController.isGrounded && _velocity.y < 0f)

        {

            _velocity.y = -2f;

        }

        _velocity.y += _gravity * Time.deltaTime;

        _characterController.Move(_velocity * Time.deltaTime);

    }

    private void UpdateAnimatorSpeed(float value)

    {

        if (_animator == null)

        {

            return;

        }

        _animator.SetFloat(_speedParameterName, value);

    }

    public void EnablePlayerMovement()

    {
        _characterController.enabled = true;
        _playerMovementEnabled = true;

    }

    public void DisablePlayerMovement()

    {

        _playerMovementEnabled = false;
        _characterController.enabled = false;

        //UpdateAnimatorSpeed(0f);

    }

}
