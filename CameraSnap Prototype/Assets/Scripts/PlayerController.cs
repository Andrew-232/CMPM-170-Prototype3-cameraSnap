using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5.0f;
    public float mouseSensitivity = 2.0f;
    public float cameraVerticalAngleLimit = 80.0f;

    [Header("Mode Settings")]
    public KeyCode cameraModeKey = KeyCode.F;
    public Volume cameraEffectVolume;

    [Header("Camera Zoom Settings")]
    public float minFOV = 15f;
    public float zoomSpeed = 10f;

    [Header("Selfie Mode Settings")]
    public KeyCode selfieKey = KeyCode.R;
    public LayerMask playerLayer;
    [Tooltip("How far the camera moves back (from the player) in selfie mode.")]
    public float selfieCameraDistance = 1.5f;

    [Header("Camera Snap Settings")]
    public Image flashImage;

    [Header("Gallery Settings")]
    public KeyCode galleryKey = KeyCode.G;
    public GalleryViewer galleryViewer;

    [Header("Context UI Settings")]
    public GameObject normalModeUI;
    public GameObject cameraModeUI;
    public GameObject galleryModeUI;

    // Private variables
    private CharacterController characterController;
    private Camera playerCamera;
    private float originalFOV;

    private bool isCameraMode = false;
    private bool isGalleryOpen = false;
    private float cameraVerticalRotation = 0f;

    private bool isSelfieMode = false;
    private float cameraYRotation = 0f;
    private int originalCullingMask;

    // variable to store the camera's original position
    private Vector3 originalCameraLocalPos;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        originalFOV = playerCamera.fieldOfView;
        originalCullingMask = playerCamera.cullingMask;

        // Stores the camera's default local position
        originalCameraLocalPos = playerCamera.transform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraEffectVolume != null)
        {
            cameraEffectVolume.weight = 0;
        }

        if (flashImage != null)
        {
            Color flashColor = flashImage.color;
            flashColor.a = 0f;
            flashImage.color = flashColor;
        }

        UpdateControlUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(galleryKey) && !isCameraMode)
        {
            ToggleGallery();
        }

        if (Input.GetKeyDown(cameraModeKey) && !isGalleryOpen)
        {
            isCameraMode = !isCameraMode;

            if (cameraEffectVolume != null)
            {
                cameraEffectVolume.weight = isCameraMode ? 1.0f : 0.0f;
            }

            if (!isCameraMode)
            {
                // Reset all camera settings when exiting
                playerCamera.fieldOfView = originalFOV;
                isSelfieMode = false;
                cameraYRotation = 0f;
                playerCamera.cullingMask = originalCullingMask;

                // Resets camera position when exiting
                playerCamera.transform.localPosition = originalCameraLocalPos;

                // Force-reset rotation
                playerCamera.transform.localRotation = Quaternion.Euler(cameraVerticalRotation, cameraYRotation, 0);
            }

            UpdateControlUI();
        }

        if (isCameraMode && Input.GetKeyDown(KeyCode.Space) && !isGalleryOpen)
        {
            StartCoroutine(PhotoSnapSequence());
        }

        if (isCameraMode && !isGalleryOpen)
        {
            // Zoom logic
            float currentFOV = playerCamera.fieldOfView;
            if (Input.GetKey(KeyCode.W)) { currentFOV -= zoomSpeed * Time.deltaTime; }
            else if (Input.GetKey(KeyCode.S)) { currentFOV += zoomSpeed * Time.deltaTime; }
            currentFOV = Mathf.Clamp(currentFOV, minFOV, originalFOV);
            playerCamera.fieldOfView = currentFOV;

            // Selfie Mode Toggle
            if (Input.GetKeyDown(selfieKey))
            {
                isSelfieMode = !isSelfieMode;
                cameraYRotation = isSelfieMode ? 180f : 0f;

                if (isSelfieMode)
                {
                    playerCamera.cullingMask |= playerLayer.value;
                    Vector3 selfiePos = originalCameraLocalPos - new Vector3(0, 0, selfieCameraDistance);
                    playerCamera.transform.localPosition = selfiePos;
                }
                else
                {
                    // Resets the camera culling to its original value
                    playerCamera.cullingMask = originalCullingMask;

                    // Resets the camera's position to its original
                    playerCamera.transform.localPosition = originalCameraLocalPos;
                }
            }
        }

        if (!isGalleryOpen)
        {
            // Mouse Look
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            cameraVerticalRotation -= mouseY;
            cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -cameraVerticalAngleLimit, cameraVerticalAngleLimit);

            // applies both the up/down look and the selfie flip
            playerCamera.transform.localRotation = Quaternion.Euler(cameraVerticalRotation, cameraYRotation, 0);
        }

        if (!isCameraMode && !isGalleryOpen)
        {
            // Movement
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
            characterController.SimpleMove(moveDirection * walkSpeed);
        }
    }

    private void ToggleGallery()
    {
        isGalleryOpen = !isGalleryOpen;

        if (galleryViewer != null)
        {
            galleryViewer.ToggleGallery(isGalleryOpen);
        }

        if (isGalleryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        UpdateControlUI();
    }

    IEnumerator PhotoSnapSequence()
    {
        if (cameraModeUI != null)
        {
            cameraModeUI.SetActive(false);
        }
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(false);
        }
        yield return new WaitForEndOfFrame();
        Texture2D photo = ScreenCapture.CaptureScreenshotAsTexture();
        if (cameraModeUI != null)
        {
            cameraModeUI.SetActive(true);
        }
        if (PhotoGallery.Instance != null)
        {
            PhotoGallery.Instance.AddPhoto(photo);
        }
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(true);
        }
        if (flashImage != null)
        {
            Color flashColor = flashImage.color;
            flashColor.a = 1.0f;
            flashImage.color = flashColor;
            yield return new WaitForSeconds(0.2f);
            flashColor.a = 0f;
            flashImage.color = flashColor;
        }
    }

    private void UpdateControlUI()
    {
        if (normalModeUI != null) normalModeUI.SetActive(false);
        if (cameraModeUI != null) cameraModeUI.SetActive(false);
        if (galleryModeUI != null) galleryModeUI.SetActive(false);

        if (isGalleryOpen)
        {
            if (galleryModeUI != null) galleryModeUI.SetActive(true);
        }
        else if (isCameraMode)
        {
            if (cameraModeUI != null) cameraModeUI.SetActive(true);
        }
        else
        {
            if (normalModeUI != null) normalModeUI.SetActive(true);
        }
    }
}
