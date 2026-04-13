using UnityEngine;

public class FloatPopEmitter : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject effectPrefab;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    [Header("Random Offset")]
    [SerializeField] private Vector2 offset = new Vector2(0.2f, 0.2f);

    // =========================
    // 外部调用接口
    // =========================
    public void Play(Sprite sprite = null)
    {
        if (effectPrefab == null || spawnPoint == null)
            return;

        Vector3 random = new Vector3(
            Random.Range(-offset.x, offset.x),
            Random.Range(0f, offset.y),
            0f
        );

        GameObject obj = Instantiate(effectPrefab, spawnPoint.position + random, Quaternion.identity);

        var fx = obj.GetComponent<FloatPopEffect>();
        if (fx != null)
            fx.Init(sprite);
    }
}