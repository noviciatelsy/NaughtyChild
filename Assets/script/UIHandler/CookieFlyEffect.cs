using UnityEngine;
using DG.Tweening;

/// <summary>
/// 将场景中的3D物体飞向Canvas上的UI图标，附带三维旋转动画。
/// 所有UI位置都会转换成相机前方的世界坐标，物体始终在相机可视范围内飞行。
/// midPoint和targetIcon都是Canvas中的RectTransform。
/// </summary>
public class CookieFlyEffect : MonoBehaviour
{
    [Header("飞行配置")]
    [SerializeField] private RectTransform targetIcon;
    [SerializeField] private float flyToMidDuration = 0.5f;
    [SerializeField] private float midPauseDuration = 0.6f;
    [SerializeField] private float flyToTargetDuration = 0.6f;
    [SerializeField] private float endDelay = 0.5f;
    [SerializeField] private float scaleEnd = 0.3f;
    [SerializeField] private Ease flyEase = Ease.InBack;
    [SerializeField] private Vector3 finalRotation = Vector3.zero;

    [Header("中间展示点（Canvas中的RectTransform）")]
    [SerializeField] private RectTransform midPoint;

    [Header("飞行距相机距离（越大物体越小）")]
    [SerializeField] private float flyDepth = 3f;

    [Header("渲染置顶")]
    [SerializeField] private int flyRenderQueue = 4000;

    private Camera mainCam;
    private Transform originalParent;
    private Vector3 originalScale;
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private int[] originalRenderQueues;
    private Material[] flyMaterials;
    private ParticleSystem[] sparkleFx;

    void Start()
    {
        mainCam = Camera.main;

        originalParent = transform.parent;
        originalScale = transform.localScale;
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;

        // 缓存粒子特效，并确保静止时不发射
        sparkleFx = GetComponentsInChildren<ParticleSystem>(true);
        SetSparkleActive(false);

        // 缓存所有材质的原始renderQueue
        var renderers = GetComponentsInChildren<Renderer>();
        int matCount = 0;
        foreach (var r in renderers) matCount += r.materials.Length;
        originalRenderQueues = new int[matCount];
        flyMaterials = new Material[matCount];
        int idx = 0;
        foreach (var r in renderers)
        {
            foreach (var m in r.materials)
            {
                originalRenderQueues[idx] = m.renderQueue;
                flyMaterials[idx] = m;
                idx++;
            }
        }
    }

    public void Fly(System.Action onComplete = null)
    {
        if (mainCam == null)
        {
            onComplete?.Invoke();
            return;
        }

        // 禁用碰撞和物理
        var col = GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;
        var rb = GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 挂到相机下，用本地坐标做动画，相机移动不影响屏幕位置
        transform.SetParent(mainCam.transform, true);

        // 渲染置顶
        SetRenderOnTop(true);

        // 开始旋转飞行 → 开启烟花
        SetSparkleActive(true);

        // 中间展示位置（相机本地坐标）
        Vector3 midLocalPos;
        if (midPoint != null)
        {
            midLocalPos = UIToCameraLocalPosition(midPoint);
        }
        else
        {
            // 没有指定midPoint时，飞到当前位置正上方
            Vector3 curLocal = transform.localPosition;
            midLocalPos = curLocal + Vector3.up * 0.5f;
        }

        // 目标位置（相机本地坐标）
        Vector3 targetLocalPos = targetIcon != null ? UIToCameraLocalPosition(targetIcon) : midLocalPos;

        Sequence seq = DOTween.Sequence();

        // ===== 阶段1：飞到中间展示点 =====
        seq.Append(transform.DOLocalMove(midLocalPos, flyToMidDuration).SetEase(Ease.OutCubic));
        seq.Join(transform.DOLocalRotate(new Vector3(360f, 0f, 90f),
            flyToMidDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear));

        // ===== 阶段2：中间停顿，原地旋转展示 =====
        seq.Append(transform.DOLocalRotate(new Vector3(0f, 360f * 2, 90f),
            midPauseDuration, RotateMode.FastBeyond360).SetEase(Ease.InOutSine));

        // ===== 阶段3：飞向目标UI图标位置 =====
        seq.Append(transform.DOLocalMove(targetLocalPos, flyToTargetDuration).SetEase(flyEase));
        seq.Join(transform.DOLocalRotate(finalRotation, flyToTargetDuration).SetEase(Ease.OutCubic));
        seq.Join(transform.DOScale(originalScale * scaleEnd, flyToTargetDuration).SetEase(Ease.InQuad));

        // ===== 阶段4：到达后延迟 =====
        seq.AppendInterval(endDelay);

        seq.OnComplete(() =>
        {
            var rend = GetComponentInChildren<Renderer>();
            if (rend != null) rend.enabled = false;
            SetSparkleActive(false);
            onComplete?.Invoke();
        });
    }

    private void SetSparkleActive(bool on)
    {
        if (sparkleFx == null) return;
        for (int i = 0; i < sparkleFx.Length; i++)
        {
            var p = sparkleFx[i];
            if (p == null) continue;
            if (on)
            {
                p.Clear(true);
                p.Play(true);
            }
            else
            {
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    public void ResetFlyState()
    {
        transform.DOKill();
        SetSparkleActive(false);

        // 从相机下移回原父物体
        if (originalParent != null)
            transform.SetParent(originalParent, false);
        else
            transform.SetParent(null, true);

        // 恢复本地变换
        transform.localPosition = originalLocalPos;
        transform.localRotation = originalLocalRot;
        transform.localScale = originalScale;

        // 恢复渲染队列
        SetRenderOnTop(false);

        // 恢复Rigidbody
        var rb = GetComponentInChildren<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;
    }

    /// <summary>
    /// 将Canvas中的RectTransform位置转换为相机本地坐标（flyDepth距离处）
    /// </summary>
    private Vector3 UIToCameraLocalPosition(RectTransform uiElement)
    {
        Canvas canvas = uiElement.GetComponentInParent<Canvas>();
        if (canvas != null) canvas = canvas.rootCanvas;

        Camera canvasCam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            canvasCam = canvas.worldCamera;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvasCam, uiElement.position);

        // 屏幕坐标 → 世界坐标 → 相机本地坐标
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, flyDepth));
        return mainCam.transform.InverseTransformPoint(worldPos);
    }

    private void SetRenderOnTop(bool onTop)
    {
        if (flyMaterials == null) return;
        for (int i = 0; i < flyMaterials.Length; i++)
        {
            if (flyMaterials[i] == null) continue;
            flyMaterials[i].renderQueue = onTop ? flyRenderQueue : originalRenderQueues[i];
        }
    }
}
