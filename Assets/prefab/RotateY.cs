using UnityEngine;

public class SpindleRotate : MonoBehaviour
{
    [Header("基础自转速度（Y轴）")]
    public float spinSpeed = 120f;

    [Header("摆动幅度（纺锤形厚度）")]
    public float swingAngle = 30f;

    [Header("摆动频率")]
    public float swingFrequency = 1.5f;

    [Header("是否启用")]
    public bool isRotating = true;

    private float time;

    void Update()
    {
        if (!isRotating) return;

        time += Time.deltaTime;

        // =========================
        // 1️ Y轴自转
        // =========================
        float y = spinSpeed * Time.deltaTime;

        // =========================
        // 2️ Z轴摆动（纺锤核心）
        // =========================
        float z = Mathf.Sin(time * swingFrequency) * swingAngle;

        // =========================
        // 3️ 应用旋转
        // =========================
        transform.Rotate(0f, y, z, Space.Self);
    }
}