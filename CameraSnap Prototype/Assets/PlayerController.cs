using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;     
using System.Collections;  

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

    [Header("Camera Snap Settings")]
    public float snapZoomFOV = 40f;     
    public float snapDuration = 0.15f;  
    public Image flashImage;            

    [Header("Gallery Settings")]
    public KeyCode galleryKey = KeyCode.G;
    public GalleryViewer galleryViewer; 

    // Private variables
    private CharacterController characterController;
    private Camera playerCamera;
    private float originalFOV; // To remember the camera's normal FOV

    private bool isCameraMode = false;
    private bool isGalleryOpen = false;
    private float cameraVerticalRotation = 0f;


    void Start()
    {
        // Get references to our components
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // Stores the camera's starting Field of View
        originalFOV = playerCamera.fieldOfView;

        // Locks and hides the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure the effects are off at the start
        if (cameraEffectVolume != null)
        {
            cameraEffectVolume.weight = 0;
        }

        // Ensure the flash is off at the start
        if (flashImage != null)
        {
            Color flashColor = flashImage.color;
            flashColor.a = 0f;
            flashImage.color = flashColor;
        }
    }

    void Update()
    {
        // Checks for Gallery Toggle (G Key)
        // Only allow toggling if not in camera mode
        if (Input.GetKeyDown(galleryKey) && !isCameraMode)
        {
            ToggleGallery();
        }

        // Checks for Mode Swap (F Key)
        // Only allow toggling if gallery is not open
        if (Input.GetKeyDown(cameraModeKey) && !isGalleryOpen)
        {
            isCameraMode = !isCameraMode;

            if (cameraEffectVolume != null)
            {
                cameraEffectVolume.weight = isCameraMode ? 1.0f : 0.0f;
            }
        }

        // Checks for Camera Snap (Spacebar)
        // Only allow if in camera mode and gallery is not open
        if (isCameraMode && Input.GetKeyDown(KeyCode.Space) && !isGalleryOpen)
        {
            StartCoroutine(PhotoSnapSequence());
        }

        // Mouse Look
        // Only allow if gallery is not open
        if (!isGalleryOpen)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotates the Player object left/right
            transform.Rotate(Vector3.up * mouseX);

            // Rotates the Camera up/down
            cameraVerticalRotation -= mouseY;
            cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -cameraVerticalAngleLimit, cameraVerticalAngleLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0, 0);
        }

        // Player Movement
        // Only allow if not in camera mode and gallery is not open
        if (!isCameraMode && !isGalleryOpen)
        {
            float horizontal = Input.GetAxis("Horizontal"); // A/D keys
            float vertical = Input.GetAxis("Vertical");     // W/S keys

            Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
            characterController.SimpleMove(moveDirection * walkSpeed);
        }
    }

    // This function controls the player state and tells the UI to open/close
    private void ToggleGallery()
    {
        isGalleryOpen = !isGalleryOpen;

        // Tell the GalleryViewer script to show/hide
        if (galleryViewer != null)
        {
            galleryViewer.ToggleGallery(isGalleryOpen);
        }

        // Manage cursor
        if (isGalleryOpen)
        {
            // Unlocks and shows the cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Re-locks and hides the cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // This is the function that handles the photo snap effect over time
    IEnumerator PhotoSnapSequence()
    {
        // Zoom In
        float halfDuration = snapDuration / 2f;
        float elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            playerCamera.fieldOfView = Mathf.Lerp(originalFOV, snapZoomFOV, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Force it to the final zoom value
        playerCamera.fieldOfView = snapZoomFOV;

        // Capture logic
        // Hides the flash UI so it's not in the picture
        if (flashImage != null)
        {
            flashImage.gameObject.SetActive(false);
        }

        // Wait for the end of the frame to ensure rendering is complete
        yield return new WaitForEndOfFrame();

        // Capture the screen and create a texture
        Texture2D photo = ScreenCapture.CaptureScreenshotAsTexture();

        // Send the new photo to our gallery
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
            flashColor.a = 1.0f; // This was 1.0f in your original code
            flashImage.color = flashColor;

            yield return new WaitForSeconds(0.2f); // This was 0.2f in your original code

            flashColor.a = 0f;
            flashImage.color = flashColor;
        }

        // Zoom Out
        elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            playerCamera.fieldOfView = Mathf.Lerp(snapZoomFOV, originalFOV, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerCamera.fieldOfView = originalFOV;
    }
}
