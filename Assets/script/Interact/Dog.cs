using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum DogState
{
    Active,
    GoingToDoor, 
    Leashed  
}

public class Dog : Interact
{
    [Header("狗狗配置")]
    [SerializeField] private door door;
    [SerializeField] private Transform doorApproachPoint;
    [SerializeField] private float doorReachDistance = 1f;

    [Header("拴住状态")]
    [SerializeField] private Transform leashedPoint;

    [Header("广告牌精灵")]
    [SerializeField] private Transform spriteTransform; // 拖入狗的精灵子物体

    private NavMeshAgent agent;
    private DogState currentState = DogState.Active;
    private bool hasHelpedThisRound = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        agent.updateRotation = false; // 贴图精灵不旋转
        agent.updateUpAxis = false;
    }

    public override bool InteractObject(GameObject item)
    {
        if (!Interactable) return false;
        if (RuleSystem.Instance.IsRuleActive("AskHelpFromDog"))
        {
            var rule = RuleSystem.Instance.GetRule("AskHelpFromDog");
            rule.OnRuleViolated(gameObject);
            return true;
        }

        if (hasHelpedThisRound)
        {
            Debug.Log("狗狗本轮已经帮过忙了");
            return true;
        }
        hasHelpedThisRound = true;
        RuleSystem.Instance.SetPending("AskHelpFromDog");
        SetState(DogState.GoingToDoor);
        Debug.Log("狗狗出发去开门");
        return true;
    }

    private void Update()
    {
        if (currentState == DogState.GoingToDoor && agent.enabled && agent.isOnNavMesh)
        {
            if (!agent.pathPending && agent.remainingDistance <= doorReachDistance)
            {
                if (door != null)
                {
                    door.Open();
                    Debug.Log("狗狗帮忙开了门");
                }
                SetState(DogState.Leashed);
            }
        }

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

    private void SetState(DogState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case DogState.Active:
                agent.enabled = false; // 自由状态原地待命
                break;

            case DogState.GoingToDoor:
                agent.enabled = true;
                if (doorApproachPoint != null)
                    agent.SetDestination(doorApproachPoint.position);
                else if (door != null)
                    agent.SetDestination(door.transform.position);
                break;

            case DogState.Leashed:
                agent.enabled = false;
                break;
        }
    }

    public override void Reset()
    {
        base.Reset();
        hasHelpedThisRound = false;
        agent.enabled = false;

        // 没帮过忙 → 回初始位置自由活动；帮过忙 → 拴住
        if (RuleSystem.Instance.IsRuleActive("AskHelpFromDog") && leashedPoint != null)
        {
            transform.position = leashedPoint.position;
            transform.rotation = leashedPoint.rotation;
            SetState(DogState.Leashed);
        }
        else
        {
            SetState(DogState.Active);
        }
    }
}
