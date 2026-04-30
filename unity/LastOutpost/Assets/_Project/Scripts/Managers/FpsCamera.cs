using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;

public class FpsCamera : MonoBehaviour
{
    [Foldout("References"), SerializeField] private Transform _playerRoot;
    [Foldout("References"), SerializeField] private Animator _animator;
    [Foldout("References"), SerializeField] private CinemachineCamera _cinemachineCamera;
    [Foldout("References"), SerializeField] private CinemachineThirdPersonFollow _thirdPersonFollow;
    [Foldout("References"), SerializeField] private IntroManager _introManager;

    [Foldout("Look - Horizontal"), SerializeField] private float _mouseSensitivityX = 120f;

    [Foldout("Look - Up Down Animator"), SerializeField] private string _upDownRatioParameterName = "UpDownRatio";
    [Foldout("Look - Up Down Animator"), SerializeField] private float _scrollSensitivity = 0.25f;
    [Foldout("Look - Up Down Animator"), SerializeField] private float _minUpDownRatio = 0f;
    [Foldout("Look - Up Down Animator"), SerializeField] private float _neutralUpDownRatio = 0.4f;
    [Foldout("Look - Up Down Animator"), SerializeField] private float _maxUpDownRatio = 1f;
    [Foldout("Look - Up Down Animator"), SerializeField] private float _animatorDampTime = 0.08f;

    [Foldout("Normal Camera"), SerializeField] private Vector3 _normalShoulderOffset = new(-1.14f, -0.5f, -0.71f);
    [Foldout("Normal Camera"), SerializeField] private float _normalVerticalArmLength = 1f;
    [Foldout("Normal Camera"), SerializeField] private float _normalCameraDistance = 0.9f;
    [Foldout("Normal Camera"), SerializeField] private float _normalFieldOfView = 60f;

    [Foldout("Aim Camera"), SerializeField] private Vector3 _aimShoulderOffset = new(-1.14f, -0.5f, -0.71f);
    [Foldout("Aim Camera"), SerializeField] private float _aimVerticalArmLength = 1f;
    [Foldout("Aim Camera"), SerializeField] private float _aimCameraDistance = 0.75f;
    [Foldout("Aim Camera"), SerializeField] private float _aimFieldOfView = 38f;

    [Foldout("Tween"), SerializeField] private float _cameraTweenDuration = 0.2f;

    private float _yaw;
    private float _upDownRatio;

    private bool _inputEnabled;
    private bool _isAiming;
    private bool _isMenuOpen;

    private LTDescr _cameraTween;

    private void OnEnable()
    {
        if (_introManager != null)
        {
            _introManager.RegisterOnIntroComplete(EnableInput);
        }
    }

    private void OnDisable()
    {
        if (_introManager != null)
        {
            _introManager.UnregisterOnIntroComplete(EnableInput);
        }

        CancelCameraTween();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        _yaw = _playerRoot.eulerAngles.y;
        _upDownRatio = _neutralUpDownRatio;

        ApplyCameraInstant(
            _normalShoulderOffset,
            _normalVerticalArmLength,
            _normalCameraDistance,
            _normalFieldOfView
        );

        SetAnimatorUpDownRatio(_upDownRatio);

        DisableInput();
    }

    private void Update()
    {
        if (!_inputEnabled || _isMenuOpen)
        {
            return;
        }

        HandleAimInput();
        HandleLookInput();
    }

    private bool ValidateReferences()
    {
        return ValidatorReferences.Validate(this,
            (_playerRoot, nameof(_playerRoot)),
            (_animator, nameof(_animator)),
            (_cinemachineCamera, nameof(_cinemachineCamera)),
            (_thirdPersonFollow, nameof(_thirdPersonFollow)),
            (_introManager, nameof(_introManager))
        );
    }

    private void HandleAimInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _isAiming = true;

