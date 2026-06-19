using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCamera : MonoBehaviour
{
    [Header("Camera to Assign")]
    public GameObject AimCam;
    public GameObject AimCanvas;
    public GameObject ThirdPersonCam;
    public GameObject ThirdPersonCanvas;

    [Header("Player Reference")]
    public PlayerController playerController;

    private void Start()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
    }

    private void Update()
    {
        bool canAim = playerController != null && 
                      playerController.activeRifle != null && 
                      playerController.activeRifle.gameObject.activeInHierarchy;

        if (canAim && Input.GetButton("Fire2"))
        {
            ThirdPersonCam.SetActive(false);
            ThirdPersonCanvas.SetActive(false);
            AimCam.SetActive(true);
            AimCanvas.SetActive(true);
        }
        else
        {
            ThirdPersonCam.SetActive(true);
            ThirdPersonCanvas.SetActive(true);
            AimCam.SetActive(false);
            AimCanvas.SetActive(false);
        }
    }
}


