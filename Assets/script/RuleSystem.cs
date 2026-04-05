using System.Collections.Generic;
using UnityEngine;

public class RuleSystem : MonoBehaviour
{
    public static RuleSystem Instance { get; private set; }

    // 当前激活的规则
    private HashSet<RuleType> activeRules = new HashSet<RuleType>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 启用规则
    /// </summary>
    public void EnableRule(RuleType rule)
    {
        if (rule == RuleType.None) return;

        if (activeRules.Add(rule))
        {
            Debug.Log($"规则开启: {rule}");
        }
    }

    /// <summary>
    /// 关闭规则
    /// </summary>
    public void DisableRule(RuleType rule)
    {
        if (activeRules.Remove(rule))
        {
            Debug.Log($"规则关闭: {rule}");
        }
    }

    /// <summary>
    /// 切换规则
    /// </summary>
    public void ToggleRule(RuleType rule)
    {
        if (IsRuleActive(rule))
            DisableRule(rule);
        else
            EnableRule(rule);
    }

    /// <summary>
    /// 查询规则是否生效
    /// </summary>
    public bool IsRuleActive(RuleType rule)
    {
        return activeRules.Contains(rule);
    }

    /// <summary>
    /// 清空所有规则
    /// </summary>
    public void ClearAllRules()
    {
        activeRules.Clear();
    }
}