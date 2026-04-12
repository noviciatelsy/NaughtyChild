using DG.Tweening;
using UnityEngine;

public class HeadBounce : MonoBehaviour
{
    public Transform childtransform;
    public NPCchild npcchild;
    [SerializeField] private float bounceForce = 20f;
    [SerializeField] private float squashDuration = 0.3f;
    [SerializeField] private float squashAmount = 0.6f; // 越小越扁

    private void OnTriggerEnter(Collider other)
    {
        // 只处理玩家
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        playermovement pm = other.GetComponent<playermovement>();
        if (pm == null) return;
        // 关键：必须是“跳跃状态”
        //!pm.IsGround
        if (other.transform.position.y<0.65f)
        {
            Debug.Log("not jump!");
            return;
        }
        npcchild.NPChead();
        // 清除原有Y速度
        Vector3 v = rb.velocity;
        v.y = 0;

        // 添加向上弹力
        rb.velocity = v;
        rb.AddForce(Vector3.up * bounceForce, ForceMode.VelocityChange);

        // ======================
        // 2. 小孩压扁动画
        // ======================
        Transform child = childtransform; // 被踩的对象（小孩）
        child.DOKill();

        Vector3 originalScale = child.localScale;
        Vector3 squashedScale = new Vector3(
            originalScale.x,
            originalScale.y * squashAmount,
            originalScale.z
        );

        Sequence seq = DOTween.Sequence();

        seq.Append(child.DOScale(squashedScale, squashDuration * 0.5f)
            .SetEase(Ease.OutQuad));

        seq.Append(child.DOScale(originalScale, squashDuration * 0.5f)
            .SetEase(Ease.OutBounce));

        Debug.Log("踩头弹跳！");
    }
}