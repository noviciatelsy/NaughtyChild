using UnityEngine;

/// <summary>
/// Trigger型规则检测
/// 简单场景直接挂这个脚本，Inspector填规则名即可
/// 需要自定义逻辑的再子类覆写
/// </summary>
[RequireComponent(typeof(Collider))]
public class RuleTrigger : MonoBehaviour
{
    [Header("规则配置")]
    [SerializeField] public string ruleName;
    [SerializeField] private string playerTag = "Player";

    [Header("成就配置")]
    [SerializeField] private AchievementSO achievement;

    public string RuleName => ruleName;

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger:"+ this.name);
        if (!other.CompareTag(playerTag)) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        if (RuleSystem.Instance.IsRuleActive(ruleName))
        {
            var rule = RuleSystem.Instance.GetRule(ruleName);
            rule.OnRuleViolated(other.gameObject);
        }
        else
        {
            RuleSystem.Instance.SetPending(ruleName);
        }

        if (achievement != null && AchievementManager.Instance != null)
        {
            AchievementManager.Instance.RecordAction(achievement.achievementName);
        }
    }
}
