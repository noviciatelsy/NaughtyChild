using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class well : Interact
{
    [Header("传送目标点")]
    public Transform transpoint;
    private bool isBroken = false;
    [Header("掉落物")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private int rockCount = 3;
    private List<GameObject> spawnedrocks = new List<GameObject>();
    private bool usedThisRound = false;

    [Header("旋转物体")]
    public Transform d1;
    [Header("井引用（外部调用）")]
    public well linkedWell;

    protected override bool OnInteracted(GameObject item)
    {
        if (isBroken) return false;
        //判断是否是 axe
        if (item != null && item.GetComponent<axe>() != null)
        {
            Debug.Log("用斧头砍井盖?");

            BreakWell();
            return true;
        }

        if (RuleSystem.Instance.IsRuleActive("DontUsewell"))
        {
            TriggerRuleSystem("DontUsewell");
            Debug.Log("规则禁止使用井");
            return true;
        }

        usedThisRound = true;
        RuleSystem.Instance.SetPending("DontUsewell");
        TransportPlayer();

        Debug.Log("玩家被传送");
        return true;
    }

    private void TransportPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("没找到Player");
            return;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // 关键：用 Rigidbody 移动
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.position = transpoint.position;
            rb.rotation = transpoint.rotation;
        }
        else
        {
            // fallback（不推荐）
            player.transform.position = transpoint.position;
            player.transform.rotation = transpoint.rotation;
        }

        if (linkedWell != null)
        {
            linkedWell.OnPlayerTeleported();
        }
    }

    private void BreakWell()
    {
        isBroken = true;

        // 不Destroy，而是隐藏
        col.enabled = false;
        rend.enabled = false;

        SpawnWoods();

        Debug.Log("井盖被破坏（掉落石板）");
    }

    private void SpawnWoods()
    {
        for (int i = 0; i < rockCount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = Mathf.Abs(offset.y); // 往上散开一点

            GameObject wood = Instantiate(
                rockPrefab,
                transform.position + offset,
                Quaternion.identity
            );

            spawnedrocks.Add(wood); // 记录！
        }
    }

    public override void Reset()
    {
        base.Reset();
        isBroken = false;
        usedThisRound = false;

        // 删除所有生成的木板
        foreach (var wood in spawnedrocks)
        {
            if (wood != null)
                Destroy(wood);
        }

        spawnedrocks.Clear();
    }

    private IEnumerator RotateD1()
    {
        if (d1 == null) yield break;

        float t = 0f;
        float duration = 0.35f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;

            float z;

            // 前半段：0 → 180
            if (k < 0.5f)
            {
                float kk = k / 0.5f;
                z = Mathf.Lerp(0f, 180f, kk);
            }
            // 后半段：180 → 0
            else
            {
                float kk = (k - 0.5f) / 0.5f;
                z = Mathf.Lerp(180f, 0f, kk);
            }

            Vector3 euler = d1.localEulerAngles;
            euler.z = z;
            d1.localEulerAngles = euler;

            yield return null;
        }

        // 确保归位
        Vector3 finalRot = d1.localEulerAngles;
        finalRot.z = 0f;
        d1.localEulerAngles = finalRot;
    }

    public void OnPlayerTeleported()
    {
        // 触发旋转
        StartCoroutine(RotateD1());

        Debug.Log("井检测到玩家传送，触发效果");
    }
}