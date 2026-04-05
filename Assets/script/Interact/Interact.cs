using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{
    public Rule rule;

    [Header("交互设置")]
    [SerializeField] private bool interactable = true;
    [SerializeField] private string interactHint = "按E交互";

    public bool Interactable => interactable && GameManager.Instance.CurrentState == GameState.Playing;
    public string InteractHint => interactHint;

    // UI层事件接口
    public event Action<Interact> OnInteractedEvent;

    //玩家交互物体，将背后的规则记录为候选
    public virtual void InteractObject()
    {
        if (!Interactable) return;

        if (rule != null)
        {
            rule.ApplyRule();
        }

        OnInteracted();
        OnInteractedEvent?.Invoke(this);
    }
    //交互完成后的回调，子类可重写实现特定表现
    protected virtual void OnInteracted()
    {
    }

    //运行时控制交互开关
    public void SetInteractable(bool value)
    {
        interactable = value;
    }
}
