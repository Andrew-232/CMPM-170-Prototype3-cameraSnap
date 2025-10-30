using UnityEngine;
using UnityEngine.Rendering;

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

    private CharacterController characterController;
    private Camera playerCamera;
    private bool isCameraMode = false;
    private float cameraVerticalRotation = 0f;

    void Start()
    {
        // Gets references to our components
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // Locks and hides the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Makes sure the effects are off at the start
        if (cameraEffectVolume != null)
        {
            cameraEffectVolume.weight = 0;
        }
    }

    void Update()
    {
        // This happens first so the other logic reacts immediately
        if (Input.GetKeyDown(cameraModeKey))
        {
            isCameraMode = !isCameraMode;

            // Updates the camera effect's volume weight so it reflects the current mode/effect state
            if (cameraEffectVolume != null)
            {
                // If in camera mode, set weight to 1
                // If not, set weight to 0
                cameraEffectVolume.weight = isCameraMode ? 1.0f : 0.0f;
            }
        }

        // Gets mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotates the Player object left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotates the Camera up/down
        cameraVerticalRotation -= mouseY;
        cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -cameraVerticalAngleLimit, cameraVerticalAngleLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraVerticalRotation, 0, 0);

        // Player Movement
        if (!isCameraMode)
        {
            float horizontal = Input.GetAxis("Horizontal"); // A/D keys
            float vertical = Input.GetAxis("Vertical");     // W/S keys

            Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;

            // Applies movement using the CharacterController component I added to the Player object
            characterController.SimpleMove(moveDirection * walkSpeed);
        }
    }
}
