using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementF2 : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    //[SerializeField] float _maxMoveSpeed = 5f;
    [SerializeField] float _mouseSensitivity = 1f;
    [SerializeField] float _mouseSpeed = 1f;
    [SerializeField] float groundDrag = 10f;
    [SerializeField] float groundBufferTime = 0.1f;
    [SerializeField] float airMultiplier = 1.5f;
    [SerializeField] float _jumpForce;

    [SerializeField] float dashForce = 20f; // Strength of the dash
    [SerializeField] float dashCooldown = 1f; // Cooldown between dashes
    [SerializeField] float dashDuration = 0.2f;

    float _verticalLookRotation = 0f;
    [SerializeField] float _verticalMouseClampAngle = 90;

    [SerializeField] Rigidbody _rb;
    [SerializeField] Camera _playerCamera;

    [Header("Gravity")]
    [SerializeField] float gravityStrength = 9.81f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float gravityRaycastDistance = 1.5f;
    Vector3 customGravityDirection = Vector3.down;
    bool isJumping = false;
    bool isGrounded;
    private float lastGroundedTime;
    private Coroutine resetGravityCoroutine;
    private bool isDashing = false;
    private float lastDashTime;

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
        Debug.Log(isGrounded);
        ClampMaxVelocity();
        HandleCameraMovement();
        HandleJump();
        //HandleDash();
        DetectGravityDirection();

        if (isGrounded && !isJumping)
            _rb.drag = groundDrag;
        else
            _rb.drag = 0;
    }

    private void FixedUpdate()
    {
        //Apply custom gravity
        if (!isDashing)
        {
            // Apply custom gravity only if not dashing
            _rb.AddForce(customGravityDirection * gravityStrength, ForceMode.Acceleration);
        }

        HandleMovement();
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
        //    _rb.AddForce(moveDirection * _walkingForce * Time.deltaTime, ForceMode.Force);
        //}

        if(isGrounded && !isJumping)
            _rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);

        else if(!isGrounded)
            _rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    /*
    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        Vector3 dashDirection = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
        dashDirection.Normalize();

        _rb.velocity = dashDirection * dashForce;
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }*/

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
        bool wasGrounded = isGrounded;
        
        if (isGrounded)
            lastGroundedTime = Time.time;
        
        bool canJump = (Time.time - lastGroundedTime) <= groundBufferTime;

        if (Physics.Raycast(transform.position, customGravityDirection, out RaycastHit hit, 1.3f))
        {
            if (hit.collider.CompareTag("GravityPlatform") && !isJumping && canJump)
            {
                isGrounded = true;

                if(Input.GetKeyDown(KeyCode.Space))
                {
                    isGrounded = false;
                    isJumping = true;
                    _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
                    _rb.AddForce(-customGravityDirection * _jumpForce, ForceMode.Impulse);
                    StartCoroutine(ResetJumpState());
                }
            }
            else
            {
                isGrounded = false;
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    private IEnumerator ResetJumpState()
    {
        yield return new WaitForSeconds(0.2f);
        isJumping = false;
    }

    void ClampMaxVelocity()
    {
        //Vector3 currentVelocity = _rb.velocity;
        // if (currentVelocity.magnitude > maxVelocity)
        // {
        //     _rb.velocity = currentVelocity.normalized * maxVelocity;
        // }
        //if (currentVelocity.magnitude > maxVelocity)
        //{
            // Smoothly reduce velocity instead of hard clamping
        //    _rb.velocity = Vector3.ClampMagnitude(currentVelocity, maxVelocity);
        //}
        Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

        //limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
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
        bool foundGround = false;

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
                    // Set gravity direction based on the hit normal
                    customGravityDirection = -hit.normal;
                    isJumping = false;
                    isGrounded = true;
                    foundGround = true;
                    
                    if (resetGravityCoroutine != null)
                    {
                        StopCoroutine(resetGravityCoroutine);
                        resetGravityCoroutine = null;
                    }
                    return; // Exit early once we find a valid platform
                }
            }
        }

        if (!foundGround)
        {
            isGrounded = false;

            if (resetGravityCoroutine == null)
            {
                resetGravityCoroutine = StartCoroutine(ResetGravityAfterDelay(0.5f));
            }
        }

        // If no platform is detected, default to downward gravity
        //customGravityDirection = Vector3.down;
    }

    private IEnumerator ResetGravityAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        customGravityDirection = Vector3.down;
        resetGravityCoroutine = null; // Clear the reference once done
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
