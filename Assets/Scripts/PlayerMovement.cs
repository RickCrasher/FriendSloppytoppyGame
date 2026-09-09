using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runningSpeed = 9.0f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;

    [Header("Heights")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float Gravity = 20.0f;
    [SerializeField] private float normalHeight = 0.9f;
    [SerializeField] private float crouchHeightMultiplier = 0.5f;

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

        currentMoveSpeed = crouchInput.IsPressed() ? walkSpeed * crouchSpeedMultiplier : currentMoveSpeed;
        

        HandleMovement(moveVector);
        HandleLooking(mouseDelta);


    }

    void HandleMovement(Vector2 moveVector)
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float oldY = moveDirection.y;

        Vector2 newSpeed = new Vector2(moveVector.y * currentMoveSpeed, moveVector.x * currentMoveSpeed);

        moveDirection = (forward * newSpeed.x) + (right * newSpeed.y);
        moveDirection.y = (jumped && cC.isGrounded && !isInWater) ? jumpForce : oldY;

        if (!cC.isGrounded)
        {
            moveDirection.y -= Gravity * Time.deltaTime;
        }

        cC.Move(moveDirection * Time.deltaTime);
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
