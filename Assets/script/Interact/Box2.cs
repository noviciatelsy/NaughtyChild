using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box2 : Interact
{
    private bool isBroken = false;
    [Header("掉落物")]
    [SerializeField] private GameObject woodPrefab;
    [SerializeField] private int woodCount = 5;
    private List<GameObject> spawnedWoods = new List<GameObject>();
    private Renderer[] allRenderers;
    private void Awake()
    {
        // 获取所有子物体 Renderer（包括 SpriteRenderer）
        allRenderers = GetComponentsInChildren<Renderer>(true);
    }


    public override bool InteractObject(GameObject item)
    {
        // =========================
        // 1. axe 逻辑（你已有）
        // =========================
        if (item != null && item.GetComponent<axe>() != null)
        {
            Debug.Log("用斧头砍箱子");
            BreakBox();
            return true;
        }
        else { 
            OnEmptyClick();
            return true;
        }

        return false;
    }

    private void OnEmptyClick()
    {
        if (isBroken) return;

        Debug.Log("空手点击箱子");

        Function1();
    }
    private void Function1()
    {
        Debug.Log("执行函数1：箱子被空手检查/敲击/互动");

        // 示例行为（你可以改）
        // 1. 播放动画
        // 2. UI提示
        // 3. 轻微抖动
    }

    private void BreakBox()
    {
        isBroken = true;

        // 不Destroy，而是隐藏
        col.enabled = false;
        //rend.enabled = false;
        foreach (var r in allRenderers)
        {
            r.enabled = false;
        }

        SpawnWoods();

        Debug.Log("储物箱被破坏（掉落木板）");
    }

    private void SpawnWoods()
    {
        float radius = 0.5f;

        for (int i = 0; i < woodCount; i++)
        {
            float angle = i * Mathf.PI * 2 / woodCount;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle),
                Random.Range(0.1f, 1.5f),
                Mathf.Sin(angle)
            ) * radius;

            GameObject wood = Instantiate(
                woodPrefab,
                transform.position + offset,
                Quaternion.identity
            );

            spawnedWoods.Add(wood);
        }
    }

    public override void Reset()
    {
        base.Reset();
        isBroken = false;
        foreach (var r in allRenderers)
        {
            if (r != null)
                r.enabled = true;
        }

        // 删除所有生成的木板
        foreach (var wood in spawnedWoods)
        {
            if (wood != null)
                Destroy(wood);
        }

        spawnedWoods.Clear();
    }

    public void BreakByCar()
    {
        if (isBroken) return;

        Debug.Log("车撞箱子！");

        BreakBox();
    }


}
