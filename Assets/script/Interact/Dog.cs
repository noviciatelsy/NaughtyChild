using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms.Impl;

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

    [SerializeField] private SleepZEmitter sleepFx;
    [SerializeField] private FloatPopEmitter appleFx;
    [SerializeField] private Sprite appleSprite;
    private NavMeshAgent agent;

    // =========================
    // 核心状态
    // =========================
    private bool isLeashed;
    private bool isSleep;
    private bool isResetting;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    // 行为状态
    private DogActionState actionState = DogActionState.Idle;

    private float wanderTimer;
    private Vector3 wanderCenter;
    [Header("成就配置")]
    [SerializeField] private AchievementSO achievement;
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

            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;

            lineRenderer.enabled = false;
        }
    }

    private void Start()
    {
        wanderCenter = transform.position;
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
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
        isLeashed = RuleSystem.Instance.IsRuleActive("askdogforhelp1")
                 || RuleSystem.Instance.IsRuleActive("askdogforhelp2")
                 || RuleSystem.Instance.IsRuleActive("askdogforhelp3");

        isSleep = RuleSystem.Instance.IsRuleActive("askdogforhelp2")
               || RuleSystem.Instance.IsRuleActive("askdogforhelp3");

        actionState = DogActionState.Idle;
    }

    // ======================================================
    // 交互逻辑（核心）
    // ======================================================
    protected override bool OnInteracted(GameObject item)
    {
        if (appleFx != null)
            appleFx.Play(appleSprite);
        SoundManager.Instance.PlaySFX("狗叫2");
        if (!Interactable) return false;

        if (actionState == DogActionState.GoingToDoor)
            return false;

        bool handled = false;

        // =========================
        // 处理 leash
        // =========================
        if (isLeashed && item != null && item.GetComponent<axe>() != null)
        {
            isLeashed = false;
            Debug.Log("解除 leash");
            handled = true;
        }

        // =========================
        //  处理 sleep
        // =========================
        if (isSleep && item != null && item.GetComponent<apple>() != null)
        {
            isSleep = false;
            if (appleFx != null)
                appleFx.Play(appleSprite);
            Debug.Log("解除 sleep");
            handled = true;
        }

        // 如果用了道具，直接返回
        if (handled)
            return true;

        // =========================
        // 状态阻断
        // =========================
        if (isSleep || isLeashed)
            return false;

        // =========================
        // 正常行为
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
        if (RuleSystem.Instance.IsRuleActive("askdogforhelp1")
                 && RuleSystem.Instance.IsRuleActive("askdogforhelp2"))
        {
            if (achievement != null && AchievementManager.Instance != null)
            {
                Debug.LogError("12345");
                AchievementManager.Instance.RecordAction(achievement.achievementName);
            }
        }
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
        if (isResetting) return;
        FaceCamera();

        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        // =========================
        //  去开门逻辑
        // =========================
        if (actionState == DogActionState.GoingToDoor)
        {
            if (!agent.pathPending && agent.remainingDistance <= doorReachDistance)
            {
                if (door != null)
                    door.Open();

                actionState = DogActionState.Idle;
            }
            return;
        }

        // =========================
        // sleep 只限制“移动”，不 return
        // =========================
        if (isSleep)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;

            sleepFx.StartSleepEffect();
        }
        else
        {
            // =========================
            // movement logic
            // =========================
            if (isLeashed)
                HandleLeashedWander();
            else
                HandleWander();

            sleepFx.StopSleepEffect();
        }

        //  leash 视觉永远更新（不受sleep影响）
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
            if (achievement != null && AchievementManager.Instance != null)
            {
                Debug.LogError("12345");
                AchievementManager.Instance.RecordAction(achievement.achievementName);
            }
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
    // leash wander（限制范围）
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
        if (lineRenderer == null || leashedPoint == null)
            return;

        if (!isLeashed)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;

        lineRenderer.SetPosition(0, leashedPoint.position);
        lineRenderer.SetPosition(1, transform.position);
    }

    public override void Reset()
    {
        isResetting = true;

        StopAllCoroutines();

        // ===== NavMeshAgent 必须彻底清干净 =====
        if (agent != null && agent.enabled)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.isStopped = true;

            agent.Warp(wanderCenter); //关键：强制回出生点
        }

        // ===== transform 双保险 =====
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        // ===== 状态清理 =====
        isLeashed = false;
        isSleep = false;
        actionState = DogActionState.Idle;
        wanderTimer = 0f;

        // ===== 视觉 =====
        if (lineRenderer != null)
            lineRenderer.enabled = false;

        sleepFx?.StopSleepEffect();

        // ===== 等一帧再恢复AI =====
        StartCoroutine(ResumeAI());
    }
    private IEnumerator ResumeAI()
    {
        yield return null; // 等1帧，让GameManager Reset先执行完

        if (agent != null)
            agent.isStopped = false;

        isResetting = false;
    }
}