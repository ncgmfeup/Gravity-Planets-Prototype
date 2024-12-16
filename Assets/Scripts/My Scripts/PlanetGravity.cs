using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    public List<Transform> planets; // Assign all planets here in the inspector or dynamically
    public float gravityForce = 9.81f;
    public float switchDistance = 10f; // Distance threshold for switching gravity
    public float alignSpeed = 3f;  // Speed of rotation when aligning with gravity
    public Transform currentPlanet;  // The current planet applying gravity

    private Rigidbody playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.useGravity = false;
        FindClosestPlanet();
    }

    private void FixedUpdate()
    {
        FindClosestPlanet();
        ApplyCustomGravity();
    }

    private void FindClosestPlanet()
    {
        float closestDistance = float.MaxValue;
        Transform closestPlanet = null;

        foreach (Transform planet in planets)
        {
            float distance = Vector3.Distance(transform.position, planet.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlanet = planet;
            }
        }

        // Switch to the new planet if it's within the switch distance
        if (closestPlanet != null && closestPlanet != currentPlanet && closestDistance <= switchDistance)
        {
            currentPlanet = closestPlanet;
        }

        Debug.Log(closestPlanet);
    }

    private void ApplyCustomGravity()
    {
        if (currentPlanet == null) return;

        // Calculate gravity direction
        Vector3 gravityDirection = (currentPlanet.position - transform.position).normalized;
        if (gravityDirection.magnitude < 0.001f) return;

        // Apply force towards the planet's center
        playerRigidbody.AddForce(gravityDirection * gravityForce, ForceMode.Acceleration);

        Vector3 playerUp = transform.up;

        if (gravityDirection.magnitude < 0.001f || playerUp.magnitude < 0.001f)
        return;

        // Smoothly rotate the player to align with the gravity direction
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, alignSpeed * Time.deltaTime);

        // Preserve yaw (horizontal) rotation using the PlayerCam script
        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(euler.x, transform.eulerAngles.y, euler.z);
    }
}