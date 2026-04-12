using UnityEngine;

public class ShadowFollower : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float heightOffset = 0.2f;
    [SerializeField] private float rayDistance = 20f;

    [Header("检测参数")]
    [SerializeField] private float castDistance = 3f;
    [SerializeField] private float radius = 0.15f;

    [Header("缩放控制")]
    [SerializeField] private float minScale = 0.2f;
    [SerializeField] private float maxScale = 1f;
    [SerializeField] private float scaleHeightFactor = 5f; // 控制衰减速度

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 center = player.position + Vector3.up * 0.5f;

        RaycastHit hit;

        bool hasHit =
            Physics.SphereCast(center, radius, Vector3.down, out hit, castDistance, ~0, QueryTriggerInteraction.Ignore);

        if (!hasHit) return;

        //关键：验证这个面是否“真正可站立”
        float slope = Vector3.Dot(hit.normal, Vector3.up);

        if (slope < 0.6f)
            return;

        Vector3 pos = hit.point;
        pos.y += heightOffset;

        transform.position = new Vector3(
            player.position.x,
            pos.y,
            player.position.z
        );

        // =========================
        // 2. 计算高度
        // =========================
        float height = player.position.y - hit.point.y;
        float t = Mathf.Clamp01(height / scaleHeightFactor);
        float scale = Mathf.Lerp(maxScale, minScale, t);
        transform.localScale = Vector3.one * scale;
    }
}