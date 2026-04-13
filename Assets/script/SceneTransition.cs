using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 全屏转场控制器，使用 ScreenTransition shader 的圆形遮罩效果。
/// 自动创建一个 Overlay Canvas + 全屏 RawImage 来渲染转场遮罩。
/// </summary>
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    private Material _transitionMat;
    private bool _isTransitioning;
    private RawImage _image;

    [Header("转场参数")]
    public float fadeDuration = 1.2f;
    public Color fadeColor = Color.black;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 用 Resources.Load 加载 shader，Shader.Find 在运行时找不到未被引用的 shader
        Shader shader = Resources.Load<Shader>("material/ScreenTransition");
        if (shader == null)
        {
            Debug.LogError("SceneTransition: 找不到 Resources/material/ScreenTransition shader");
            return;
        }
        _transitionMat = new Material(shader);
        _transitionMat.SetColor("_Color", fadeColor);
        _transitionMat.SetFloat("_Progress", 0f);

        CreateOverlayCanvas();
    }

    private void CreateOverlayCanvas()
    {
        GameObject canvasGo = new GameObject("TransitionCanvas");
        canvasGo.transform.SetParent(transform);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        GameObject imgGo = new GameObject("TransitionImage");
        imgGo.transform.SetParent(canvasGo.transform, false);
        _image = imgGo.AddComponent<RawImage>();
        _image.material = _transitionMat;
        _image.color = Color.white;

        RectTransform rt = _image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        _image.enabled = false;
    }
    public void TransitionToScene(string sceneName, Action onSceneLoaded = null)
    {
        if (_isTransitioning || _image == null) return;
        _isTransitioning = true;
        _image.enabled = true;

        // 初始状态：全透明（Progress=1）
        _transitionMat.SetFloat("_Progress", 1f);

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        // 淡入：圆从边缘向中心覆盖 → 全黑（1→0）
        seq.Append(DOTween.To(
            () => _transitionMat.GetFloat("_Progress"),
            v => _transitionMat.SetFloat("_Progress", v),
            0f, fadeDuration
        ).SetEase(Ease.InQuad).SetUpdate(true));

        // 淡入结束后加载场景
        seq.AppendCallback(() =>
        {
            StartCoroutine(LoadSceneThenFadeOut(sceneName, onSceneLoaded));
        });
    }

    private IEnumerator LoadSceneThenFadeOut(string sceneName, Action onSceneLoaded)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        onSceneLoaded?.Invoke();

        // 淡出：圆从中心向外缩小 → 全透明（0→1）
        DOTween.To(
            () => _transitionMat.GetFloat("_Progress"),
            v => _transitionMat.SetFloat("_Progress", v),
            1f, fadeDuration
        ).SetEase(Ease.OutQuad).SetUpdate(true).OnComplete(() =>
        {
            _image.enabled = false;
            _isTransitioning = false;
        });
    }

    void OnDestroy()
    {
        if (_transitionMat != null)
        {
            Destroy(_transitionMat);
        }
    }
}
