using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Rule : MonoBehaviour
{
    public Action<Rule> onRuleApplied, onRuleRemoved;
    public int priority;
    public virtual void ApplyRule()
    {
        onRuleApplied?.Invoke(this);
    }
    public virtual void RemoveRule()
    {
        onRuleRemoved?.Invoke(this);
    }
}

