using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FloatPopEffect : MonoBehaviour
{
    [Header("Motion")]
    public float riseSpeed = 0.5f;
    public float lifeTime = 1.2f;

    [Header("Scale")]
    public float startScale = 0.8f;
    public float endScale = 1.2f;

    [Header("Optional Random")]
    public Vector2 randomOffset = new Vector2(0.2f, 0.2f);

    private Image uiImage;
    private SpriteRenderer spriteRenderer;

    private Color originalColor;

    // =========================
    // 初始化（UI or Sprite 自动适配）
    // =========================
    public void Init(Sprite sprite = null)
    {
        uiImage = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (uiImage != null && sprite != null)
        {
            uiImage.sprite = sprite;
            originalColor = uiImage.color;
        }
        else if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
            originalColor = spriteRenderer.color;
        }

        StartCoroutine(Play());
    }

    // =========================
    // 主效果
    // =========================
    private IEnumerator Play()
    {
        float t = 0f;

        Vector3 startPos = transform.position;
        Vector3 startScaleVec = Vector3.one * startScale;
        Vector3 endScaleVec = Vector3.one * endScale;

        transform.localScale = startScaleVec;

        while (t < lifeTime)
        {
            t += Time.deltaTime;
            float k = t / lifeTime;

            // 上升
            transform.position = startPos + Vector3.up * (riseSpeed * t);

            // 放大
            transform.localScale = Vector3.Lerp(startScaleVec, endScaleVec, k);

            // 渐隐
            SetAlpha(1f - k);

            yield return null;
        }

        Destroy(gameObject);
    }

    // =========================
    // 统一透明控制
    // =========================
    private void SetAlpha(float a)
    {
        if (uiImage != null)
        {
            Color c = originalColor;
            c.a = a;
            uiImage.color = c;
        }

        if (spriteRenderer != null)
        {
            Color c = originalColor;
            c.a = a;
            spriteRenderer.color = c;
        }
    }
}