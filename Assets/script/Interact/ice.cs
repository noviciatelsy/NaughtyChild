using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ice : Interact
{
    [Header("成就")]
    [SerializeField] private AchievementSO achievement;

    [Header("是否只能触发一次")]
    [SerializeField] private bool oneShot = true;

    private bool used = false;

    protected override bool OnInteracted(GameObject item)
    {
        if (!Interactable) return false;

        // 防止重复触发
        if (oneShot && used) return false;

        used = true;

        // =========================
        // 触发成就
        // =========================
        if (achievement != null && AchievementManager.Instance != null)
        {
            AchievementManager.Instance.RecordAction(achievement.achievementName);
        }

        // =========================
        // 销毁自己
        // =========================
        Destroy(gameObject);

        return true;
    }
}
