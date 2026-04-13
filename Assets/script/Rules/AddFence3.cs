using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddFence3 : RuleTrigger
{
    public Transform fence;
    private int curRound = 0;

    private void Start()
    {
        curRound = GameManager.Instance.CurrentRound;

        GameManager.Instance.OnRoundStarted += OnRoundChanged;
        if (fence != null)
        {
            fence.gameObject.SetActive(false);
        }
    }

    private void OnRoundChanged(int newRound)
    {
        if (newRound > curRound)
        {
            curRound = newRound;
            if (RuleSystem.Instance.IsRuleActive(ruleName))
            {
                fence.gameObject.SetActive(true);
            }
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        //只允许第1次触发:蓝字补丁
        if (RuleSystem.Instance.IsRuleActive(ruleName))
        {
            return;
        }

        // 调用父类原逻辑
        base.OnTriggerEnter(other);
        //fence.gameObject.SetActive(true);
    }
}
