using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class playermovement : MonoBehaviour, PlayerInput.IGameModeActions,PlayerInput.IUIModeActions
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [Range(0.1f, 2.5f)]
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float sprintMultiplier = 2f;
    private Vector3 moveVelocity;
    private float externalForceTimer = 0f;
    private float externalForceDuration = 0.3f;

    [Header("跳跃参数")]
    private bool isjump = false;
    public bool IsGround => isGrounded;
    [SerializeField] private float jumpHeight = 2f;

    [Header("视角拖动")]
    [SerializeField] private Transform cameraRoot; // 你要旋转的物?
    [SerializeField] private Transform playertextureroot; // 你要旋转的物?
    [SerializeField] private float dragSensitivity = 0.2f;
    private bool isDragging = false;
    private bool isGrounded;
    [SerializeField] private float holdThreshold = 0.5f; // 超过这个时间算长按
    private float mouseDownTime;

    private PlayerInput inputActions;
    private Vector2 moveInput;
    private Rigidbody rb;
    private Camera mainCamera;
    private bool isRushing;
    private float yRotation;
    private bool isLocked;

    [SerializeField] private GameObject playerVisual;
    [SerializeField] private GameObject carVisual;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private Collider carCollider;

    public enum PlayerState
    {
        Normal,
        Driving
    }
    private PlayerState currentState = PlayerState.Normal;

    private void Awake()
    {
        inputActions = new PlayerInput();
        inputActions.GameMode.AddCallbacks(this);

        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.useGravity = true; // 关键：用Unity自带重力
        rb.isKinematic = false;

        SetDrivingVisual(false);
    }

    private bool isBoardOpen;

    private void OnEnable() => inputActions.GameMode.Enable();
    private void OnDisable()
    {
        inputActions.GameMode.Disable();
        inputActions.UIMode.Disable();
    }
    private void OnDestroy() => inputActions.Dispose();

    private void Update()
    {
        HandleMouseDrag();
    }

    private void FixedUpdate()
    {
        CheckGround();

        Move();
        ApplyFallAcceleration();
        // 保持角色直立
        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }

    // =========================
    // 移动
    // =========================
    private void Move()
    {
        if (currentState == PlayerState.Normal)
        {
            Move_Normal();
        }
        else if (currentState == PlayerState.Driving)
        {
            Move_Car();
        }
    }

    private void Move_Normal()
    {
        if (isLocked)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        if (externalForceTimer > 0f)
        {
            externalForceTimer -= Time.fixedDeltaTime;
            return;
        }

        float currentSpeed = moveSpeed;

        if (isRushing && isGrounded)
            currentSpeed *= sprintMultiplier;
        // 只取摄像机Y轴角
        float y = cameraRoot.eulerAngles.y;

        // 用角度构造一个“水平旋转�?
        Quaternion yawRotation = Quaternion.Euler(0f, y, 0f);

        // 得到方向（完全水平）
        Vector3 forward = yawRotation * Vector3.forward;
        Vector3 right = yawRotation * Vector3.right;

        Vector3 inputDir =
            forward * moveInput.y +
            right * moveInput.x;

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

    private void Move_Car()
    {
        float currentSpeed = moveSpeed;

        if (isRushing && isGrounded)
            currentSpeed *= sprintMultiplier;
        // 只取摄像机Y轴角
        float y = cameraRoot.eulerAngles.y;

        // 用角度构造一个“水平旋转�?
        Quaternion yawRotation = Quaternion.Euler(0f, y, 0f);

        // 得到方向（完全水平）
        Vector3 forward = yawRotation * Vector3.forward;
        Vector3 right = yawRotation * Vector3.right;

        Vector3 inputDir =
            forward * moveInput.y +
            right * moveInput.x;

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
    // 跳跃（最简稳定版）
    // =========================
    private void HandleJump()
    {
        if (!isGrounded) return;
        isjump = true;

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
        if (!context.performed) return;

        if (currentState == PlayerState.Driving)
        {
            ExitCar(); // 
        }
        else
        {
            HandleJump(); // 正常跳跃
        }
    }

    public void OnIsRushing(InputAction.CallbackContext context)
    {
        isRushing = context.ReadValue<float>() > 0;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.canceled) return;
        float holdTime = Time.time - mouseDownTime;

        // 用时间判断，而不是 isLongPress
        if (holdTime > holdThreshold) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        Debug.DrawRay(ray.origin, ray.direction * 300f, Color.green, 2f);

        float sphereRadius = 0.3f; // 👉 可调（0.2~0.5手感最好）

        bool hasValidTarget = false;
        Interact interact = null;

        RaycastHit[] hits = Physics.SphereCastAll(ray, sphereRadius, 300f);

        Interact bestInteract = null;
        float bestScore = float.MinValue;

        foreach (var hit in hits)
        {
            Interact temp = hit.collider.GetComponentInParent<Interact>();
            if (temp == null || !temp.Interactable)
                continue;

            float distance = Vector3.Distance(transform.position, temp.transform.position);
            if (distance > 3f)
                continue;

            // 🎯 评分机制（核心）
            Vector3 toTarget = (temp.transform.position - ray.origin).normalized;
            float alignment = Vector3.Dot(ray.direction, toTarget); // 越接近1越准

            float score = alignment * 2f - distance * 0.2f;

            if (score > bestScore)
            {
                bestScore = score;
                bestInteract = temp;
            }

            Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
        }

        // 最终结果
        hasValidTarget = bestInteract != null;
        interact = bestInteract;

        // =========================
        // 行为逻辑
        // =========================

        if (PlayerHand.Instance.HasItem)
        {
            if (hasValidTarget)
            {
                // 有物�? + 点到交互�? �? 只交互，不扔
                PlayerHand.Instance.UseItem(interact.gameObject);
            }
            else
            {
                Vector3 throwDir = mainCamera.transform.forward;

                // 从鼠标发射射�?
                Ray ray1 = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (Physics.Raycast(ray1, out RaycastHit hit1, 300f))
                {
                    //  朝鼠标命中点方向
                    throwDir = (hit1.point - PlayerHand.Instance.transform.position).normalized;
                }

                PlayerHand.Instance.ThrowItem(throwDir);
            }
        }
        else
        {
            if (hasValidTarget)
            {
                // 空手 + 点到 �? 正常交互
                interact.InteractObject(gameObject);
            }
            // 空手 + 没点�? �? 什么都不做
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            0.65f
        );

        if (isGrounded)
        {
            isjump = false; //落地结束跳跃
        }
    }

    private bool IsHittingWall(Vector3 dir)
    {
        if (dir == Vector3.zero) return false;

        Ray ray = new Ray(transform.position, dir);

        // 画射线（黄色�?
        Debug.DrawRay(ray.origin, ray.direction * 0.55f, Color.yellow);

        if (Physics.Raycast(ray, out RaycastHit hit, 0.55f, ~0, QueryTriggerInteraction.Ignore))
        {
            // 命中画红�?
            Debug.DrawLine(ray.origin, hit.point, Color.red);

            Debug.Log("撞墙: " + hit.collider.name);

            return true;
        }

        return false;
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    private void HandleMouseDrag()
    {
        if (isLocked || isBoardOpen) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            mouseDownTime = Time.time;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            isDragging = false;

        if (!isDragging) return;

        float mouseX = Mouse.current.delta.ReadValue().x;

        float angle = -mouseX * dragSensitivity;
        Vector3 euler = cameraRoot.eulerAngles;
        euler.y -= angle;
        cameraRoot.eulerAngles = euler;
        playertextureroot.eulerAngles = euler;
    }

    public void OnShowRules(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            OpenBoard();
        }
        // 关闭由 UIMode 的 OnCloseBoard 处理
    }

    private void OpenBoard()
    {
        if (isBoardOpen) return;
        isBoardOpen = true;

        // 先移除 GameMode 回调，防止 Disable 时触发 canceled
        inputActions.GameMode.RemoveCallbacks(this);
        inputActions.GameMode.Disable();

        // 清除残留输入
        moveInput = Vector2.zero;

        inputActions.UIMode.AddCallbacks(this);
        inputActions.UIMode.Enable();

        GameManager.Instance.RequestShowRules(true);
    }

    private void CloseBoard()
    {
        if (!isBoardOpen) return;
        isBoardOpen = false;

        inputActions.UIMode.RemoveCallbacks(this);
        inputActions.UIMode.Disable();

        inputActions.GameMode.AddCallbacks(this);
        inputActions.GameMode.Enable();

        GameManager.Instance.RequestShowRules(false);
    }

    private void ApplyFallAcceleration()
    {
        if (rb.velocity.y < 0)
        {
            Vector3 v = rb.velocity;

            // 只增强下落速度（不影响上升）
            v += Vector3.up * Physics.gravity.y * 1.2f * Time.fixedDeltaTime;

            rb.velocity = v;
        }
    }

    public void ApplyExternalForce(Vector3 velocity)
    {
        rb.velocity = velocity; // 直接设置冲量（最稳定）
        externalForceTimer = externalForceDuration;
    }

    public void OnSwitchBoard(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameManager.Instance.RequestSwitchBoard();
        }
    }

    public void OnCloseBoard(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            CloseBoard();
        }
    }

    void SetDrivingVisual(bool driving)
    {
        playerVisual.SetActive(!driving);
        carVisual.SetActive(driving);

        playerCollider.enabled = !driving;
        carCollider.enabled = driving;
    }

    public car curtargetCar;
    public void EnterCar(car targetCar)
    {
        curtargetCar = targetCar;
        currentState = PlayerState.Driving;

        SetDrivingVisual(true);

        // 同步位置
        transform.position = targetCar.transform.position + new Vector3(0, -0.8f, 0);

        targetCar.gameObject.SetActive(false); //不要销毁
    }

    public void ExitCar()
    {
        currentState = PlayerState.Normal;

        SetDrivingVisual(false);

        curtargetCar.transform.position = transform.position + new Vector3(0,1.2f,0);   
        transform.position+=new Vector3(0,0,1.5f);
        curtargetCar.gameObject.SetActive(true);
        curtargetCar = null;
    }
}