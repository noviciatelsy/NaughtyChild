using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RulesBoard : MonoBehaviour
{
    public GameObject rulePrefab;
    public GameObject achievementPrefab;
    public RectTransform content;
    public Image Panel;
    [SerializeField] private RectTransform ort;
    [SerializeField] private RectTransform drt;
    [Header("背景图切换")]
    [SerializeField] private Image rulesBg;
    [SerializeField] private Image achievementsBg;
    private Image boardImage;
    private float oTp = 0.95f;
    private float dtp = 1f;
    private RectTransform selfRect;
    private int ruleCount = 1;
    private bool showingAchievements = false;
    private List<GameObject> ruleInstances = new List<GameObject>();
    private List<GameObject> achievementInstances = new List<GameObject>();
    private CanvasGroup contentCanvasGroup;
    private bool isSwitching = false;

    void Start()
    {
        Panel.gameObject.SetActive(false);
        selfRect = GetComponent<RectTransform>();
        boardImage = GetComponent<Image>();
        contentCanvasGroup = content.GetComponent<CanvasGroup>();
        GameManager.Instance.OnRuleCommitted += (r) => { AddRuleData(r); };
        GameManager.Instance.OnShowRulesRequested += (show) => { HandleToggle(show); };
        GameManager.Instance.OnSwitchBoardRequested += SwitchContent;
        selfRect.anchoredPosition = new Vector2(ort.anchoredPosition.x, selfRect.anchoredPosition.y);

        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnSwitchBoardRequested -= SwitchContent;
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
    }

    private void OnAchievementUnlocked(AchievementSO ach)
    {
        Debug.Log($"[RulesBoard] 收到成就解锁: {ach.achievementName}, achievementPrefab={(achievementPrefab != null ? "有" : "空")}");
        var instance = Instantiate(achievementPrefab, content, false);
        var item = instance.GetComponent<AchievementItem>();
        if (item != null)
            item.SetData(ach);
        instance.SetActive(showingAchievements);
        achievementInstances.Add(instance);
        Debug.Log($"[RulesBoard] 成就实例数: {achievementInstances.Count}, 当前显示成就: {showingAchievements}");
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
                if (rulesBg != null) boardImage = rulesBg;
            }
            else
            {
                foreach (var obj in ruleInstances) obj.SetActive(false);
                foreach (var obj in achievementInstances) obj.SetActive(true);
                showingAchievements = true;
                if (achievementsBg != null) boardImage = achievementsBg;
            }
        });
        seq.Append(contentCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic));
        seq.Join(content.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
        seq.OnComplete(() => isSwitching = false);
    }

    private void AddRuleData(Rule rule)
    {
        var instance = Instantiate(rulePrefab, content, false);
        var text = instance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            text.text = $" {ruleCount++}. {rule.description}";
        }
        ruleInstances.Add(instance);
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
                if (rulesBg != null) boardImage = rulesBg;
            }
            isSwitching = false;
            contentCanvasGroup.alpha = 1f;
            content.localScale = Vector3.one;

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