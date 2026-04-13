using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Driver : MonoBehaviour
{
    [Header("目标点")]
    [SerializeField] public Transform targetPoint;

    [Header("移动组件")]
    private NavMeshAgent agent;
    [Header("掉落物")]
    [SerializeField] private GameObject icePrefab;

    [Header("停止距离")]
    [SerializeField] private float stopDistance = 1.0f;

    private bool isMoving = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    private void Start()
    {
        StartMove();
        StartCoroutine(SpawnIceAfterDelay(1f));
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
        Debug.Log("moving");
    }

    private void Update()
    {
        if((targetPoint.position-transform.position).magnitude < stopDistance)
        {
            Debug.Log("arrive");
            Destroy(gameObject);
        }
        if (!isMoving) return;
        if (targetPoint == null) return;
        if (!agent.isOnNavMesh) return;

        // 等路径计算完成
        if (agent.pathPending) return;

        // 如果没有路径，直接退出
        if (!agent.hasPath) return;

        // 核心：用“速度 + 距离”判断
        float speed = agent.velocity.magnitude;

        float distance = Vector3.Distance(transform.position, targetPoint.position);

        if (speed < 0.05f && distance <= stopDistance)
        {
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

        Debug.Log("?!");
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

    private IEnumerator SpawnIceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (icePrefab == null) yield break;

        Vector3 pos = transform.position;

        Instantiate(icePrefab, pos, Quaternion.identity);

        Debug.Log("Ice dropped!");
    }
}