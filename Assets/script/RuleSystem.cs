using System;
using System.Collections.Generic;
using UnityEngine;

public class RuleSystem : MonoBehaviour
{
    public static RuleSystem Instance { get; private set; }
    public List<Rule> rules; // 数据库

    private HashSet<Rule> activeRules;
    private Rule pendingRule;

    //UI层事件接口
    public event Action<Rule> OnPendingRuleChanged;  //候选规则变更
    public event Action<Rule> OnRuleActivated;       //规则正式生效
    public event Action<Rule> OnRuleDeactivated;     //规则被移除

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
        foreach (var rule in rules)
        {
            rule.onRuleApplied += OnRuleApplied;
            rule.onRuleRemoved += OnRuleRemoved;
        }
    }

    //规则被交互触发时，按优先级竞争候选位
    private void OnRuleApplied(Rule rule)
    {
        if (pendingRule == null || rule.priority > pendingRule.priority)
        {
            pendingRule = rule;
            OnPendingRuleChanged?.Invoke(pendingRule);
            Debug.Log($"候选规则更新: {rule.GetType().Name} (优先级 {rule.priority})");
        }
    }

    //规则被主动移除
    private void OnRuleRemoved(Rule rule)
    {
        if (activeRules.Remove(rule))
        {
            OnRuleDeactivated?.Invoke(rule);
            Debug.Log($"规则移除: {rule.GetType().Name}");
        }
    }

    //通关结算，将候选规则正式加入生效列表，返回被提交的规则
    public Rule CommitRound()
    {
        Rule committed = pendingRule;
        if (committed != null)
        {
            activeRules.Add(committed);
            OnRuleActivated?.Invoke(committed);
            Debug.Log($"规则生效: {committed.GetType().Name} (优先级 {committed.priority})，当前共 {activeRules.Count} 条规则");
        }
        pendingRule = null;
        OnPendingRuleChanged?.Invoke(null);
        return committed;
    }

    //重置候选
    public void ResetPending()
    {
        pendingRule = null;
        OnPendingRuleChanged?.Invoke(null);
    }

    public bool IsRuleActive(Rule rule)
    {
        return activeRules.Contains(rule);
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
        var snapshot = new List<Rule>(activeRules);
        foreach (var rule in snapshot)
        {
            rule.RemoveRule();
        }
        pendingRule = null;
        OnPendingRuleChanged?.Invoke(null);
    }
}