using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public event Action<bool> OnShowRulesRequested;
    public event Action OnSwitchBoardRequested;

    public int RulesToEnd = 15; //累计触发多少条规则可以游戏结束
    public string EndingSceneName = "Ending";
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
    }
    public void StartRound()
    {
        CurrentRound++;
        RuleSystem.Instance.ResetPending();
        SetState(GameState.Playing);
        OnRoundStarted?.Invoke(CurrentRound);
        Debug.Log($"第 {CurrentRound} 轮开始");

        SetplayerPos();
    }

    //玩家通关，结算本轮
    public void CompleteRound()
    {
        if (CurrentState != GameState.Playing) return;

        SetState(GameState.RoundComplete);

        Rule committed = RuleSystem.Instance.CommitRound();
        OnRoundCompleted?.Invoke(CurrentRound);
        if (committed != null)
            OnRuleCommitted?.Invoke(committed);

        Debug.Log(committed != null
            ? $"第 {CurrentRound} 轮结束，新增规则: {committed.name}"
            : $"第 {CurrentRound} 轮结束，无新增规则");


        //判断是否结束游戏：累计生效规则数 >= RulesToEnd
        if (RuleSystem.Instance.GetActiveRules().Count >= RulesToEnd)
        {
            TransitionToEnding();
        }
        else
        {
            //todo:新的一轮动画,etc...
            StartRound();
        }
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
    public void RequestShowRules(bool show)
    {
        OnShowRulesRequested?.Invoke(show);
    }

    public void RequestSwitchBoard()
    {
        OnSwitchBoardRequested?.Invoke();
    }

    public void GameOver()
    {
        SetState(GameState.GameOver);
    }
    [ContextMenu("测试结算结束")]
    private void TransitionToEnding()
    {
        SetState(GameState.GameOver);
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.TransitionToScene(EndingSceneName);
        }
        else
        {
            // 没有转场组件时直接切换
            SceneManager.LoadScene(EndingSceneName);
        }
    }

    private void SetState(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }

    private void SetplayerPos()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playermovement pl = player.GetComponent<playermovement>();
            pl.ResetPlayerState();
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero; // 清速度
                rb.angularVelocity = Vector3.zero;
                rb.position = new Vector3(0f, 0.5f, -2f);
            }
            else
            {
                player.transform.position = new Vector3(0f, 0.5f, -2f);
            }
        }
    }
}
