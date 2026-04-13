using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using DG.Tweening;

public class Settings : MonoBehaviour
{
    [Header("暂停菜单面板（会被 SetActive 切换）")]
    [SerializeField] private GameObject pausePanel;

    [Header("按钮")]
    [SerializeField] private Button pauseButton;       // 局内暂停按钮（不在 pausePanel 下）
    [SerializeField] private Button resumeButton;      // 回到游戏（在 pausePanel 下）
    [SerializeField] private Button mainMenuButton;    // 主菜单
    [SerializeField] private Button quitButton;        // 退出游戏
    [SerializeField] private Button showRulesButton;   // 显示规则板

    [Header("音量")]
    [SerializeField] private Slider volumeSlider;

    [Header("场景名")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool _isPauseMenuOpen;
    private bool _waitingForBoardClose;
    private CanvasGroup _panelCanvasGroup;

    void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            _panelCanvasGroup = pausePanel.GetComponent<CanvasGroup>();
            if (_panelCanvasGroup == null)
                _panelCanvasGroup = pausePanel.AddComponent<CanvasGroup>();
        }

        pauseButton?.onClick.AddListener(OpenPauseMenu);
        resumeButton?.onClick.AddListener(ClosePauseMenu);
        mainMenuButton?.onClick.AddListener(GoToMainMenu);
        quitButton?.onClick.AddListener(QuitGame);
        showRulesButton?.onClick.AddListener(ShowRulesBoard);

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(v => AudioListener.volume = v);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnShowRulesRequested += OnShowRulesRequested;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnShowRulesRequested -= OnShowRulesRequested;
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (_waitingForBoardClose)
        {
            var player = FindObjectOfType<playermovement>();
            if (player != null)
                player.SetBoardInputState(false);
            GameManager.Instance.RequestShowRules(false);
        }
        else if (_isPauseMenuOpen)
        {
            ClosePauseMenu();
        }
        else if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            OpenPauseMenu();
        }
    }

    private void OpenPauseMenu()
    {
        if (_isPauseMenuOpen || pausePanel == null) return;
        _isPauseMenuOpen = true;

        // 打开暂停面板时隐藏暂停按钮
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);

        GameManager.Instance.Pause();

        pausePanel.SetActive(true);
        _panelCanvasGroup.alpha = 0f;
        pausePanel.transform.localScale = Vector3.one * 0.9f;

        DOTween.Sequence().SetUpdate(true)
            .Append(_panelCanvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic))
            .Join(pausePanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
    }

    private void ClosePauseMenu()
    {
        if (!_isPauseMenuOpen) return;

        DOTween.Sequence().SetUpdate(true)
            .Append(_panelCanvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InCubic))
            .Join(pausePanel.transform.DOScale(0.9f, 0.2f).SetEase(Ease.InCubic))
            .OnComplete(() =>
            {
                pausePanel.SetActive(false);
                _isPauseMenuOpen = false;

                // 恢复暂停按钮
                if (pauseButton != null)
                    pauseButton.gameObject.SetActive(true);

                GameManager.Instance.Resume();
            });
    }

    private void ShowRulesBoard()
    {
        _waitingForBoardClose = true;

        DOTween.Sequence().SetUpdate(true)
            .Append(_panelCanvasGroup.DOFade(0f, 0.15f).SetEase(Ease.InCubic))
            .OnComplete(() =>
            {
                pausePanel.SetActive(false);
                GameManager.Instance.RequestShowRules(true);

                var player = FindObjectOfType<playermovement>();
                if (player != null)
                    player.SetBoardInputState(true);
            });
    }

    private void OnShowRulesRequested(bool show)
    {
        if (!show && _waitingForBoardClose)
        {
            _waitingForBoardClose = false;
            _isPauseMenuOpen = false;

            // 直接关闭暂停状态，不再重新显示设置面板
            if (pauseButton != null)
                pauseButton.gameObject.SetActive(true);

            GameManager.Instance.Resume();
        }
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        _isPauseMenuOpen = false;

        if (SceneTransition.Instance != null)
            SceneTransition.Instance.TransitionToScene(mainMenuScene);
        else
            SceneManager.LoadScene(mainMenuScene);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
