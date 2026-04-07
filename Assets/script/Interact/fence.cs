using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fence : Interact
{
    private bool isBroken = false;
    [Header("掉落物")]
    [SerializeField] private GameObject woodPrefab;
    [SerializeField] private int woodCount = 2;
    private List<GameObject> spawnedWoods = new List<GameObject>();

    protected override void OnInteracted(GameObject item)
    {
        if (isBroken) return;
        //判断是否是 axe
        if (item != null && item.GetComponent<axe>() != null)
        {
            TriggerRuleSystem("DontDestroyfence");
            if (RuleSystem.Instance.IsRuleActive("DontDestroyfence")) return;
            Debug.Log("用斧头砍围栏");
            Breakfence();
        }

        // 默认逻辑
        Debug.Log("普通交互围栏");
    }

    private void Breakfence()
    {
        isBroken = true;

        // 不Destroy，而是隐藏
        col.enabled = false;
        rend.enabled = false;

        SpawnWoods();

        Debug.Log("围栏被破坏（掉落木板）");
    }

    private void SpawnWoods()
    {
        for (int i = 0; i < woodCount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = Mathf.Abs(offset.y); // 往上散开一点

            GameObject wood = Instantiate(
                woodPrefab,
                transform.position + offset,
                Quaternion.identity
            );

            spawnedWoods.Add(wood); // 记录！
        }
    }

    public override void Reset()
    {
        base.Reset();
        isBroken = false;

        // 删除所有生成的木板
        foreach (var wood in spawnedWoods)
        {
            if (wood != null)
                Destroy(wood);
        }

        spawnedWoods.Clear();
    }
}
