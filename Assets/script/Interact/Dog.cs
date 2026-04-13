using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum DogActionState
{
    Idle,
    GoingToDoor
}

public class Dog : Interact
{
    [Header("Config")]
    [SerializeField] private door door;
    [SerializeField] private Transform doorApproachPoint;
    [SerializeField] private float doorReachDistance = 1f;

    [Header("Wander")]
    [SerializeField] private float wanderRadius = 4f;
    [SerializeField] private float wanderInterval = 2f;

    [Header("Leash")]
    [SerializeField] private Transform leashedPoint;

    [Header("Visual")]
    [SerializeField] private Transform spriteTransform;
    [Header("Leash Visual")]
    [SerializeField] private LineRenderer lineRenderer;

    private NavMeshAgent agent;

    // =========================
    // 核心状态
    // =========================
    private bool isLeashed;
    private bool isSleep;

    // 行为状态
    private DogActionState actionState = DogActionState.Idle;

    private float wanderTimer;
    private Vector3 wanderCenter;

    // ======================================================
    // 初始化
    // ======================================================
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // 初始化线
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.startColor = Color.black;
            lineRenderer.endColor = Color.black;
            lineRenderer.enabled = false;
        }
    }

    private void Start()
    {
        wanderCenter = transform.position;

        //SyncStageFromRules();
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForGameManager());
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundStarted -= OnRoundStart;
    }

    private void OnRoundStart(int _)
    {
        SyncStageFromRules();
    }

    // ======================================================
    //  回合同步（唯一状态入口）
    // ======================================================
    private void SyncStageFromRules()
    {
        bool r3 = RuleSystem.Instance.IsRuleActive("askdogforhelp3");
        bool r2 = RuleSystem.Instance.IsRuleActive("askdogforhelp2");
        bool r1 = RuleSystem.Instance.IsRuleActive("askdogforhelp1");

        if (r3)
        {
            isLeashed = true;
            isSleep = true;
        }
        else if (r2)
        {
            isLeashed = true;
            isSleep = true;
        }
        else if (r1)
        {
            isLeashed = true;
            isSleep = false;
        }
        else
        {
            isLeashed = false;
            isSleep = false;
        }

        actionState = DogActionState.Idle;
    }

    // ======================================================
    // 交互逻辑（核心）
    // ======================================================
    protected override bool OnInteracted(GameObject item)
    {
        if (!Interactable) return false;

        //  正在执行行为时禁止重复点击
        if (actionState == DogActionState.GoingToDoor)
            return false;

        // =========================
        // 1️ 睡眠状态（最高优先级）
        // =========================
        if (isSleep)
        {
            if (item != null && item.GetComponent<apple>() != null)
            {
                isSleep = false;
                Debug.Log("解除 sleep");
                return true;
            }

            return false;
        }

        // =========================
        // 2️ leash 状态
        // =========================
        if (isLeashed)
        {
            if (item != null && item.GetComponent<axe>() != null)
            {
                isLeashed = false;
                Debug.Log("解除 leash");
                return true;
            }
            return false;
        }

        // =========================
        // 3️正常点击 → 开门 + 推规则
        // =========================
        StartGoToDoor();
        TriggerRules();

        return true;
    }

    // ======================================================
    //  去开门
    // ======================================================
    private void StartGoToDoor()
    {
        actionState = DogActionState.GoingToDoor;

        agent.speed = 4f;

        if (doorApproachPoint != null)
            agent.SetDestination(doorApproachPoint.position);
        else if (door != null)
            agent.SetDestination(door.transform.position);
    }

    // ======================================================
    // Update
    // ======================================================
    private void Update()
    {
        FaceCamera();

        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        // =========================
        // 去开门逻辑
        // =========================
        if (actionState == DogActionState.GoingToDoor)
        {
            if (!agent.pathPending && agent.remainingDistance <= doorReachDistance)
            {
                if (door != null)
                {
                    door.Open();
                    Debug.Log("狗狗开门");
                }

                actionState = DogActionState.Idle;
            }

            return; // ❗阻断其他行为
        }

        // =========================
        // 😴 睡眠
        // =========================
        if (isSleep)
        {
            agent.ResetPath();
            return;
        }

        // =========================
        // 🚶 wander
        // =========================
        if (isLeashed)
            HandleLeashedWander();
        else
            HandleWander();

        UpdateLeashLine();
    }

    // ======================================================
    //  规则推进（关键修复点）
    // ======================================================
    private void TriggerRules()
    {
        // 没有1 → 推1
        if (!RuleSystem.Instance.IsRuleActive("askdogforhelp1"))
        {
            TriggerRuleSystem("askdogforhelp1");
            return;
        }

        // 有1 → 推2
        if (!RuleSystem.Instance.IsRuleActive("askdogforhelp2"))
        {
            TriggerRuleSystem("askdogforhelp2");
            return;
        }

        // 有2 → 推3
        if (!RuleSystem.Instance.IsRuleActive("askdogforhelp3"))
        {
            TriggerRuleSystem("askdogforhelp3");
            return;
        }
        else
        {
            TriggerRuleSystem("askdogforhelp3");
        }
    }

    // ======================================================
    //  wander（自由）
    // ======================================================
    private void HandleWander()
    {
        wanderTimer += Time.deltaTime;
        if (wanderTimer < wanderInterval) return;

        wanderTimer = 0f;
        agent.speed = 2.5f;

        agent.SetDestination(RandomPoint(wanderCenter, wanderRadius));
    }

    // ======================================================
    // 🔗leash wander（限制范围）
    // ======================================================
    private void HandleLeashedWander()
    {
        wanderTimer += Time.deltaTime;
        if (wanderTimer < wanderInterval) return;

        wanderTimer = 0f;
        agent.speed = 2f;

        Vector3 center = leashedPoint != null ? leashedPoint.position : transform.position;
        Vector2 r = Random.insideUnitCircle * 2f;

        Vector3 target = new Vector3(center.x + r.x, transform.position.y, center.z + r.y);

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination(center);
    }

    // ======================================================
    //  随机点
    // ======================================================
    private Vector3 RandomPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 p = center + Random.insideUnitSphere * radius;

            if (NavMesh.SamplePosition(p, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }

        return center;
    }

    // ======================================================
    //  朝向摄像机（2D朝向）
    // ======================================================
    private void FaceCamera()
    {
        if (spriteTransform == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 dir = cam.transform.position - spriteTransform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
            spriteTransform.rotation = Quaternion.LookRotation(-dir);
    }

    private IEnumerator WaitForGameManager()
    {
        // 等 GameManager 初始化
        yield return new WaitUntil(() => GameManager.Instance != null);

        GameManager.Instance.OnRoundStarted += OnRoundStart;
    }

    private void UpdateLeashLine()
    {
        if (lineRenderer == null) return;

        if (isLeashed && leashedPoint != null)
        {
            lineRenderer.enabled = true;

            lineRenderer.SetPosition(0, leashedPoint.position);
            lineRenderer.SetPosition(1, transform.position);
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }
}