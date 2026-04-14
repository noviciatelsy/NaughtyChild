using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndScene : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private string gameSceneName = "GameScene";

    void Start()
    {
        restartButton?.onClick.AddListener(RestartGame);
        exitButton?.onClick.AddListener(ExitGame);
    }

    private bool _isTransitioning;

    private void RestartGame()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.ResetGameState();

        if (SceneTransition.Instance != null)
            SceneTransition.Instance.TransitionToScene(gameSceneName);
        else
            SceneManager.LoadScene(gameSceneName);
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
