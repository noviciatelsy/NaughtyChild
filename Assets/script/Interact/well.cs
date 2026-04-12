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

        if (usedThisRound || RuleSystem.Instance.IsRuleActive("DontUsewell"))
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
    }

    private void BreakWell()
    {
        isBroken = true;

        // 不Destroy，而是隐藏
        col.enabled = false;
        rend.enabled = false;

        SpawnWoods();

        Debug.Log("井盖被破坏（掉落木板）");
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
}