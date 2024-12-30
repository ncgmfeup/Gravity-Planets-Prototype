using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColEvents : MonoBehaviour
{
    public GameObject WinCanvas;
    private Timer_ timer;

    void Start()
    {
        timer = FindObjectOfType<Timer_>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Win")
        {
            WinCanvas.SetActive(true);
            timer.OnLevelComplete();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
    }
}
