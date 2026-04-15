using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class RulesBoard : MonoBehaviour
{
    public GameObject rulePrefab;
    public GameObject achievementPrefab;
    public RectTransform content;
    public Image Panel;
    [SerializeField] private RectTransform ort;
    [SerializeField] private RectTransform drt;
    [Header("背景图切换")]
    [SerializeField] private Sprite rulesBg;
    [SerializeField] private Sprite achievementsBg;
    [Header("违规高亮")]
    [SerializeField] private Color violationColor = Color.red;
    [SerializeField] private ScrollRect scrollRect;
    [Header("Cookie计数")]
    [SerializeField] private TextMeshProUGUI cookieCountText;
    [SerializeField] private RectTransform cookieIcon;
    private Image boardImage;
    private float oTp = 0.95f;
    private float dtp = 1f;
    private RectTransform selfRect;
    private int ruleCount = 1;
    private bool showingAchievements = false;
    private List<GameObject> ruleInstances = new List<GameObject>();
    private List<GameObject> achievementInstances = new List<GameObject>();
    private Dictionary<Rule, GameObject> ruleInstanceMap = new Dictionary<Rule, GameObject>();
    private TextMeshProUGUI highlightedText;
    private Color originalTextColor;
    private CanvasGroup contentCanvasGroup;
    private bool isSwitching = false;

    void Start()
    {
        Panel.gameObject.SetActive(false);
        selfRect = GetComponent<RectTransform>();
        boardImage = GetComponent<Image>();
        contentCanvasGroup = content.GetComponent<CanvasGroup>();
        GameManager.Instance.OnRuleCommitted += OnRuleCommitted;
        GameManager.Instance.OnShowRulesRequested += HandleToggle;
        GameManager.Instance.OnSwitchBoardRequested += SwitchContent;
        GameManager.Instance.OnRuleViolated += OnRuleViolated;
        GameManager.Instance.OnCookieCollected += OnCookieCollected;
        selfRect.anchoredPosition = new Vector2(ort.anchoredPosition.x, selfRect.anchoredPosition.y);

        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;

        SetCookieText(0);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRuleCommitted -= OnRuleCommitted;
            GameManager.Instance.OnShowRulesRequested -= HandleToggle;
            GameManager.Instance.OnSwitchBoardRequested -= SwitchContent;
            GameManager.Instance.OnRuleViolated -= OnRuleViolated;
            GameManager.Instance.OnCookieCollected -= OnCookieCollected;
        }
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
    }

    private void OnRuleCommitted(Rule r)
    {
        AddRuleData(r);
    }

    private void OnAchievementUnlocked(AchievementSO ach)
    {
        Debug.Log($"[RulesBoard] 收到成就解锁: {ach.achievementName}");

        var instance = Instantiate(achievementPrefab, content, false);

        var item = instance.GetComponent<AchievementItem>();
        if (item != null)
            item.SetData(ach);

        // ⭐关键修复：不要用 showingAchievements 控制“是否生成可见”
        instance.SetActive(true);

        achievementInstances.Add(instance);

        // ⭐如果当前正在看成就页，确保它立刻可见
        if (showingAchievements)
        {
            instance.transform.SetAsLastSibling();
        }
        // 如果当前不在成就页，自动切一次（只触发一次体验提升）
        if (!showingAchievements)
        {
            SwitchContent();
        }
    }

    private void SwitchContent()
    {
        if (isSwitching) return;
        isSwitching = true;

        // 淡出 + 缩小
        Sequence seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(contentCanvasGroup.DOFade(0f, 0.15f).SetEase(Ease.InCubic));
        seq.Join(content.DOScale(0.9f, 0.15f).SetEase(Ease.InCubic));
        seq.AppendCallback(() =>
        {
            // 切换内容
            if (showingAchievements)
            {
                foreach (var obj in achievementInstances) obj.SetActive(false);
                foreach (var obj in ruleInstances) obj.SetActive(true);
                showingAchievements = false;
                if (rulesBg != null) boardImage.sprite = rulesBg;
            }
            else
            {
                foreach (var obj in ruleInstances) obj.SetActive(false);
                foreach (var obj in achievementInstances) obj.SetActive(true);
                showingAchievements = true;
                if (achievementsBg != null) boardImage.sprite = achievementsBg;
            }
        });
        seq.Append(contentCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic));
        seq.Join(content.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
        seq.OnComplete(() => isSwitching = false);
    }

    private void AddRuleData(Rule rule)
    {
        var instance = Instantiate(rulePrefab, content, false);
        var text = instance.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = $" {ruleCount++}. {rule.description}";
        }
        ruleInstances.Add(instance);
        ruleInstanceMap[rule] = instance;
    }

    private void OnRuleViolated(Rule rule)
    {
        if (showingAchievements)
        {
            foreach (var obj in achievementInstances) obj.SetActive(false);
            foreach (var obj in ruleInstances) obj.SetActive(true);
            showingAchievements = false;
            if (rulesBg != null) boardImage.sprite = rulesBg;
        }

        ClearHighlight();
        if (ruleInstanceMap.TryGetValue(rule, out var ruleObj))
        {
            var text = ruleObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                highlightedText = text;
                originalTextColor = highlightedText.color;
                highlightedText.color = violationColor;
                highlightedText.DOKill();
                highlightedText.DOFade(0.3f, 0.3f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
            }
            ScrollToItem(ruleObj.GetComponent<RectTransform>());
        }
    }

    private void OnCookieCollected(int count)
    {
        SetCookieText(count);
        if (cookieCountText != null)
        {
            // 弹跳动画
            cookieCountText.transform.DOKill();
            cookieCountText.transform.localScale = Vector3.one;
            cookieCountText.transform.DOPunchScale(Vector3.one * 0.5f, 0.4f, 6, 0.7f);
        }
        if (cookieIcon != null)
        {
            cookieIcon.DOKill();
            cookieIcon.localScale = Vector3.one;
            cookieIcon.DOPunchScale(Vector3.one * 0.3f, 0.4f, 6, 0.7f);
        }
    }

    private void SetCookieText(int count)
    {
        if (cookieCountText != null)
            cookieCountText.text = $"：{count}";
    }

    private void ClearHighlight()
    {
        if (highlightedText != null)
        {
            highlightedText.DOKill();
            highlightedText.color = originalTextColor;
            highlightedText.alpha = 1f;
            highlightedText = null;
        }
    }

    /// <summary>
    /// 自动滚动ScrollView让目标item可见（居中偏上）
    /// </summary>
    private void ScrollToItem(RectTransform target)
    {
        if (scrollRect == null || target == null) return;

        // 等一帧让布局刷新
        StartCoroutine(ScrollToItemCoroutine(target));
    }

    private IEnumerator ScrollToItemCoroutine(RectTransform target)
    {
        yield return null; // 等Canvas重建布局
        Canvas.ForceUpdateCanvases();

        RectTransform viewport = scrollRect.viewport ?? scrollRect.GetComponent<RectTransform>();
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight <= viewportHeight)
            yield break; // 内容不超出视口，无需滚动

        // 计算目标在content中的局部y位置（从顶部算起的偏移）
        float targetY = -target.anchoredPosition.y;
        float targetHalfH = target.rect.height * 0.5f;

        // 期望让目标出现在视口中心
        float desiredScrollY = targetY - viewportHeight * 0.5f + targetHalfH;

        // clamp到合法范围
        float maxScroll = contentHeight - viewportHeight;
        desiredScrollY = Mathf.Clamp(desiredScrollY, 0f, maxScroll);

        // normalizedPosition: 1=顶部, 0=底部
        float normalizedY = 1f - (desiredScrollY / maxScroll);

        DOTween.To(() => scrollRect.verticalNormalizedPosition,
            v => scrollRect.verticalNormalizedPosition = v,
            normalizedY, 0.4f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    private void HandleToggle(bool show)
    {
        selfRect.DOKill();
        transform.DOKill();
        Panel.DOKill();
        transform.localScale = Vector3.one * 1.3f;
        transform.localRotation = Quaternion.identity;
        if (show)
        {
            Panel.gameObject.SetActive(true);
            Panel.DOFade(0.5f, 0.3f)
                .SetEase(Ease.InCubic)
                .SetUpdate(true);
            this.GetComponent<Image>().DOFade(dtp, 0.4f).SetEase(Ease.OutCubic)
                .SetUpdate(true);
            selfRect.DOAnchorPosX(drt.anchoredPosition.x, 0.4f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
            transform.DOPunchScale(Vector3.one * 0.15f, 0.5f, 8, 1.0f)
                .SetUpdate(true);
        }
        else
        {
            // 关闭时切回规则视图
            if (showingAchievements)
            {
                foreach (var obj in achievementInstances) obj.SetActive(false);
                foreach (var obj in ruleInstances) obj.SetActive(true);
                showingAchievements = false;
                if (rulesBg != null) boardImage.sprite = rulesBg;
            }
            isSwitching = false;
            contentCanvasGroup.alpha = 1f;
            content.localScale = Vector3.one;
            ClearHighlight();

            Panel.DOFade(0f, 0.3f)
                .SetEase(Ease.InCubic)
                .SetUpdate(true)
                .OnComplete(() => Panel.gameObject.SetActive(false));
            this.GetComponent<Image>().DOFade(oTp, 0.4f).SetEase(Ease.OutCubic)
                .SetUpdate(true);
            selfRect.DOAnchorPosX(ort.anchoredPosition.x, 0.3f)
                .SetEase(Ease.InCubic)
                .SetUpdate(true);
        }
    }
}