using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform player;
    public Transform currentPlanet;

    float xRotation;
    float yRotation;
    PlanetGravity plGravity;

    private void Start()
    {
        plGravity = GetComponentInParent<PlanetGravity>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        
        /*
        // Gravity direction (up relative to the planet)
        currentPlanet = plGravity.currentPlanet;
        Vector3 gravityUp = (player.position - currentPlanet.position).normalized;

        // Compute yaw rotation based on the gravity up axis
        Quaternion yawRotation = Quaternion.AngleAxis(yRotation, gravityUp);

        // Compute the forward direction relative to the gravity up axis
        Vector3 forwardDirection = yawRotation * Vector3.forward;

        // Update the orientation's rotation
        orientation.rotation = Quaternion.LookRotation(forwardDirection, gravityUp);
        */       
    }
}
