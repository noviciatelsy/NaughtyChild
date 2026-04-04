using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions; 
public class playermovement : MonoBehaviour, PlayerInput.IGameModeActions
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    private PlayerInput inputActions;
    private Vector2 moveInput;
    private Rigidbody rb;

    private void Awake()
    {
        inputActions = new PlayerInput();
        inputActions.GameMode.AddCallbacks(this);
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        inputActions.GameMode.Enable();
    }

    private void OnDisable()
    {
        inputActions.GameMode.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveInput.x, 0f, moveInput.y));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
        }
    }
}
