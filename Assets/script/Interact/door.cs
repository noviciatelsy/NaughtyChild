using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : Interact
{
    private bool isOpening = false;

    private bool isBroken = false;
    [Header("掉落物")]
    [SerializeField] private GameObject woodPrefab;
    [SerializeField] private int woodCount = 2;
    private List<GameObject> spawnedWoods = new List<GameObject>();
    public override void InteractObject(GameObject item)
    {
        if (isBroken || isOpening) return;
        //判断是否是 axe
        if (item != null && item.GetComponent<axe>() != null)
        {
            TriggerRuleSystem("DontDestroydoor");
            if (RuleSystem.Instance.IsRuleActive("DontDestroydoor")) return;

            Debug.Log("用斧头砍门");
            Breakfence();
            return;
        }

        // 默认逻辑
        Debug.Log("普通交互围栏");
        base.InteractObject(item);
        Openthedoor();
    }


    public void Openthedoor()
    {
        TriggerRuleSystem("DontJustOpendoor");
        if (!isOpening && !RuleSystem.Instance.IsRuleActive("DontJustOpendoor") )
        {
            StartCoroutine(OpenDoor());
        }
    }

    public override void Reset()
    {
        StopAllCoroutines();
        base.Reset();
        isOpening = false;
        isBroken = false;

        // 删除所有生成的木板
        foreach (var wood in spawnedWoods)
        {
            if (wood != null)
                Destroy(wood);
        }
        spawnedWoods.Clear();

        Quaternion targetRot = Quaternion.Euler(0f, 0f, 0f);
        transform.rotation = targetRot;
    }

    private IEnumerator OpenDoor()
    {
        isOpening = true;

        float duration = 0.5f;
        float time = 0f;

        // 初始状态
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // 目标状态
        //Vector3 targetPos = startPos + new Vector3(-1f, 0f, 1f);
        Vector3 targetPos = startPos;
        Quaternion targetRot = Quaternion.Euler(0f, -90f, 0f);

        while (time < duration)
        {
            if (!isOpening)
                yield break;

            time += Time.deltaTime;
            float t = time / duration;

            // 插值（平滑）
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        // 最终强制对齐（防止误差）
        transform.position = targetPos;
        transform.rotation = targetRot;
        if (!isOpening)
        {
            transform.position = startPos;
            transform.rotation = startRot;
        }
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

}
