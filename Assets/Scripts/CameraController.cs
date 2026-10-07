using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target to Follow")]
    [SerializeField] private Transform playerTransform;

    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 3.0f;
    [SerializeField] private float verticalMinLimit = -50.0f; // Look down limit
    [SerializeField] private float verticalMaxLimit = 75.0f;  // Look up limit
    [SerializeField] private Vector3 cameraOffset = new Vector3(1.5f, 2.0f, -3.0f); // Over-the-shoulder offset

    private float mouseX, mouseY;
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerTransform == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // 1. Get mouse input for free look
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX += mouseX;
        rotationY -= mouseY;

        // 2. Clamp vertical angle so camera doesn't flip backward
        rotationY = Mathf.Clamp(rotationY, verticalMinLimit, verticalMaxLimit);

        // 3. Calculate rotation and position behind player
        Quaternion targetRotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 targetPosition = playerTransform.position + targetRotation * cameraOffset;

        transform.rotation = targetRotation;
        transform.position = targetPosition;
    }
}