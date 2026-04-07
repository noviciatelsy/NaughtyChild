using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class Throwable : MonoBehaviour
{
    private Rigidbody rb;
    private LineRenderer lr;
    [SerializeField] private float mass = 1f;

    private Vector3 cachedVelocity;
    private Collider[] playerColliders;
    [Header("轨迹参数")]
    [SerializeField] private int resolution = 150;
    [SerializeField] private float timeStep = 0.02f;

    private Transform handPoint;
    private bool isHeld = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lr = GetComponent<LineRenderer>(); //缺这个
        lr.enabled = false;
        SetupLineColor();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerColliders = player.GetComponentsInChildren<Collider>();
        }
    }

    private void Update()
    {
        if (!isHeld)
        {
            if (lr.enabled)
                lr.enabled = false;
            return;
        }

        DrawTrajectory(); //核心
    }

    public void OnPicked(Transform hand)
    {
        handPoint = hand;
        isHeld = true;

        rb.isKinematic = true;
        rb.useGravity = false;

        Collider selfCol = GetComponent<Collider>();

        if (playerColliders != null)
        {
            foreach (var col in playerColliders)
            {
                Physics.IgnoreCollision(selfCol, col, true);
            }
        }

        GetComponent<Collider>().enabled = false;
    }

    public void OnThrow(Vector3 dir)
    {
        isHeld = false;
        lr.enabled = false;
        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;
        GetComponent<Collider>().enabled = true;

        Collider selfCol = GetComponent<Collider>();
        if (playerColliders != null)
        {
            foreach (var col in playerColliders)
            {
                Physics.IgnoreCollision(selfCol, col, true);
            }
        }

        rb.velocity = cachedVelocity;
    }

    public virtual void OnUse(Interact target)
    {
        Debug.Log("用物体 interacting: " + target.name);

        // 默认行为：触发目标的交互
        target.InteractObject();
    }


    private void DrawTrajectory()
    {
        if (!GetMouseHitPoint(out Vector3 targetPos1))
            return;

        lr.enabled = true;
        lr.positionCount = resolution;

        Vector3 startPos = handPoint.position;
        Vector3 targetPos = Vector3.Lerp(
            startPos,
            targetPos1,
            1f / mass
        );
        float totalTime = resolution * timeStep;
        totalTime = GetThrowTime(startPos, targetPos);
        // 关键：反推初速度（保证落点=鼠标点）
        cachedVelocity = CalculateInitialVelocity(startPos, targetPos, totalTime);
        Vector3 v0 = cachedVelocity;

        Vector3 prevPos = startPos;

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timeStep;

            Vector3 pos =
                startPos +
                v0 * t +
                0.5f * Physics.gravity * t * t;

            // 碰撞检测
            if (i > 0)
            {
                Vector3 dir = pos - prevPos;
                float dist = dir.magnitude;

                if (Physics.Raycast(prevPos, dir.normalized, out RaycastHit hit, dist))
                {
                    if (hit.transform != transform &&
                        !hit.transform.CompareTag("Player"))
                    {
                        lr.positionCount = i + 1;
                        lr.SetPosition(i, hit.point);
                        break;
                    }
                }
            }

            lr.SetPosition(i, pos);
            prevPos = pos;
        }
    }

    private bool GetMouseHitPoint(out Vector3 point)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            point = hit.point;
            return true;
        }

        // 没打到物体 → 给一个远点
        point = ray.origin + ray.direction * 20f;
        return false;
    }
    private Vector3 CalculateInitialVelocity(Vector3 start, Vector3 target, float t)
    {
        return (target - start - 0.5f * Physics.gravity * t * t) / t;
    }
    private float GetThrowTime(Vector3 start, Vector3 target)
    {
        float distance = Vector3.Distance(start, target);

        // ⭐关键：控制投掷节奏（你可以调这个）
        float minTime = 0.2f;
        float maxTime = 3.0f;

        // 距离归一化
        float t = Mathf.Clamp(distance / 15f, 0f, 1f);

        return Mathf.Lerp(minTime, maxTime, t);
    }

    private void SetupLineColor()
    {
        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new GradientColorKey[]
            {
            new GradientColorKey(Color.white, 0f),   // 起点颜色
            new GradientColorKey(Color.cyan, 1f)     // 终点颜色
            },
            new GradientAlphaKey[]
            {
            new GradientAlphaKey(1f, 0f),   // 起点不透明
            new GradientAlphaKey(0f, 1f)    // 终点透明
            }
        );

        lr.colorGradient = gradient;
    }
}