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


    public float playerSpeed = 5.0f;
    public float sprintSpeed = 7.0f;
    public float crouchSpeed = 2.0f;
    public float jumpHeight = 0.8f;
    public float gravityMultiplier = 2f;
    public float gravityValue = -9.81f;

    private Transform cameraTransform;

    private Vector3 gravityVelocity;
    private Vector3 targetDirection;

    private float speed;
    private float velocitySmoothing = 0.1f;

    private bool isGrounded;

    [SerializeField] private OrbAI orbieAI;
    private Animator animator;
    private AudioSource playerAudio;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerAudio = gameObject.AddComponent<AudioSource>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        // Get the CinemachineCameraOffset component
        cameraOffset = cinemachineVirtualCamera.GetComponent<CinemachineCameraOffset>();

        if (cameraOffset != null)
        {
            defaultOffset = cameraOffset.m_Offset;
        }
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;

        gravityValue *= gravityMultiplier;
    }

    private void Update()
    {
        if (!isThirdPerson)
        {
            HandleMovement();
            HandleCamera();
            orbieAI.DeactivateThirdPersonMode();
        }
        else
        {
            HandleThirdMovement();
            orbieAI.ActivateThirdPersonMode();
        }

        CheckShelter();

        // Toggle between first-person and third-person view when "V" is pressed
        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            ToggleCameraView();
        }

        speed = Mathf.Clamp(moveInput.magnitude, 0f, 1f);
        speed = Mathf.SmoothDamp(animator.GetFloat("Speed"), speed, ref velocitySmoothing, 0.1f);
        animator.SetFloat("Speed", speed);

        // Animation: Update Fall State
        bool isFalling = !IsGrounded() && velocity.y < 0;
        animator.SetBool("IsGrounded", IsGrounded());
        animator.SetFloat("VelocityY", velocity.y);

        HandleKeyAnimations();
    }

    private void HandleThirdMovement()
    {
        isGrounded = characterController.isGrounded;

        if (isGrounded && gravityVelocity.y < 0)
        {
            gravityVelocity.y = 0f;
        }

        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        Vector3 right = cameraTransform.right;
        targetDirection = moveInput.x * right + moveInput.y * forward;

        float moveSpeed = isSprinting ? sprintSpeed : playerSpeed;
        Vector3 moveVelocity = targetDirection.normalized * moveSpeed;

        characterController.Move(moveVelocity * Time.deltaTime + gravityVelocity * Time.deltaTime);


        if (targetDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }

        // Jump
        if (jumpPressed && isGrounded)
        {
            gravityVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
            jumpPressed = false;
        }

        // Gravity
        gravityVelocity.y += gravityValue * Time.deltaTime;

        // Sprinting Reset
        if (moveInput.magnitude == 0)
        {
            isSprinting = false;
        }
    }

    public void ToggleCameraView()
    {
        isThirdPerson = !isThirdPerson;

        if (isThirdPerson)
        {
            firstPersonCamera.Priority = 0;
            thirdPersonCamera.Priority = 10;
        }
        else
        {
            firstPersonCamera.Priority = 10;
            thirdPersonCamera.Priority = 0;
        }
    }

    public void ForceFirstPersonMode()
    {
        isThirdPerson = false;
        firstPersonCamera.Priority = 10;
        thirdPersonCamera.Priority = 0;
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
            if (moveDirection.sqrMagnitude > 0)
            {
                AudioManager.Instance.PlayRandomAudio("PlayerFootsteps", ref playerAudio, 0.25f, true, speed * 0.35f, true, 0.7f, 1.3f);
                animator.SetFloat("SpeedMod", speed * 0.2f);
            }
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

    // Weather
    private void CheckShelter()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f; // Slightly above the player to avoid ground collision
        Vector3 rayDirection = Vector3.up;
        float rayDistance = 1000f;

        RaycastHit hit;
        bool isSheltered = Physics.Raycast(rayStart, rayDirection, out hit, rayDistance);

        // Draw the debug ray
        Color rayColor = isSheltered ? Color.green : Color.red;
        Debug.DrawRay(rayStart, rayDirection * rayDistance, rayColor);

        // Debugging logs
        //if (isSheltered)
        //{
        //    Debug.Log($"[PlayerController] Player is UNDER SHELTER. Hit: {hit.collider.gameObject.name}");
        //}
        //else
        //{
        //    Debug.Log("[PlayerController] Player is EXPOSED to weather.");
        //}
    }

    // Public method for WeatherManager
    public bool IsUnderShelter()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Vector3 rayDirection = Vector3.up;
        float rayDistance = 1000f;

        RaycastHit hit;
        return Physics.Raycast(rayStart, rayDirection, out hit, rayDistance);
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
            animator.SetTrigger("Jump");
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
    }

    private void HandleKeyAnimations()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            animator.SetTrigger("Interact");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            animator.SetTrigger("Consume");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            animator.SetTrigger("Attack");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            animator.SetTrigger("Mine");
        }
    }
}