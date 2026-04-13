using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class AchievementPopup : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private AchievementItem achievementItem; 

    [Header("音效")]
    [SerializeField] private AudioClip unlockSound;
    private AudioSource _audioSource;

    [Header("动画参数")]
    [SerializeField] private float showPosY;     
    [SerializeField] private float hidePosY;    
    [SerializeField] private float slideTime = 0.5f;     
    [SerializeField] private float displayTime = 3f;     

    private RectTransform _rect;
    private Queue<AchievementSO> _queue = new Queue<AchievementSO>();
    private bool _isShowing;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        // 初始位置放到屏幕外
        _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, hidePosY);
    }

    void Start()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked += OnUnlocked;
    }

    void OnDestroy()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked -= OnUnlocked;
    }

    private void OnUnlocked(AchievementSO achievement)
    {
        _queue.Enqueue(achievement);
        if (!_isShowing)
            StartCoroutine(ProcessQueue());
    }
    private IEnumerator ProcessQueue()
    {
        _isShowing = true;

        while (_queue.Count > 0)
        {
            AchievementSO ach = _queue.Dequeue();
            yield return StartCoroutine(ShowPopup(ach));
        }

        _isShowing = false;
    }

    private IEnumerator ShowPopup(AchievementSO achievement)
    {
        // 填充数据
        if (achievementItem != null)
            achievementItem.SetData(achievement);

        // 播放音效
        if (unlockSound != null && _audioSource != null)
            _audioSource.PlayOneShot(unlockSound);

        gameObject.SetActive(true);
        _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, hidePosY);

        // 滑入
        Tween slideIn = _rect.DOAnchorPosY(showPosY, slideTime)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
        yield return slideIn.WaitForCompletion();

        // 停留
        float waited = 0f;
        while (waited < displayTime)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        // 滑出
        Tween slideOut = _rect.DOAnchorPosY(hidePosY, slideTime)
            .SetEase(Ease.InCubic)
            .SetUpdate(true);
        yield return slideOut.WaitForCompletion();

        gameObject.SetActive(false);
    }
}
