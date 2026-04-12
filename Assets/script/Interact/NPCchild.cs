using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NPCState
{
    Idle,
    GoingToDoor,
    Done
}

public class NPCchild : Interact
{
    [Header("NPC配置")]
    [SerializeField] private door door;
    [SerializeField] private Transform doorApproachPoint;
    [SerializeField] private float doorReachDistance = 1f;

    [Header("Idle游走")]
    [SerializeField] private float wanderRadius = 5f;     // 游走半径
    [SerializeField] private float wanderInterval = 4f;   // 每隔多久换目标
    private float wanderTimer;
    private Vector3 startPoint; // 初始中心点

    [Header("完成后位置")]
    [SerializeField] private Transform finishPoint;

    [Header("广告牌精灵")]
    [SerializeField] private Transform spriteTransform;

    private NavMeshAgent agent;
    private NPCState currentState = NPCState.Idle;
    private bool hasHelped = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Start()
    {
        startPoint = transform.position;
    }

    // 和 Dog 一样：交互直接触发状态
    protected override bool OnInteracted(GameObject item)
    {
        if (!Interactable) return false;

        if (hasHelped)
        {
            Debug.Log("NPC已经帮过了");
            return true;
        }

        // 关键：只有 axe 才触发
        if (item != null && item.GetComponent<axe>() != null)
        {
            Debug.Log("NPC接收到斧头，准备去开门");

            hasHelped = true;

            TriggerRuleSystem("DontAskNPCForHelp");
            if (RuleSystem.Instance.IsRuleActive("DontAskNPCForHelp"))
                return true;

            SetState(NPCState.GoingToDoor);
        }
        else
        {
            Debug.Log("NPC不理你：没有斧头");
            return false;
        }
        return false;
    }

    private void Update()
    {
        if (currentState == NPCState.Idle)
        {
            HandleIdleWander();
        }

        if (currentState == NPCState.GoingToDoor && agent.enabled)
        {
            if (!agent.pathPending && agent.remainingDistance <= doorReachDistance)
            {
                if (door != null)
                {
                    door.Open();
                    Debug.Log("NPC帮忙开门");
                }

                SetState(NPCState.Done);
            }
        }

        // 广告牌
        // 精灵广告牌：始终面向摄像机
        if (spriteTransform != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 dir = cam.transform.position - spriteTransform.position;
                dir.y = 0; // 只水平旋转
                if (dir != Vector3.zero)
                    spriteTransform.rotation = Quaternion.LookRotation(-dir);
            }
        }
    }

    private void SetState(NPCState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case NPCState.Idle:
                agent.enabled = true;
                wanderTimer = wanderInterval; // 立刻选点
                break;

            case NPCState.GoingToDoor:
                agent.enabled = true;

                if (doorApproachPoint != null)
                    agent.SetDestination(doorApproachPoint.position);
                else if (door != null)
                    agent.SetDestination(door.transform.position);

                break;

            case NPCState.Done:
                agent.enabled = false;

                if (finishPoint != null)
                {
                    transform.position = finishPoint.position;
                    transform.rotation = finishPoint.rotation;
                }
                break;
        }
    }

    public override void Reset()
    {
        base.Reset();

        hasHelped = false;
        agent.enabled = false;

        SetState(NPCState.Idle);
    }

    private void HandleIdleWander()
    {
        if (!agent.enabled)
            agent.enabled = true;

        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderInterval)
        {
            Vector3 randomPos = GetRandomNavMeshPoint(startPoint, wanderRadius);

            agent.SetDestination(randomPos);

            wanderTimer = 0f;
        }
    }
    private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 10; i++) // 最多尝试10次
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // fallback（找不到就原地）
        return center;
    }
}