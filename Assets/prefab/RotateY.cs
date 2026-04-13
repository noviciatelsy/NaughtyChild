using UnityEngine;

public class RotateY : MonoBehaviour
{
    [Header("旋转速度（度/秒）")]
    public float speed = 90f;

    [Header("是否启用旋转")]
    public bool isRotating = true;

    void Update()
    {
        if (!isRotating) return;

        // 每帧绕Y轴旋转
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }
}