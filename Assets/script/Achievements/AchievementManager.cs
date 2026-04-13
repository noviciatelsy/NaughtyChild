using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [SerializeField] private List<AchievementSO> allAchievements = new List<AchievementSO>();

    public event Action<AchievementSO> OnAchievementUnlocked;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        foreach (var ach in allAchievements)
        {
            ach.ResetProgress();
            ach.OnUnlocked += HandleUnlocked;
        }
    }

    private void OnDestroy()
    {
        foreach (var ach in allAchievements)
            ach.OnUnlocked -= HandleUnlocked;
    }

    private void HandleUnlocked(AchievementSO achievement)
    {
        Debug.Log($"[AchievementManager] 成就解锁事件触发: {achievement.achievementName}");
        OnAchievementUnlocked?.Invoke(achievement);
    }
    public T GetAchievement<T>(string name) where T : AchievementSO
    {
        foreach (var ach in allAchievements)
        {
            if (ach.achievementName == name && ach is T typed)
                return typed;
        }
        return null;
    }

    public List<AchievementSO> GetUnlockedAchievements()
    {
        var result = new List<AchievementSO>();
        foreach (var ach in allAchievements)
        {
            if (ach.IsUnlocked)
                result.Add(ach);
        }
        return result;
    }

    /// <summary>
    /// 供外部随时调用，记录玩家行为并推进成就
    /// </summary>
    public void RecordAction(string actionName, int amount = 1)
    {
        Debug.Log($"[AchievementManager] RecordAction: {actionName}");

        var boolAch = GetAchievement<BoolAchievementSO>(actionName);
        if (boolAch != null) { boolAch.SetCondition(0, true); return; }

        var intAch = GetAchievement<IntAchievementSO>(actionName);
        if (intAch != null) { intAch.AddProgress(amount); return; }

        var floatAch = GetAchievement<FloatAchievementSO>(actionName);
        if (floatAch != null) { floatAch.AddProgress(amount); return; }

        Debug.LogWarning($"[AchievementManager] 未找到成就: {actionName}");
    }
}
