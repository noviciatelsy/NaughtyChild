using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
public class playermovement : MonoBehaviour, PlayerInput.IGameModeActions
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [Range(0.1f, 2.5f)]
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float sprintMultiplier = 2f;

    private PlayerInput inputActions;
    private Vector2 moveInput;
    private Rigidbody rb;
    private Camera mainCamera;
    private bool isRushing;
    private float yRotation;

    private void Awake()
    {
        inputActions = new PlayerInput();
        inputActions.GameMode.AddCallbacks(this);
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
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

    private void Update()
    {
        RotateTowardsMouse();
    }

    private void FixedUpdate()
    {
        Move();
    }
    private void Move()
    {
        float currentSpeed = isRushing ? moveSpeed * sprintMultiplier : moveSpeed;
        Vector3 direction = transform.forward * moveInput.y + transform.right * moveInput.x;
        Vector3 movement = direction * currentSpeed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
    }
    private void RotateTowardsMouse()
    {
        float mouseX = Mouse.current.delta.x.ReadValue();
        yRotation += mouseX * mouseSensitivity;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
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

    public void OnIsRushing(InputAction.CallbackContext context)
    {
        isRushing = context.ReadValue<float>() > 0;
    }
}
