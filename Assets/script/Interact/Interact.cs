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

    public Collider col;
    public Renderer rend;
    public Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;
    private int curRound;
    private void Start()
    {
        col = GetComponentInChildren<Collider>();
        rend = GetComponentInChildren<Renderer>();
        rb = GetComponentInChildren<Rigidbody>();

        startPos = transform.position;
        startRot = transform.rotation;

        curRound = GameManager.Instance.CurrentRound;
        GameManager.Instance.OnRoundStarted += OnRoundChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRoundStarted -= OnRoundChanged;
    }

    public virtual void InteractObject(GameObject item)
    {
        if (!Interactable) return;  

        Debug.Log("与" + this.name + "交互");
        if (item == null)
        {
            var throwable = GetComponent<Throwable>();
            if (throwable != null && !PlayerHand.Instance.HasItem)
            {
                PlayerHand.Instance.PickItem(throwable);
                Debug.Log("捡起:" + this.name);
            }
        }

        // 规则系统（保持原样）
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

        OnInteracted(item); // 传下去
        OnInteractedEvent?.Invoke(this);
    }

    protected virtual void OnInteracted(GameObject item) { }

    public void SetInteractable(bool value)
    {
        interactable = value;
    }

    public virtual void Reset()
    {
        transform.position = startPos;
        transform.rotation = startRot;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        col.enabled = true;
        rend.enabled = true;

        Debug.Log("物品" + this.name + "重置");
    }

    public virtual void OnRoundChanged(int newRound)
    {
        //if (newRound != curRound)
        //{
            curRound = newRound;
            Reset();
        //}
    }
}

