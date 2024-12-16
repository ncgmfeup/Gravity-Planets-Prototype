using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("KeyBindings")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float groundDrag;
    public float playerHeight;
    public float rayLength = 0.2f;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;
    public Transform currentPlanet;

    float horizontalInput;
    float verticalInput;

    private Vector3 lastMoveDirection;
    Vector3 moveDirection;
    Rigidbody rb;
    PlanetGravity plGravity;

    private void Start()
    {
        plGravity = GetComponent<PlanetGravity>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, -transform.up, playerHeight * 0.5f + rayLength, whatIsGround);
        currentPlanet = plGravity.currentPlanet;

        MyInput();
        SpeedControl();
        AlignToGravity();

        if(grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            //continuosly jump if you hold down the key
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // Calculate gravity direction (planet "up" direction)
        Vector3 gravityUp = (transform.position - currentPlanet.position).normalized;

        // Recalculate forward and right directions relative to the player's current "up"
        Vector3 forward = Vector3.ProjectOnPlane(orientation.forward, gravityUp).normalized;
        Vector3 right = Vector3.ProjectOnPlane(orientation.right, gravityUp).normalized;

        // Use these recalculated directions for movement
        moveDirection = forward * verticalInput + right * horizontalInput;

        if (moveDirection.magnitude > 0.1f) // Avoid tiny movements
        {
            if (grounded)
                rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
            else
            {
                // Allow mid-air control with reduced force
                Vector3 airControlForce = moveDirection * moveSpeed * airMultiplier * 10f;

                // Only apply force in directions perpendicular to gravity
                rb.AddForce(Vector3.ProjectOnPlane(airControlForce, gravityUp), ForceMode.Force);
            }
        }
    }

    private void SpeedControl()
    {
        Vector3 gravityUp = (transform.position - currentPlanet.position).normalized;
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.velocity, gravityUp);

        if (horizontalVelocity.magnitude > moveSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * moveSpeed;
        }

        // Recombine horizontal and vertical velocities
        rb.velocity = horizontalVelocity + Vector3.Project(rb.velocity, gravityUp);
    }

    private void Jump()
    {   
        Vector3 gravityUp = (transform.position - currentPlanet.position).normalized;
        rb.velocity = Vector3.ProjectOnPlane(rb.velocity, gravityUp);

        // Apply jump force in the local "up" direction
        rb.AddForce(gravityUp * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
    
    private void AlignToGravity()
    {
        // Calculate gravity direction
        Vector3 gravityUp = (transform.position - currentPlanet.position).normalized;

        // Calculate the player's forward direction (preserve movement direction)
        Vector3 desiredForward = Vector3.ProjectOnPlane(orientation.forward, gravityUp).normalized;

        if (desiredForward.magnitude < 0.001f)
            desiredForward = Vector3.ProjectOnPlane(transform.forward, gravityUp).normalized;

        // Smoothly align the player's up direction with the gravity direction
        Quaternion targetRotation = Quaternion.LookRotation(desiredForward, gravityUp);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * (grounded ? 10f : 5f));
    }
}
