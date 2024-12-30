using UnityEngine;
using UnityEngine.UI;

public class GrapplingGunPickup : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    public Transform playerCamera;
    public Transform gun;
    public Transform gunHoldPoint; // Where the gun should attach (camera's child)
    public Text interactText; // UI text to display the "Press E to Grab" message

    [Header("Settings")]
    public float pickupRange = 5f; 
    public KeyCode pickupKey = KeyCode.E;

    private bool isGunPickedUp = false;

    void Start()
    {
        player.GetComponent<Grappling>().enabled = false;
        interactText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isGunPickedUp) return;

        HandleGunPickup();
    }

    void HandleGunPickup()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            if (hit.transform == gun)
            {
                interactText.gameObject.SetActive(true);
                if (Input.GetKeyDown(pickupKey))
                {
                    PickUpGun();
                }
                return;
            }
        }

        interactText.gameObject.SetActive(false);
    }

    void PickUpGun()
    {
        isGunPickedUp = true;

        gun.SetParent(gunHoldPoint);
        gun.gameObject.GetComponent<BoxCollider>().enabled = false;
        gun.gameObject.GetComponent<LineRenderer>().enabled = true;

        gun.localPosition = Vector3.zero;
        gun.localRotation = Quaternion.identity;

        interactText.gameObject.SetActive(false);
        player.GetComponent<Grappling>().enabled = true;
    }
}

