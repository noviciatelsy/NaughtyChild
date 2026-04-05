using System;
using System.Collections.Generic;
using UnityEngine;

public class RuleSystem : MonoBehaviour
{
    public static RuleSystem Instance { get; private set; }

    // 所有规则SO（Inspector拖入）
    [SerializeField] private List<Rule> allRules = new List<Rule>();

    // 初始就生效的规则
    [SerializeField] private List<Rule> initialRules = new List<Rule>();

    // 按name索引，O(1)查找
    private Dictionary<string, Rule> ruleMap;
    private HashSet<Rule> activeRules;
    private Rule pendingRule;

    // UI层事件接口
    public event Action<Rule> OnPendingRuleChanged;
    public event Action<Rule> OnRuleActivated;
    public event Action<Rule> OnRuleDeactivated;

    public Rule PendingRule => pendingRule;

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

    public void Initialize()
    {
        activeRules = new HashSet<Rule>();
        pendingRule = null;

        // 构建字典
        ruleMap = new Dictionary<string, Rule>();
        foreach (var rule in allRules)
        {
            if (rule == null) continue;
            ruleMap[rule.name] = rule;
        }

        // 初始规则直接生效
        foreach (var rule in initialRules)
        {
            if (rule == null) continue;
            ActivateRule(rule);
        }
    }

    /// <summary>
    /// 按名字获取Rule SO
    /// </summary>
    public Rule GetRule(string ruleName)
    {
        ruleMap.TryGetValue(ruleName, out var rule);
        return rule;
    }

    /// <summary>
    /// 按名字查规则是否生效
    /// </summary>
    public bool IsRuleActive(string ruleName)
    {
        var rule = GetRule(ruleName);
        return rule != null && activeRules.Contains(rule);
    }

    public bool IsRuleActive(Rule rule)
    {
        return activeRules != null && activeRules.Contains(rule);
    }

    /// <summary>
    /// 按名字设置候选
    /// </summary>
    public void SetPending(string ruleName)
    {
        SetPending(GetRule(ruleName));
    }

    public void SetPending(Rule rule)
    {
        if (rule == null) return;
        if (pendingRule == null || rule.priority > pendingRule.priority)
        {
            pendingRule = rule;
            OnPendingRuleChanged?.Invoke(pendingRule);
            Debug.Log($"候选规则更新: {rule.name} (优先级 {rule.priority})");
        }
    }

    public Rule CommitRound()
    {
        Rule committed = pendingRule;
        if (committed != null)
        {
            ActivateRule(committed);
        }
        pendingRule = null;
        OnPendingRuleChanged?.Invoke(null);
        return committed;
    }

    private void ActivateRule(Rule rule)
    {
        if (activeRules.Add(rule))
        {
            OnRuleActivated?.Invoke(rule);
            Debug.Log($"规则生效: {rule.name} (优先级 {rule.priority})，当前共 {activeRules.Count} 条");
        }
    }

    public void DeactivateRule(Rule rule)
    {
        if (activeRules.Remove(rule))
        {
            OnRuleDeactivated?.Invoke(rule);
            Debug.Log($"规则移除: {rule.name}");
        }
    }

    public void ResetPending()
    {
        pendingRule = null;
        OnPendingRuleChanged?.Invoke(null);
    }

    public bool HasPendingRule()
    {
        return pendingRule != null;
    }

    public IReadOnlyCollection<Rule> GetActiveRules()
    {
        return activeRules;
    }

    public void ClearAllRules()
    {
        activeRules.Clear();
        pendingRule = null;
        OnPendingRuleChanged?.Invoke(null);
    }
}