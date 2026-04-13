using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class car : Interact
{
    [Header("路径点")]
    private Vector3 pointA = new Vector3(18.5f, 1.1f, -4.5f);
    private Vector3 pointB = new Vector3(-8.5f, 1.1f, -4.5f);

    [Header("移动参数")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float reachDistance = 0.2f;

    [SerializeField] private float force = 15f;
    private Vector3 moveDir;

    [Header("碰撞弹簧形变")]
    [SerializeField] private float squashAmount = 0.85f;
    [SerializeField] private float stretchAmount = 1.15f;
    [SerializeField] private float squashDuration = 0.3f;

    [SerializeField] private bool canDrive = false;
    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    private void Start()
    {
        transform.position = pointA;

        // 计算方向（A → B）
        moveDir = (pointB - transform.position).normalized;
    }

    public override bool InteractObject(GameObject user)
    {
        if (!canDrive) return false;

        playermovement pm = user.GetComponent<playermovement>();
        if (pm == null) return false;

        pm.EnterCar(this);
        return true;
    }


    private void FixedUpdate()
    {
        Move();
        ClampHeight();
        CorrectRotation();
    }

    private void Move()
    {
        // 持续给速度
        moveDir = (pointB - transform.position).normalized;
        rb.velocity = moveDir * speed;
        Vector3 v = rb.velocity;
        v.y = 0;
        rb.velocity = v;

        // 到达终点 → 瞬移回A
        if (Vector3.Distance(transform.position, pointB) <= reachDistance)
        {
            transform.position = pointA;

            //防止残留速度造成抖动
            rb.velocity = Vector3.zero;
        }
    }

    public override void Reset()
    {
        base.Reset();

        transform.position = pointA;
        rb.velocity = Vector3.zero;
    }

    private void ClampHeight()
    {
        // 位置锁定
        Vector3 pos = rb.position;
        if (pos.y < 0.9f)
        {
            pos.y = 0.9f;
            rb.position = pos;
        }

    }


    [SerializeField] private float rotationRecoverSpeed = 100f;

    private void CorrectRotation()
    {
        Quaternion targetRot = Quaternion.identity;

        rb.rotation = Quaternion.RotateTowards(
            rb.rotation,
            targetRot,
            rotationRecoverSpeed * Time.fixedDeltaTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        // 计算方向：(-trigger + player + 向上偏移)
        Vector3 dir = (other.transform.position - transform.position + new Vector3(0, 1.2f, 0)).normalized;

        playermovement pm = other.GetComponent<playermovement>();
        if (pm == null) return;

        // 直接给“冲量速度”
        pm.ApplyExternalForce(dir * force);
        PlaySquashEffect();

        TriggerRuleSystem("touchcar");
        Debug.Log("触发击退！");
    }

    private void PlaySquashEffect()
    {
        Transform t = transform;

        t.DOKill(); // 防止叠加

        Vector3 original = t.localScale;

        Vector3 squash = new Vector3(
            original.x * 1.05f,     // 横向稍微撑开（更有弹性）
            original.y * squashAmount,
            original.z * 1.05f
        );

        Vector3 stretch = new Vector3(
            original.x * 0.95f,
            original.y * stretchAmount,
            original.z * 0.95f
        );

        Sequence seq = DOTween.Sequence();

        // 1️ 压扁
        seq.Append(t.DOScale(squash, squashDuration * 0.3f)
            .SetEase(Ease.OutQuad));

        // 2️ 反弹拉长
        seq.Append(t.DOScale(stretch, squashDuration * 0.3f)
            .SetEase(Ease.OutQuad));

        // 3️ 回到原始（带弹性）
        seq.Append(t.DOScale(original, squashDuration * 0.4f)
            .SetEase(Ease.OutBounce));
    }
}
