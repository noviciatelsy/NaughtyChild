using UnityEngine;

/// <summary>
/// 规则 = ScriptableObject 纯数据
/// 用 SO 资产的 name 作为唯一标识，无需维护枚举
/// 创建：Assets → Create → Rules → Rule
/// </summary>
[CreateAssetMenu(fileName = "NewRule", menuName = "Rules/Rule")]
public class Rule : ScriptableObject
{
    public int priority;
    [TextArea] public string description;

    public void OnRuleViolated(GameObject violator)
    {
        Debug.Log($"规则 [{name}] 被 {violator.name} 违反!");
        GameManager.Instance.RestartRound();
    }
}


