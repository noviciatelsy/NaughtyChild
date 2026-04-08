using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
public class RulesBoard : MonoBehaviour
{
    public GameObject rulePrefab;
    public RectTransform content;
    [SerializeField] private RectTransform ort; // 拖入：隐藏位置的空物体
    [SerializeField] private RectTransform drt; // 拖入：显示位置的空物体
    private RectTransform selfRect;
    private int ruleCount = 1;
    void Start()
    {
        selfRect = GetComponent<RectTransform>();
        GameManager.Instance.OnRuleCommitted += (r) => { AddRuleData(r); };
        GameManager.Instance.OnShowRulesRequested += (show) => { HandleToggle(show); };
        selfRect.anchoredPosition = new Vector2(ort.anchoredPosition.x, selfRect.anchoredPosition.y);
    }
    private void AddRuleData(Rule rule)
    {
        var instance = Instantiate(rulePrefab, content, false);
        var text = instance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            text.text = $"规则{ruleCount++}: {rule.description}";
        }
    }
    private void HandleToggle(bool show)
    {
        selfRect.DOKill();
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        if (show)
        {
            selfRect.DOAnchorPosX(drt.anchoredPosition.x, 0.4f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
            transform.DOPunchScale(Vector3.one * 0.1f, 0.4f, 6, 0.5f)
                .SetUpdate(true);
            transform.DOPunchRotation(new Vector3(0, 0, 5f), 0.1f, 8, 0.5f)
                .SetUpdate(true);
        }
        else
        {
            selfRect.DOAnchorPosX(ort.anchoredPosition.x, 0.3f)
                .SetEase(Ease.InCubic)
                .SetUpdate(true);
        }
    }
}
