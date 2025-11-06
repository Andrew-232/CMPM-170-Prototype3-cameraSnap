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

    [Header("Camera Snap Settings")]
    public Image flashImage;

    [Header("Gallery Settings")]
    public KeyCode galleryKey = KeyCode.G;
    public GalleryViewer galleryViewer;

    // UI references
    [Header("Context UI Settings")]
    [Tooltip("UI to show during normal gameplay")]
    public GameObject normalModeUI;
    [Tooltip("UI to show when in camera mode")]
    public GameObject cameraModeUI;
    [Tooltip("UI to show when in the gallery")]
    public GameObject galleryModeUI;

    // Private variables
    private CharacterController characterController;
    private Camera playerCamera;
    private float originalFOV;

    private bool isCameraMode = false;
    private bool isGalleryOpen = false;
    private float cameraVerticalRotation = 0f;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        originalFOV = playerCamera.fieldOfView;

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

        // Sets the initial UI state
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
                playerCamera.fieldOfView = originalFOV;
            }

            // Updates UI when camera mode changes
            UpdateControlUI();
        }

        if (isCameraMode && Input.GetKeyDown(KeyCode.Space) && !isGalleryOpen)
        {
            StartCoroutine(PhotoSnapSequence());
        }

        if (isCameraMode && !isGalleryOpen)
        {
            float currentFOV = playerCamera.fieldOfView;
            if (Input.GetKey(KeyCode.W)) { currentFOV -= zoomSpeed * Time.deltaTime; }
            else if (Input.GetKey(KeyCode.S)) { currentFOV += zoomSpeed * Time.deltaTime; }
            currentFOV = Mathf.Clamp(currentFOV, minFOV, originalFOV);
            playerCamera.fieldOfView = currentFOV;
        }

        if (!isGalleryOpen)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);
            cameraVerticalRotation -= mouseY;
            cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -cameraVerticalAngleLimit, cameraVerticalAngleLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0, 0);
        }

        if (!isCameraMode && !isGalleryOpen)
        {
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

        // Updates UI when gallery state changes
        UpdateControlUI();
    }

    IEnumerator PhotoSnapSequence()
    {
        if (cameraModeUI != null)
        {
            cameraModeUI.SetActive(false);
        }

        // Capture logic
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(false);
        }

        yield return new WaitForEndOfFrame();

        Texture2D photo = ScreenCapture.CaptureScreenshotAsTexture();

        // Shows the camera mode UI again
        if (cameraModeUI != null)
        {
            cameraModeUI.SetActive(true);
        }

        // Sends the new photo to our gallery
        if (PhotoGallery.Instance != null)
        {
            PhotoGallery.Instance.AddPhoto(photo);
        }

        // Re-enables the flash GameObject for the flash effect
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(true);
        }

        // Flash Effect
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

    // manages UI visibility
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
