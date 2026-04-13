using System.Collections;
using UnityEngine;

public class SleepZEmitter : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject zPrefab;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 0.6f;
    [SerializeField] private float randomOffset = 0.3f;

    [Header("Offset")]
    [SerializeField] private Vector3 spawnArea = new Vector3(0.2f, 0.2f, 0.2f);

    private Coroutine routine;
    private bool isPlaying;

    // =========================
    // 外部接口：开始
    // =========================
    public void StartSleepEffect()
    {
        if (isPlaying) return;

        isPlaying = true;
        routine = StartCoroutine(SpawnLoop());
    }

    // =========================
    //  外部接口：停止
    // =========================
    public void StopSleepEffect()
    {
        isPlaying = false;

        if (routine != null)
            StopCoroutine(routine);
    }

    // =========================
    // 生成循环
    // =========================
    private IEnumerator SpawnLoop()
    {
        while (isPlaying)
        {
            SpawnOne();

            float wait = spawnInterval + Random.Range(-randomOffset, randomOffset);
            yield return new WaitForSeconds(wait);
        }
    }

    // =========================
    //  生成一个 Z
    // =========================
    private void SpawnOne()
    {
        if (zPrefab == null || spawnPoint == null)
            return;

        Vector3 offset = new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x),
            Random.Range(0f, spawnArea.y),
            Random.Range(-spawnArea.z, spawnArea.z)
        );

        Vector3 pos = spawnPoint.position + offset;

        GameObject obj = Instantiate(zPrefab, pos, Quaternion.identity);

        var fx = obj.GetComponent<SleepZFloat>();
        if (fx != null)
            fx.Play();
    }
}