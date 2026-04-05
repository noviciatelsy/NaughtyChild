using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    WaitingToStart,
    Playing,
    Paused,
    RoundComplete,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    //给UI层用的
    public event Action<GameState> OnGameStateChanged;
    public event Action<int> OnRoundStarted;          //当前轮次
    public event Action<int> OnRoundCompleted;         //完成的轮次
    public event Action<Rule> OnRuleCommitted;         //本轮生效的规则
    public event Action OnGameRestarted;

    public GameState CurrentState { get; private set; } = GameState.WaitingToStart;
    public int CurrentRound { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        RuleSystem.Instance.Initialize();
        StartRound();
    }
    public void StartRound()
    {
        CurrentRound++;
        RuleSystem.Instance.ResetPending();
        SetState(GameState.Playing);
        OnRoundStarted?.Invoke(CurrentRound);
        Debug.Log($"第 {CurrentRound} 轮开始");
    }

    //玩家通关，结算本轮
    public void CompleteRound()
    {
        if (CurrentState != GameState.Playing) return;

        SetState(GameState.RoundComplete);

        Rule committed = RuleSystem.Instance.CommitRound();
        OnRoundCompleted?.Invoke(CurrentRound);
        OnRuleCommitted?.Invoke(committed);

        Debug.Log(committed != null
            ? $"第 {CurrentRound} 轮结束，新增规则: {committed.name}"
            : $"第 {CurrentRound} 轮结束，无新增规则");
    }

    //重开当前轮不结算规则
    public void RestartRound()
    {
        RuleSystem.Instance.ResetPending();
        CurrentRound--;
        StartRound();
    }

    //完全重开游戏，清空规则和轮次
    public void RestartGame()
    {
        RuleSystem.Instance.ClearAllRules();
        CurrentRound = 0;
        OnGameRestarted?.Invoke();
        StartRound();
    }
    public void Pause()
    {
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.Paused);
        Time.timeScale = 0f;
    }
    public void Resume()
    {
        if (CurrentState != GameState.Paused) return;
        Time.timeScale = 1f;
        SetState(GameState.Playing);
    }
    public void GameOver()
    {
        SetState(GameState.GameOver);
    }

    private void SetState(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}
