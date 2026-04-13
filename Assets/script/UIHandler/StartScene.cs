using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private string gameSceneName = "GameScene";

    void Start()
    {
        startButton?.onClick.AddListener(StartGame);
        exitButton?.onClick.AddListener(ExitGame);
    }

    private void StartGame()
    {
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
