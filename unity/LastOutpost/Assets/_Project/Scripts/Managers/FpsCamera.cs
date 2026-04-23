using NaughtyAttributes;

using Unity.Cinemachine;

using UnityEngine;

public class FpsCamera : MonoBehaviour

{

    [Foldout("References"), SerializeField] private Transform _playerRoot;

    [Foldout("References"), SerializeField] private Transform _pitchRoot;

    [Foldout("References"), SerializeField] private CinemachineThirdPersonFollow _thirdPersonFollow;

    [Foldout("References"), SerializeField] private IntroManager _introManager;

    [Foldout("Look"), SerializeField] private float _mouseSensitivity = 120f;

    [Foldout("Look"), SerializeField] private float _minPitch = -35f;

    [Foldout("Look"), SerializeField] private float _maxPitch = 60f;

    [Foldout("Normal Camera"), SerializeField] private Vector3 _normalShoulderOffset = new(0.5f, 1.5f, -0.5f);

    [Foldout("Normal Camera"), SerializeField] private float _normalVerticalArmLength = 2f;

    [Foldout("Normal Camera"), SerializeField] private float _normalCameraDistance = 3f;

    [Foldout("Aim Camera"), SerializeField] private Vector3 _aimShoulderOffset = new(0.15f, 1.45f, 0f);

    [Foldout("Aim Camera"), SerializeField] private float _aimVerticalArmLength = 1.4f;

    [Foldout("Aim Camera"), SerializeField] private float _aimCameraDistance = 0.6f;

    [Foldout("Tween"), SerializeField] private float _cameraTweenDuration = 0.2f;

    [Foldout("UI"), SerializeField, ReadOnly] private KeyCode _toggleMenuKey = KeyCode.Tab;

    private float _yaw;

    private float _pitch;

    private bool _isAiming;

    private bool _inputEnabled;

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

        _inputEnabled = false;

        ApplyCameraInstant(_normalShoulderOffset, _normalVerticalArmLength, _normalCameraDistance);

        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;

    }

    private void Update()

    {
        HanldeMenuToggleInput();
        if (!_inputEnabled || _isMenuOpen)

        {

            return;

        }

        HandleAimInput();

        HandleMouseLook();

    }

    private bool ValidateReferences()

    {

        return ValidatorReferences.Validate(this,

            (_playerRoot, nameof(_playerRoot)),

            (_pitchRoot, nameof(_pitchRoot)),

            (_thirdPersonFollow, nameof(_thirdPersonFollow)),

            (_introManager, nameof(_introManager))

        );

    }

    private void HanldeMenuToggleInput()
    {
        if (Input.GetKeyDown(_toggleMenuKey))
        {
            if (_isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
    }

    private void OpenMenu()
    {
        _isMenuOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        DisableInput();
    }

    private void CloseMenu()
    {
        _isMenuOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void HandleAimInput()

    {

        if (Input.GetMouseButtonDown(1))

        {

            _isAiming = true;

            TweenCamera(

                _aimShoulderOffset,

                _aimVerticalArmLength,

                _aimCameraDistance

            );

        }

        if (Input.GetMouseButtonUp(1))

        {

            _isAiming = false;

            TweenCamera(

                _normalShoulderOffset,

                _normalVerticalArmLength,

                _normalCameraDistance

            );

        }

    }

    private void HandleMouseLook()

    {

        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;

        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        _yaw += mouseX;

        _pitch -= mouseY;

        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        _playerRoot.rotation = Quaternion.Euler(0f, _yaw, 0f);

        //_pitchRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

    }

    private void TweenCamera(Vector3 targetShoulderOffset, float targetVerticalArmLength, float targetCameraDistance)

    {

        CancelCameraTween();

        Vector3 startShoulderOffset = _thirdPersonFollow.ShoulderOffset;

        float startVerticalArmLength = _thirdPersonFollow.VerticalArmLength;

        float startCameraDistance = _thirdPersonFollow.CameraDistance;

        _cameraTween = LeanTween.value(gameObject, 0f, 1f, _cameraTweenDuration)

            .setEase(LeanTweenType.easeInOutQuad)

            .setOnUpdate((float t) =>

            {

                _thirdPersonFollow.ShoulderOffset = Vector3.Lerp(startShoulderOffset, targetShoulderOffset, t);

                _thirdPersonFollow.VerticalArmLength = Mathf.Lerp(startVerticalArmLength, targetVerticalArmLength, t);

                _thirdPersonFollow.CameraDistance = Mathf.Lerp(startCameraDistance, targetCameraDistance, t);

            })

            .setOnComplete(() => _cameraTween = null);

    }

    private void ApplyCameraInstant(Vector3 shoulderOffset, float verticalArmLength, float cameraDistance)

    {

        _thirdPersonFollow.ShoulderOffset = shoulderOffset;

        _thirdPersonFollow.VerticalArmLength = verticalArmLength;

        _thirdPersonFollow.CameraDistance = cameraDistance;

    }

    private void CancelCameraTween()

    {

        if (_cameraTween != null)

        {

            LeanTween.cancel(gameObject);

            _cameraTween = null;

        }

    }

    public void EnableInput()

    {

        _inputEnabled = true;

    }

    public void DisableInput()

    {

        _inputEnabled = false;

        _isAiming = false;

        CancelCameraTween();

        ApplyCameraInstant(_normalShoulderOffset, _normalVerticalArmLength, _normalCameraDistance);

    }

    public bool IsAiming()

    {

        return _isAiming;

    }

    public bool IsInputEnabled()
    {
        return _inputEnabled;
    }

}
