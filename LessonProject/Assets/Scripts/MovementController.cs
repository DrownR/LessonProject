using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    [Header("REFERENCSE")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraDirection;

    [Header("MOVEMENT")]

    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float rotationSpeed = 10f;
    private Vector3 playerVelocity;
    private bool groundedPlayer;


    [Header("JUMP")]

    [SerializeField] private float jumpHeight = 1.5f;

    [Header("INPUT ACTIONS")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();

        jumpAction.action.performed += OnJump;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();

        jumpAction.action.performed -= OnJump;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        Jump();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && playerVelocity.y < -2f)
        {
            playerVelocity.y = -2f;
        }

        // INPUT
        Vector3 move = GetMovementInput();

        // CAMERA RELATIVE MOVEMENT
        if (move != Vector3.zero)
        {
            Vector3 camForward, camRight;
            HandleCameraRotation(out camForward, out camRight);

            Vector3 moveDirection = camForward * move.z + camRight * move.x;

            // ROTATION (smooth)
            HandleRotation(moveDirection);

            // MOVE
            controller.Move(moveDirection * playerSpeed * Time.deltaTime);
        }        

        // GRAVITY
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(Vector3.up * playerVelocity.y * Time.deltaTime);
    }

    

    private void Jump()
    {
        if (groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }
    }

    private void HandleRotation(Vector3 moveDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleCameraRotation(out Vector3 camForward, out Vector3 camRight)
    {
        camForward = cameraDirection.forward;
        camRight = cameraDirection.right;
        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();
    }

    private Vector3 GetMovementInput()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 move = new Vector3(input.x, 0, input.y);
        move = Vector3.ClampMagnitude(move, 1f);

        return move;
    }
}