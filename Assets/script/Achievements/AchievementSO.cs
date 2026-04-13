using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementSO : ScriptableObject
{
    public Sprite icon;
    public string achievementName;
    public string description;
    public bool IsUnlocked { get; private set; }

    public event Action<AchievementSO> OnUnlocked;

    public virtual bool CheckUnlock()
    {
        return false;
    }

    public void TryUnlock()
    {
        if (IsUnlocked) return;
        if (!CheckUnlock()) return;

        IsUnlocked = true;
        Debug.Log($"成就解锁: {achievementName}");
        OnUnlocked?.Invoke(this);
    }

    public virtual void ResetProgress()
    {
        IsUnlocked = false;
    }
}
