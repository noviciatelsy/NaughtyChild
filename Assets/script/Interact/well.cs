using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class well : Interact
{
    [Header("传送目标点")]
    public Transform transpoint;

    protected override void OnInteracted(GameObject item)
    {
        if(RuleSystem.Instance.IsRuleActive("DontUsewell"))
        {
            Debug.Log("规则禁止使用井");
            return;
        }
        TransportPlayer();

        Debug.Log("玩家被传送");
    }

    private void TransportPlayer()
    {
        TriggerRuleSystem("DontUsewell");
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
}