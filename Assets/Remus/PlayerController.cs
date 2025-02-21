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
    private AudioSource audioSource;
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        HandleMovement();
        HandleCamera();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.Normalize();

        float speed = isSprinting ? WalkSpeed * SprintMultiplier : WalkSpeed;
        characterController.Move(moveDirection * speed * Time.deltaTime);

        if (IsGrounded())
        {
            if (moveDirection.sqrMagnitude > 0)
                AudioManager.Instance.PlayRandomAudio("PlayerFootsteps", ref audioSource, default, default, speed * 0.2f);
            if (velocity.y < 0) velocity.y = -2f; // Reset gravity accumulation

            if (jumpPressed)
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


    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, GroundCheckDistance + 0.1f);
    }

    // ---- INPUT SYSTEM CALLBACKS ----
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
        if (context.performed)
        {
            jumpPressed = true;
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
    }
}