using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementF : MonoBehaviour
{
    [SerializeField] float _walkingForce;
    [SerializeField] float _maxMoveSpeed = 5f;
    [SerializeField] float _mouseSensitivity = 1f;
    [SerializeField] float _mouseSpeed = 1f;
    [SerializeField] float _jumpForce;

    float _verticalLookRotation = 0f;
    [SerializeField] float _verticalMouseClampAngle = 90;

    [SerializeField] Rigidbody _rb;
    [SerializeField] Camera _playerCamera;

    [Header("Gravity")]
    [SerializeField] float gravityStrength = 9.81f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float gravityRaycastDistance = 1.5f;
    [SerializeField] private float wallSwitchCooldown = 0.5f;
    private float lastWallSwitchTime = -Mathf.Infinity;
    Vector3 customGravityDirection = Vector3.down;

    bool isJumping = false;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerCamera = GetComponentInChildren<Camera>();

        _rb.useGravity = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleCameraMovement();
        HandleJump();
        DetectGravityDirection();
    }

    private void FixedUpdate()
    {
        if (!isJumping)
        {
            // Apply custom gravity
            _rb.AddForce(customGravityDirection * gravityStrength, ForceMode.Acceleration);
        }

        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(_rb.velocity, customGravityDirection);
        if (horizontalVelocity.magnitude > _maxMoveSpeed)
        {
            _rb.velocity -= horizontalVelocity - (horizontalVelocity.normalized * _maxMoveSpeed);
        }

        AlignCharacterToGravity();
    }

    void HandleMovement()
    {
        //Vector2 playerMovementControls = InputManager.Instance.GetPlayerMovement();

        //float moveX = playerMovementControls.x;
        //float moveZ = playerMovementControls.y;
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        moveDirection.Normalize();

        // if (_rb.velocity.magnitude < _maxMoveSpeed)
        // {
        //     _rb.AddForce(moveDirection * _walkingForce * Time.deltaTime, ForceMode.Acceleration);
        //
        // }

        //if (_rb.velocity.magnitude < _maxMoveSpeed)
        //{
        //    _rb.AddForce(moveDirection * _walkingForce * Time.deltaTime, ForceMode.Acceleration);
        //}

        Vector3 targetVelocity = moveDirection * _maxMoveSpeed;

        Vector3 currentHorizontalVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        
        if (moveDirection != Vector3.zero)
        {
            Vector3 velocityChange = targetVelocity - currentHorizontalVelocity;
            _rb.AddForce(velocityChange, ForceMode.Acceleration);
        }
        else
        {
            // Apply damping to stop the player more quickly
            _rb.velocity = Vector3.Lerp(
                _rb.velocity,
                new Vector3(0, _rb.velocity.y, 0), // Keep vertical velocity (e.g., falling)
                Time.deltaTime * 200f // Adjust this factor to control the stopping speed
            );
        }

        ClampMaxVelocity(_maxMoveSpeed);
    }

    void HandleCameraMovement()
    {
        // Vector2 cameraMovementControls = InputManager.Instance.GetCameraMovement();
        // float mouseX = cameraMovementControls.x * _mouseSensitivity;
        // float mouseY = cameraMovementControls.y * _mouseSensitivity;

        // transform.Rotate(Vector3.up * mouseX);

        // _verticalLookRotation -= mouseY;
        // _verticalLookRotation = Mathf.Clamp(_verticalLookRotation, -_verticalLookRotation, _verticalLookRotation);

        // _playerCamera.transform.localEulerAngles = Vector3.right * _verticalLookRotation;
        Vector2 mouseMovement = InputManager.Instance.GetCameraMovement();
        float mouseX = (mouseMovement.x * _mouseSpeed * _mouseSensitivity) * Time.deltaTime;
        float mouseY = (mouseMovement.y * _mouseSpeed * _mouseSensitivity) * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        _verticalLookRotation -= mouseY;
        _verticalLookRotation = Mathf.Clamp(_verticalLookRotation, -_verticalMouseClampAngle, _verticalMouseClampAngle);

        _playerCamera.transform.localEulerAngles = Vector3.right * _verticalLookRotation;
    }

    void HandleJump()
    {
        // Check for jump input and if the player is grounded
        if (Physics.Raycast(transform.position, customGravityDirection, out RaycastHit hit, gravityRaycastDistance))
        {
            if (hit.collider.CompareTag("GravityPlatform"))
            {
                isJumping = false;
                
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 jumpDirection = hit.normal;
                    _rb.velocity -= Vector3.Project(_rb.velocity, -customGravityDirection);
                    _rb.AddForce(jumpDirection * _jumpForce, ForceMode.Impulse);
                    isJumping = true;
                     StartCoroutine(DisableGravityTemporarily(0.2f));
                }
            }
        }
    }

    IEnumerator DisableGravityTemporarily(float delay)
    {
        customGravityDirection = Vector3.zero; // Disable gravity
        yield return new WaitForSeconds(delay);
        DetectGravityDirection(); // Re-enable gravity
    }

    void ClampMaxVelocity(float maxVelocity)
    {
        Vector3 currentVelocity = _rb.velocity;
        // if (currentVelocity.magnitude > maxVelocity)
        // {
        //     _rb.velocity = currentVelocity.normalized * maxVelocity;
        // }
        if (currentVelocity.magnitude > maxVelocity)
        {
            // Smoothly reduce velocity instead of hard clamping
            _rb.velocity = Vector3.ClampMagnitude(currentVelocity, maxVelocity);
        }
    }


    void AlignCharacterToGravity()
    {
        // Calculate the target "up" direction based on the gravity direction
        Vector3 targetUpDirection = -customGravityDirection;

        // Determine the target rotation to make the player's "up" align with the gravity direction
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetUpDirection) * transform.rotation;

        // Smoothly rotate towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void DetectGravityDirection()
    {
        // Define directions to cast rays in
        Vector3[] directions = {
        -transform.up,      // Local down
        transform.up,       // Local up
        -transform.right,   // Local left
        transform.right,    // Local right
        transform.forward,  // Local forward
        -transform.forward  // Local backward
    };

        foreach (var direction in directions)
        {
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, gravityRaycastDistance))
            {
                if (hit.collider.CompareTag("GravityPlatform"))
                {
                    if (Time.time - lastWallSwitchTime >= wallSwitchCooldown)
                    {
                        // Set gravity direction based on the hit normal
                        customGravityDirection = -hit.normal;
                        lastWallSwitchTime = Time.time;
                    }
                    return; // Exit early once we find a valid platform
                }
            }

            customGravityDirection = Vector3.down;
        }

        // If no platform is detected, default to downward gravity
        //customGravityDirection = Vector3.down;
    }
    // private void OnCollisionExit(Collision other)
    // {
    //     // If leaving a gravity platform, reset to global downward gravity
    //     if (other.gameObject.CompareTag("GravityPlatform"))
    //     {
    //         customGravityDirection = Vector3.down;
    //     }
    // }

    private void OnDrawGizmos()
    {
        if (_rb == null) return;

        // Set the color for the gravity direction Gizmo
        Gizmos.color = Color.red;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + (customGravityDirection * 2f);
        Gizmos.DrawLine(startPosition, endPosition);
        Gizmos.DrawSphere(endPosition, 0.1f);

        // Draw raycast directions in Gizmos
        Gizmos.color = Color.blue;
        Vector3[] directions = {
        -transform.up,      // Local down
        transform.up,       // Local up
        -transform.right,   // Local left
        transform.right,    // Local right
        transform.forward,  // Local forward
        -transform.forward  // Local backward
    };

        foreach (var direction in directions)
        {
            Gizmos.DrawRay(transform.position, direction * gravityRaycastDistance);
        }
    }
}
