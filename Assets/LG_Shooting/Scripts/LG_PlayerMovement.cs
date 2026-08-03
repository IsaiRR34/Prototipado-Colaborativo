using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class LG_PlayerMovement : MonoBehaviour
{
    [Header("Movement Properties")]
    [Tooltip("Speed of player movement.")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Rotation speed of the player turning to face the movement direction.")]
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Input Settings (Optional)")]
    [Tooltip("Input action reference to read Vector2 movement input. If left empty, legacy inputs will be used instead.")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Components (Auto-detected if null)")]
    [Tooltip("Character Controller for physics-based collision and movement. (Recommended for standard humanoid characters)")]
    [SerializeField] private CharacterController characterController;

    [Tooltip("Rigidbody for physics-controlled movement. (Alternative to CharacterController)")]
    [SerializeField] private Rigidbody rb;

    private Vector2 inputVector;
    private Vector3 moveDirection;

    private void OnEnable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
        {
            moveAction.action.Disable();
        }
    }

    private void Start()
    {
        // Auto-assign references if not set in Inspector
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        // Configure Rigidbody parameters for cleaner movement logic if it exists
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
    }

    private void Update()
    {
        // Read movement input
        if (moveAction != null && moveAction.action != null)
        {
            inputVector = moveAction.action.ReadValue<Vector2>();
        }
        else
        {
            // Legacy input manager fallback
            inputVector.x = Input.GetAxisRaw("Horizontal");
            inputVector.y = Input.GetAxisRaw("Vertical");
        }

        // Calculate direction relative to the world coordinates
        moveDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

        // Rotate the player to face the movement direction
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Fallback: Translate movement via Transform if neither CharacterController nor Rigidbody is present
        if (characterController == null && rb == null)
        {
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    private void FixedUpdate()
    {
        // Handle Rigidbody movement
        if (rb != null)
        {
            Vector3 movement = moveDirection * moveSpeed;
            // Maintain the current y-velocity (gravity, jumping, etc.)
            rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
        }
    }

    private void LateUpdate()
    {
        // Handle CharacterController movement
        if (characterController != null)
        {
            Vector3 movement = moveDirection * moveSpeed;

            // Apply basic gravity effect to keep the controller grounded
            if (!characterController.isGrounded)
            {
                movement.y += Physics.gravity.y;
            }
            else
            {
                movement.y = -0.5f; // Small downward force to stay stuck to slopes
            }

            characterController.Move(movement * Time.deltaTime);
        }
    }
}
