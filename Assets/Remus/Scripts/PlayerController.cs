using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float WalkSpeed = 5f;
    public float SprintMultiplier = 2f;
    public float JumpForce = 5f;
    public float GroundCheckDistance = 1.5f;
    public float LookSensitivityX = 1f;
    public float LookSensitivityY = 1f;
    public float MinYLookAngle = -90f;
    public float MaxYLookAngle = 90f;
    public float Gravity = -9.8f;

    private CharacterController characterController;
    private Vector3 velocity;
    private float verticalRotation = 0f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;
    private bool jumpPressed;

    private float xRotation;
    private float mouseX;
    private float mouseY;

    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    [SerializeField] private CinemachineVirtualCamera firstPersonCamera;
    [SerializeField] private CinemachineVirtualCamera thirdPersonCamera;
    [SerializeField] private Transform thirdPersonAnchor; // The orb or anchor point

    private bool isThirdPerson = false;


    // Camera bobbing parameters
    [SerializeField] private float bobFrequency = 6f; // Speed of bobbing
    [SerializeField] private float bobAmplitude = 0.1f; // Intensity of bobbing
    [SerializeField] private float bobSmoothing = 5f; // Smooth transitions

    private CinemachineCameraOffset cameraOffset;
    private float bobTimer = 0f;
    private Vector3 defaultOffset;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        // Get the CinemachineCameraOffset component
        cameraOffset = cinemachineVirtualCamera.GetComponent<CinemachineCameraOffset>();

        if (cameraOffset != null)
        {
            defaultOffset = cameraOffset.m_Offset;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleCamera();

        // Toggle between first-person and third-person view when "V" is pressed
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            ToggleCameraView();
        }
    }

    private void ToggleCameraView()
    {
        isThirdPerson = !isThirdPerson;

        if (isThirdPerson)
        {
            firstPersonCamera.Priority = 0;  // Lower priority (inactive)
            thirdPersonCamera.Priority = 10; // Higher priority (active)

            // Ensure thirdPersonCamera follows the anchor correctly
            thirdPersonCamera.Follow = thirdPersonAnchor;
            thirdPersonCamera.LookAt = transform;
        }
        else
        {
            firstPersonCamera.Priority = 10; // Higher priority (active)
            thirdPersonCamera.Priority = 0;  // Lower priority (inactive)
        }
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.Normalize();

        float speed = isSprinting ? WalkSpeed * SprintMultiplier : WalkSpeed;
        characterController.Move(moveDirection * speed * Time.deltaTime);

        // More reliable ground check
        bool grounded = IsGrounded();

        if (grounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f; // Ensures player stays grounded

            if (jumpPressed) // Jump only when grounded
            {
                velocity.y = JumpForce;
                jumpPressed = false; // Reset jump input
            }
        }
        else
        {
            velocity.y += Gravity * Time.deltaTime;
        }

        characterController.Move(velocity * Time.deltaTime);

        // Call the bobbing effect function
        ApplyHeadBobbing();
    }

    private void HandleCamera()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cinemachineVirtualCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // Helper Functions
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, GroundCheckDistance + 0.1f);
    }

    private void ApplyHeadBobbing()
    {
        if (cameraOffset == null) return;

        if (moveInput.sqrMagnitude > 0.01f && IsGrounded()) // Player is moving and grounded
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;

            Vector3 newOffset = defaultOffset;
            newOffset.y += bobOffset;

            cameraOffset.m_Offset = Vector3.Lerp(cameraOffset.m_Offset, newOffset, Time.deltaTime * bobSmoothing);
        }
        else
        {
            bobTimer = 0;
            cameraOffset.m_Offset = Vector3.Lerp(cameraOffset.m_Offset, defaultOffset, Time.deltaTime * bobSmoothing);
        }
    }

    // Input Callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            jumpPressed = true;
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
    }
}