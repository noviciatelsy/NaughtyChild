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
    private Vector3 moveVelocity;

    [Header("跳跃参数")]
    private bool isjump = false;
    [SerializeField] private float jumpHeight = 2f;

    private bool isGrounded;

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

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.useGravity = true; // ✅关键：用Unity自带重力
        rb.isKinematic = false;
    }

    private void OnEnable() => inputActions.GameMode.Enable();
    private void OnDisable() => inputActions.GameMode.Disable();
    private void OnDestroy() => inputActions.Dispose();

    private void FixedUpdate()
    {
        CheckGround();

        Move();

        // 保持角色直立
        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }

    // =========================
    // 移动（完全保留你的逻辑）
    // =========================
    private void Move()
    {
        float currentSpeed = moveSpeed;

        if (isRushing && isGrounded)
            currentSpeed *= sprintMultiplier;

        Vector3 inputDir =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        if (inputDir.magnitude > 1f)
            inputDir.Normalize();
        if (!isGrounded && IsHittingWall(inputDir))
        {
            inputDir = Vector3.zero; //不再往墙推
        }

        Vector3 v = rb.velocity;

        v.x = inputDir.x * currentSpeed;
        v.z = inputDir.z * currentSpeed;

        rb.velocity = v;
    }

    // =========================
    // ⭐跳跃（最简稳定版）
    // =========================
    private void HandleJump()
    {
        if (!isGrounded) return;

        //transform.position += Vector3.up * 10;
        Vector3 v = rb.velocity;
        v.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
        rb.velocity = v;
    }

    

    // =========================
    // INPUT
    // =========================
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            HandleJump();
        }
    }

    public void OnIsRushing(InputAction.CallbackContext context)
    {
        isRushing = context.ReadValue<float>() > 0;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Debug.DrawRay(ray.origin, ray.direction * 30f, Color.green, 2f);
            if (Physics.Raycast(ray, out RaycastHit hit, 30f))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red, 2f);
                Debug.Log("interact:" + hit);
                Interact interactable = hit.collider.GetComponent<Interact>();
                if (interactable != null && interactable.Interactable)
                {
                    interactable.InteractObject();
                }
            }
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            0.65f
        );
    }

    private bool IsHittingWall(Vector3 dir)
    {
        if (dir == Vector3.zero) return false;

        Ray ray = new Ray(transform.position, dir);

        // 画射线（黄色）
        Debug.DrawRay(ray.origin, ray.direction * 0.55f, Color.yellow);

        if (Physics.Raycast(ray, out RaycastHit hit, 0.55f))
        {
            // 命中画红色
            Debug.DrawLine(ray.origin, hit.point, Color.red);

            Debug.Log("撞墙: " + hit.collider.name);

            return true;
        }

        return false;
    }
}