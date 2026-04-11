using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tree : Interact
{
    [Header("掉落物")]
    [SerializeField] private GameObject ApplePrefab;   // 第一次掉落
    [SerializeField] private GameObject boardPrefab; // 最终掉落
    [SerializeField] private int boardCount = 3;

    [Header("倒下位置")]
    [SerializeField] private Transform fallenTransform;

    private int hitCount = 0;
    private bool isFallen = false;
    private bool isDestroyed = false;
    private Coroutine fallCoroutine;

    private List<GameObject> spawnedItems = new List<GameObject>();
    private Renderer[] allRenderers;
    private void Awake()
    {
        allRenderers = GetComponentsInChildren<Renderer>(true); // true = 包含inactive
    }

    protected override bool OnInteracted(GameObject item)
    {
        if (isDestroyed) return false;

        // 1. 必须是斧子
        if (item == null || item.GetComponent<axe>() == null)
        {
            Debug.Log("不是斧子，无法砍树");
            return false;
        }

        hitCount++;
        Debug.Log($"砍树次数：{hitCount}");

        // 第一次砍：倒下 + 掉落1个log
        if (hitCount == 1)
        {
            FallTree();
            SpawnApple();
        }
        // 2~4次：只计数
        else if (hitCount < 5)
        {
            Debug.Log("树被继续砍中...");
        }
        // 第5次：消失 + 掉落board
        else if (hitCount >= 5)
        {
            DestroyTree();
        }
        return true;
    }

    private void FallTree()
    {
        if (isFallen) return;

        isFallen = true;

        if (fallCoroutine != null)
            StopCoroutine(fallCoroutine);

        fallCoroutine = StartCoroutine(FallTreeRoutine());


        Debug.Log("树倒下了");
    }
    private IEnumerator FallTreeRoutine()
    {
        float duration = 0.3f;
        float t = 0f;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 targetPos = fallenTransform != null ? fallenTransform.position : startPos;
        Quaternion targetRot = fallenTransform != null ? fallenTransform.rotation : startRot;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;

            transform.position = Vector3.Lerp(startPos, targetPos, lerp);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, lerp);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        fallCoroutine = null;
    }

    private void SpawnApple()
    {
        if (ApplePrefab == null) return;

        GameObject log = Instantiate(
            ApplePrefab,
            transform.position + Vector3.up * 2.5f,
            Quaternion.identity
        );

        spawnedItems.Add(log);
    }

    private void DestroyTree()
    {
        isDestroyed = true;

        // 隐藏本体
        col.enabled = false;
        rend.enabled = false;
        SetRenderers(false);
        SpawnBoards();

        Debug.Log("树被砍倒并完全消失");
    }

    private void SpawnBoards()
    {
        if (boardPrefab == null) return;

        for (int i = 0; i < boardCount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = Mathf.Abs(offset.y);

            GameObject board = Instantiate(
                boardPrefab,
                transform.position + offset,
                Quaternion.identity
            );

            spawnedItems.Add(board);
        }
    }

    public override void Reset()
    {
        base.Reset();
        SetRenderers(true);
        hitCount = 0;
        isFallen = false;
        isDestroyed = false;

        foreach (var obj in spawnedItems)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedItems.Clear();
    }

    private void SetRenderers(bool state)
    {
        foreach (var r in allRenderers)
        {
            if (r != null)
                r.enabled = state;
        }
    }
}
