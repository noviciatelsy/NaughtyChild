using UnityEngine;
using UnityEngine.AI;

public class Driver : MonoBehaviour
{
    [Header("目标点")]
    [SerializeField] private Transform targetPoint;

    [Header("移动组件")]
    private NavMeshAgent agent;

    [Header("停止距离")]
    [SerializeField] private float stopDistance = 0.2f;

    private bool isMoving = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        StartMove();
    }

    // =========================
    // 启动寻路
    // =========================
    public void StartMove()
    {
        if (targetPoint == null) return;

        isMoving = true;
        agent.isStopped = false;
        agent.SetDestination(targetPoint.position);
    }

    private void Update()
    {
        if (!isMoving) return;
        if (targetPoint == null) return;

        // 必须确保路径有效
        if (agent.pathPending) { Debug.Log("noo"); return; }
        if (!agent.hasPath) { Debug.Log("noo"); return; }
        if (agent.pathStatus != NavMeshPathStatus.PathComplete) { return; }

        float distance = agent.remainingDistance;

        if (distance <= stopDistance)
        {
            Debug.Log("Arrived");
            Arrive();
        }
    }

    // =========================
    // 到达终点
    // =========================
    private void Arrive()
    {
        isMoving = false;

        agent.isStopped = true;
        agent.ResetPath();

        gameObject.SetActive(false);
    }

    // =========================
    // 外部设置目标
    // =========================
    public void SetTarget(Transform newTarget)
    {
        targetPoint = newTarget;

        if (isActiveAndEnabled)
        {
            StartMove();
        }
    }
}