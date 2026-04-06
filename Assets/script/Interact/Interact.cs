using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interact型规则检测
/// 简单场景直接挂这个脚本，Inspector填规则名即可
/// 需要自定义逻辑的再子类覆写
/// </summary>
public class Interact : MonoBehaviour
{
    [Header("规则配置")]
    [SerializeField] private string ruleName;

    [Header("交互设置")]
    [SerializeField] private bool interactable = true;
    [SerializeField] private string interactHint = "按E交互";

    public string RuleName => ruleName;
    public bool Interactable => interactable && GameManager.Instance.CurrentState == GameState.Playing;
    public string InteractHint => interactHint;

    public event Action<Interact> OnInteractedEvent;

    public virtual void InteractObject()
    {
        if (!Interactable) return;

        Debug.Log("与" + this.name + "交互");
        if (RuleSystem.Instance.IsRuleActive(ruleName))
        {
            var rule = RuleSystem.Instance.GetRule(ruleName);
            rule.OnRuleViolated(gameObject);
            return;
        }
        else
        {
            RuleSystem.Instance.SetPending(ruleName);
        }

        OnInteracted();
        OnInteractedEvent?.Invoke(this);
    }

    protected virtual void OnInteracted() { }

    public void SetInteractable(bool value)
    {
        interactable = value;
    }
}

