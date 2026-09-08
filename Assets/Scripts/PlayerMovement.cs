using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runningSpeed = 9.0f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float waterSpeedMultiplier = 0.5f;

    [Header("Heights")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float Gravity = 20.0f;
    [SerializeField] private float normalHeight = 0.9f;
    [SerializeField] private float crouchHeightMultiplier = 0.5f;

    [Header("Swimming")]
    [SerializeField] private float swimUpSpeed = 3.0f;
    [SerializeField] private float swimDownSpeed = 3.0f;
    [SerializeField] private float waterGravityMultiplier = 0.2f;
    [SerializeField] private float waterDrag = 2.0f;
    [SerializeField] private float buoyancyStrength = 1.5f;
    [SerializeField] private float buoyancyDepthRange = 1.0f;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobSpeed = 1.5f;

    private float waterSurfaceY;
    private float bobTimer;

    [Header("View Settings")]
    [SerializeField] private float mouseSens = 1f;
    [SerializeField] private float lookAngleLimit = 90f;

    private Camera mainCamera;
    private CharacterController cC;

    private InputAction moveInput;
    private InputAction runInput;

    private InputAction jumpInput;
    private bool jumped = false;
    private bool isCrouched = false;
    private bool isInWater = false;

    private InputAction crouchInput;


    [SerializeField] private float currentMoveSpeed = 0.0f;
    [SerializeField] private Vector3 moveDirection = Vector3.zero;
    [SerializeField] private float lookAngle;

    private void Awake()
    {
        jumpInput = InputSystem.actions.FindAction("Jump");
        jumpInput.started += Jumped;

        crouchInput = InputSystem.actions.FindAction("Crouch");
        crouchInput.started += Crouched;
    }

    private void Start()
    {
        mainCamera = GetComponentInChildren<Camera>();
        cC = GetComponent<CharacterController>();

        moveInput = InputSystem.actions.FindAction("Move");
        runInput = InputSystem.actions.FindAction("Sprint");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentMoveSpeed = walkSpeed;
    }

    private void Update()
    {
        Vector2 moveVector = moveInput.ReadValue<Vector2>();
        Vector2 mouseDelta = new Vector2(Mouse.current.delta.x.ReadValue(), Mouse.current.delta.y.ReadValue());

        if (!cC.isGrounded)
        {
            jumped = false;
        }

        currentMoveSpeed = runInput.IsPressed() ? runningSpeed : walkSpeed;

        if (isCrouched)
        {
            currentMoveSpeed = walkSpeed * crouchSpeedMultiplier;
        }

        if (isInWater)
        {
            currentMoveSpeed *= waterSpeedMultiplier;
        }

        if (isInWater)
        {
            HandleSwimming(moveVector);
        }
        else
        {
            HandleMovement(moveVector);
        }

        HandleLooking(mouseDelta);
    }

    void HandleMovement(Vector2 moveVector)
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float oldY = moveDirection.y;

        Vector2 newSpeed = new Vector2(moveVector.y * currentMoveSpeed, moveVector.x * currentMoveSpeed);

        moveDirection = (forward * newSpeed.x) + (right * newSpeed.y);
        moveDirection.y = (jumped && cC.isGrounded) ? jumpForce : oldY;

        if (!cC.isGrounded)
        {
            moveDirection.y -= Gravity * Time.deltaTime;
        }

        cC.Move(moveDirection * Time.deltaTime);
    }

    void HandleSwimming(Vector2 moveVector)
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector2 newSpeed = new Vector2(moveVector.y * currentMoveSpeed, moveVector.x * currentMoveSpeed);
        Vector3 horizontal = (forward * newSpeed.x) + (right * newSpeed.y);

        moveDirection.x = horizontal.x;
        moveDirection.z = horizontal.z;

        // depth below the water surface (positive = submerged, using this planet's own Gravity value)
        float headY = transform.position.y + cC.center.y;
        float depth = waterSurfaceY - headY;
        float submersion = Mathf.Clamp01(depth / buoyancyDepthRange);

        // gravity still pulls down, just resisted by water - never zeroed, so it still scales per-planet
        moveDirection.y -= Gravity * waterGravityMultiplier * Time.deltaTime;

        // buoyancy pushes back up, stronger the deeper you are, scaled off the same Gravity value
        float buoyancy = Gravity * buoyancyStrength * submersion;
        moveDirection.y += buoyancy * Time.deltaTime;

        float verticalInput = 0f;
        bool activeInput = false;
        if (jumped) { verticalInput += swimUpSpeed; activeInput = true; }
        if (isCrouched) { verticalInput -= swimDownSpeed; activeInput = true; }

        if (activeInput)
        {
            moveDirection.y = Mathf.Lerp(moveDirection.y, verticalInput, waterDrag * Time.deltaTime);
            bobTimer = 0f;
        }
        else
        {
            moveDirection.y = Mathf.Lerp(moveDirection.y, 0f, waterDrag * Time.deltaTime);

            // idle bob only near the surface, not while sinking/diving
            if (submersion < 1f)
            {
                bobTimer += Time.deltaTime * bobSpeed;
                moveDirection.y += Mathf.Sin(bobTimer) * bobAmplitude;
            }
        }

        jumped = false;

        cC.Move(moveDirection * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = true;
            waterSurfaceY = other.bounds.max.y;
            moveDirection.y = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = false;
        }
    }

    void Jumped(InputAction.CallbackContext _ctx)
    {
        jumped = true;
    }

    void HandleLooking(Vector2 mouseDelta)
    {
        lookAngle += -mouseDelta.y * mouseSens;
        lookAngle = Mathf.Clamp(lookAngle, -lookAngleLimit, lookAngleLimit);

        mainCamera.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);
        transform.rotation *= Quaternion.Euler(0, mouseDelta.x * mouseSens, 0);
    }

    void HandleCrouching()
    {
        currentMoveSpeed = isCrouched ? walkSpeed * crouchSpeedMultiplier : walkSpeed;
        if (!isCrouched)
        {
            transform.localScale = new Vector3(1, normalHeight * crouchHeightMultiplier, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, normalHeight, 1);
        }
    }

    void Crouched(InputAction.CallbackContext _ctx)
    {
        isCrouched = !isCrouched;
        HandleCrouching();
    }
}