using System.Collections;
using UnityEngine;
using TMPro;

public class SleepZFloat : MonoBehaviour
{
    [Header("Move")]
    public float floatSpeed = 0.5f;
    public float lifeTime = 2f;

    [Header("Fade")]
    public float fadeSpeed = 1f;

    public TextMeshPro text;
    private Color originalColor;

    private void Awake()
    {
        text = GetComponent<TextMeshPro>();
        if (text != null)
            originalColor = text.color;
    }

    public void Play()
    {
        StartCoroutine(FloatingRoutine());
    }

    private IEnumerator FloatingRoutine()
    {
        float t = 0f;

        Vector3 start = transform.position;

        while (t < lifeTime)
        {
            t += Time.deltaTime;

            // 向上漂浮
            transform.position = start + Vector3.up * (floatSpeed * t);

            // 渐隐
            if (text != null)
            {
                Color c = originalColor;
                c.a = Mathf.Lerp(1f, 0f, t / lifeTime);
                text.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}