using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;

public enum PlayerStance
{
    Stand,
    Crouch,
    Climb,
    Glide
}

public class PlayerTraversal : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private InputManager _input;
    [SerializeField]
    private CameraManager _cameraManager;
    [SerializeField]
    private HUDManager _hudManager;
    [SerializeField]
    private Transform _cameraTransform;
    [SerializeField]
    private Transform _fpsTarget;


    [Header("Movement")]
    [SerializeField]
    private float _crouchSpeed;
    [SerializeField]
    private float _climbSpeed;
    [SerializeField]
    private float _walkSpeed;
    [SerializeField]
    private float _runSpeed;
    [SerializeField]
    private float _glideSpeed;
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private float _rotationSmoothTime = 0.1f;
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private LayerMask _climableLayer;
    [SerializeField]
    private Transform _stepChecker;
    [SerializeField]
    private float _stepOffset;
    [SerializeField]
    private Transform _groundChecker;
    [SerializeField]
    private float _groundCheckerRadius;
    [SerializeField]
    private Transform _climbChecker;
    [SerializeField]
    private float _ClimbCheckDistance;
    [SerializeField]
    private Vector3 _climbOffset;
    [SerializeField]
    private Vector3 _glideRotationSpeed;
    [SerializeField]
    private float _airDrag;
    [SerializeField]
    private Vector3 _minGlideRotation;
    [SerializeField]
    private Vector3 _maxGlideRotation;

    private Rigidbody _rigidbody;
    private Animator _animator;
    private CapsuleCollider _collider;

    private PlayerStance _playerStance;
    private float _speed;
    private float _rotationSmoothVelocity;
    private bool _isGrounded = true;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();

        _playerStance = PlayerStance.Stand;
        _speed = _walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        _input.OnMoveInput += Move;
        _input.OnSprintInput += Sprint;
        _input.OnCrouchInput += Crouch;
        _input.OnJumpInput += Jump;
        _input.OnClimbInput += StartClimb;
        _input.OnCancelClimb += CancelClimb;
        _input.OnGlideInput += StartGlide;
        _input.OnCancelGlide += CancelGlide;
    }
    private void Update()
    {
        CheckIsGrounded();
        CheckIsClimbable();
        StepClimb();
        Glide();
    }

    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
        _input.OnSprintInput -= Sprint;
        _input.OnCrouchInput -= Crouch;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
        _input.OnGlideInput -= StartGlide;
        _input.OnCancelGlide -= CancelGlide;
    }

    private void Move(Vector3 direction)
    {
        Vector3 movementDirection = Vector3.zero;
        if (_playerStance == PlayerStance.Stand || _playerStance == PlayerStance.Crouch)
        {
            switch (_cameraManager.CameraState)
            {
                case CameraState.ThirdPerson:
                    if (direction.magnitude >= 0.1)
                    {
                        float rotationAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
                        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                        movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                        _rigidbody.AddForce(movementDirection * Time.fixedDeltaTime * _speed, ForceMode.Acceleration);
                    }
                    break;
                case CameraState.FirstPerson:
                    transform.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
                    Vector3 verticalDirection = direction.z * transform.forward;
                    Vector3 horizontalDirection = direction.x * transform.right;
                    movementDirection = verticalDirection + horizontalDirection;
                    _rigidbody.AddForce(movementDirection * Time.fixedDeltaTime * _speed, ForceMode.Acceleration);
                    break;
                default:
                    break;
            }
            Vector3 velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _animator.SetFloat("Velocity", velocity.magnitude);
        }
        else if (_playerStance == PlayerStance.Climb)
        {
            Vector3 horizontal = direction.x * transform.right;
            Vector3 vertical = direction.z * transform.up;
            movementDirection = horizontal + vertical;
            _rigidbody.AddForce(movementDirection * Time.fixedDeltaTime * _climbSpeed, ForceMode.Acceleration);
            _animator.SetFloat("ClimbVelocityX", _rigidbody.velocity.x * -1);
            _animator.SetFloat("ClimbVelocityY", _rigidbody.velocity.y);
        }
        else if (_playerStance == PlayerStance.Glide)
        {
            Vector3 rotationDegree = transform.rotation.eulerAngles;
            rotationDegree.x += _glideRotationSpeed.x * direction.z * Time.deltaTime;
            rotationDegree.x = Mathf.Clamp(rotationDegree.x, _minGlideRotation.x, _maxGlideRotation.x);
            rotationDegree.z += _glideRotationSpeed.z * direction.x * Time.deltaTime;
            // rotationDegree.z = Mathf.Clamp(rotationDegree.z, _minGlideRotation.z, _maxGlideRotation.z);
            rotationDegree.y += _glideRotationSpeed.y * direction.x * Time.deltaTime;
            // rotationDegree.y = Mathf.Clamp(rotationDegree.y, _minGlideRotation.y, _maxGlideRotation.y);
            transform.rotation = Quaternion.Euler(rotationDegree);
        }
    }

    private void Glide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            Vector3 rotationDegree = transform.rotation.eulerAngles;
            float lift = rotationDegree.x;
            Vector3 movementDirection = (transform.forward * _glideSpeed) + (transform.up * (lift + _airDrag));
            _rigidbody.AddForce(movementDirection * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    private void Sprint(bool isSprint)
    {
        if (isSprint)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsCrouch", false);
            if (_speed < _runSpeed)
            {
                _speed += 20 * Time.deltaTime;
            }
        }
        else
        {
            if (_speed > _walkSpeed)
            {
                _speed -= 20 * Time.deltaTime; ;
            }
        }
    }

    private void Crouch()
    {
        if (_playerStance == PlayerStance.Crouch)
        {
            _collider.height = 1.8f;
            _collider.center = Vector3.up * 0.9f;
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsCrouch", false);
            _speed = _walkSpeed;
        }
        else
        {

            _collider.height = 1.3f;
            _collider.center = Vector3.up * 0.66f;
            _playerStance = PlayerStance.Crouch;
            _animator.SetBool("IsCrouch", true);
            _speed = _crouchSpeed;
        }
    }

    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundChecker.position, _groundCheckerRadius, _groundLayer);
        _animator.SetBool("IsGrounded", _isGrounded);
        if (!_isGrounded && _playerStance != PlayerStance.Climb && _playerStance != PlayerStance.Glide)
        {
            _hudManager.ShowGlideKeyInfo();
        }
        else
        {
            _hudManager.HideGlideKeyInfo();
        }
        if (_isGrounded)
        {
            CancelGlide();
        }
    }

    private void Jump()
    {
        Vector3 jumpDirection = Vector3.up * _jumpForce;
        if (_isGrounded)
        {
            _rigidbody.AddForce(jumpDirection * Time.fixedDeltaTime * _jumpForce, ForceMode.Acceleration);
            _animator.SetTrigger("Jump");
        }
    }

    private void CheckIsClimbable()
    {
        if (Physics.Raycast(_climbChecker.position, transform.forward, out RaycastHit hit, _ClimbCheckDistance, _climableLayer) && _isGrounded && _playerStance != PlayerStance.Climb)
        {
            _hudManager.ShowClimbKeyInfo();
        }
        else
        {
            _hudManager.HideClimbKeyInfo();
        }
    }

    private void StartClimb()
    {
        if (Physics.Raycast(_climbChecker.position, transform.forward, out RaycastHit hit, _ClimbCheckDistance, _climableLayer) && _isGrounded && _playerStance != PlayerStance.Climb)
        {
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(70);
            _animator.SetBool("IsClimbing", true);
            _collider.center = Vector3.up * 1.3f;
            transform.position = hit.point - (transform.forward * _climbOffset.z) - (Vector3.up * _climbOffset.y);
            _playerStance = PlayerStance.Climb;
            _rigidbody.useGravity = false;
            _hudManager.ShowCancelKeyInfo("Climb");
        }
    }
    private void CancelClimb()
    {
        if (_playerStance == PlayerStance.Climb)
        {
            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(40);
            _animator.SetBool("IsClimbing", false);
            _collider.center = Vector3.up * 0.9f;
            _playerStance = PlayerStance.Stand;
            _rigidbody.useGravity = true;
            transform.position -= transform.forward * 1f;
            _hudManager.HideCancelKeyInfo();
        }
    }

    private void StepClimb()
    {
        if (Physics.Raycast(_groundChecker.transform.position, transform.forward, out RaycastHit lowerHit, 0.1f))
        {
            if (!Physics.Raycast(_stepChecker.transform.position, transform.forward, out RaycastHit upperHit, 0.2f))
            {
                _rigidbody.AddForce(0, 2.5f, 0);
            }
        }
    }

    private void StartGlide()
    {
        if (_playerStance != PlayerStance.Glide)
        {
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _animator.SetBool("IsGlided", true);
            _playerStance = PlayerStance.Glide;
            _hudManager.ShowCancelKeyInfo("Glide");
        }
    }

    private void CancelGlide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _animator.SetBool("IsGlided", false);
            _playerStance = PlayerStance.Stand;
            _hudManager.HideCancelKeyInfo();
        }
    }
}
