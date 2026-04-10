using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [Header("是否只水平旋转")]
    [SerializeField] private bool lockY = true;

    private Transform cam;

    private void Start()
    {
        if (Camera.main != null)
            cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        Vector3 dir = transform.position - cam.position;

        if (lockY)
        {
            dir.y = 0;
        }

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}