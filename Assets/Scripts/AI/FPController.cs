using UnityEngine;

public class FPController : MonoBehaviour
{
    public float moveSpeed = 5f;  // Movement speed
    public float lookSpeed = 2f;  // Mouse look sensitivity

    private float rotationX = 0f;
    private float rotationY = 0f;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        // Get input for WASD movement
        float moveForward = Input.GetAxis("Vertical");   // W/S or Up/Down arrow
        float moveRight = Input.GetAxis("Horizontal");  // A/D or Left/Right arrow

        // Q/E for vertical movement
        float moveUp = 0f;
        if (Input.GetKey(KeyCode.Q)) moveUp = -1f;
        if (Input.GetKey(KeyCode.E)) moveUp = 1f;

        // Combine movement vectors
        Vector3 move = transform.forward * moveForward + transform.right * moveRight + transform.up * moveUp;

        // Apply movement
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        // Rotate the camera based on mouse input
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Clamp vertical rotation to avoid flipping

        rotationY += mouseX;

        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
}