            TweenCamera(
                _aimShoulderOffset,
                _aimVerticalArmLength,
                _aimCameraDistance,
                _aimFieldOfView
            );
        }

        if (Input.GetMouseButtonUp(1))
        {
            _isAiming = false;

            TweenCamera(
                _normalShoulderOffset,
                _normalVerticalArmLength,
                _normalCameraDistance,
                _normalFieldOfView
            );
        }
    }

    private void HandleLookInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivityX * Time.deltaTime;
        float scroll = Input.mouseScrollDelta.y;

        RotatePlayerHorizontally(mouseX);
        UpdateUpDownRatioWithScroll(scroll);
    }

    private void RotatePlayerHorizontally(float mouseX)
    {
        _yaw += mouseX;
        _playerRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);
    }

    private void UpdateUpDownRatioWithScroll(float scroll)
    {
        if (Mathf.Abs(scroll) <= 0.01f)
        {
            return;
        }

        _upDownRatio += scroll * _scrollSensitivity;
        _upDownRatio = Mathf.Clamp(_upDownRatio, _minUpDownRatio, _maxUpDownRatio);

        SetAnimatorUpDownRatio(_upDownRatio);
    }

    private void SetAnimatorUpDownRatio(float value)
    {
        if (_animator == null)
        {
            return;
        }

        _animator.SetFloat(
            _upDownRatioParameterName,
            value,
            _animatorDampTime,
            Time.deltaTime
        );
    }

    private void TweenCamera(
        Vector3 targetShoulderOffset,
        float targetVerticalArmLength,
        float targetCameraDistance,
        float targetFieldOfView
    )
    {
        CancelCameraTween();

        Vector3 startShoulderOffset = _thirdPersonFollow.ShoulderOffset;
        float startVerticalArmLength = _thirdPersonFollow.VerticalArmLength;
        float startCameraDistance = _thirdPersonFollow.CameraDistance;
        float startFieldOfView = _cinemachineCamera.Lens.FieldOfView;

        _cameraTween = LeanTween.value(gameObject, 0f, 1f, _cameraTweenDuration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float t) =>
            {
                _thirdPersonFollow.ShoulderOffset =
                    Vector3.Lerp(startShoulderOffset, targetShoulderOffset, t);

                _thirdPersonFollow.VerticalArmLength =
                    Mathf.Lerp(startVerticalArmLength, targetVerticalArmLength, t);

                _thirdPersonFollow.CameraDistance =
                    Mathf.Lerp(startCameraDistance, targetCameraDistance, t);

                _cinemachineCamera.Lens.FieldOfView =
                    Mathf.Lerp(startFieldOfView, targetFieldOfView, t);
            })
            .setOnComplete(() => _cameraTween = null);
    }

    private void ApplyCameraInstant(
        Vector3 shoulderOffset,
        float verticalArmLength,
        float cameraDistance,
        float fieldOfView
    )
    {
        _thirdPersonFollow.ShoulderOffset = shoulderOffset;
        _thirdPersonFollow.VerticalArmLength = verticalArmLength;
        _thirdPersonFollow.CameraDistance = cameraDistance;
        _cinemachineCamera.Lens.FieldOfView = fieldOfView;
    }

    private void CancelCameraTween()
    {
        if (_cameraTween == null)
        {
            return;
        }

        LeanTween.cancel(gameObject);
        _cameraTween = null;
    }

    public void EnableInput()
    {
        _inputEnabled = true;
        LockCursor();
    }

    public void DisableInput()
    {
        _inputEnabled = false;
        _isAiming = false;

        CancelCameraTween();

        ApplyCameraInstant(
            _normalShoulderOffset,
            _normalVerticalArmLength,
            _normalCameraDistance,
            _normalFieldOfView
        );

        UnlockCursor();
    }

    public void OpenMenuMode()
    {
        _isMenuOpen = true;
        UnlockCursor();
    }

    public void CloseMenuMode()
    {
        _isMenuOpen = false;

        if (_inputEnabled)
        {
            LockCursor();
        }
    }

    public void SetMenuOpen(bool isOpen)
    {
        if (isOpen)
        {
            OpenMenuMode();
        }
        else
        {
            CloseMenuMode();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public bool IsAiming()
    {
        return _isAiming;
    }

    public bool IsMenuOpen()
    {
        return _isMenuOpen;
    }

    public float GetUpDownRatio()
    {
        return _upDownRatio;
    }
}