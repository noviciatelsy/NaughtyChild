using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Throwable : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float throwForce = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnPicked()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        GetComponent<Collider>().enabled = false;
    }

    public void OnThrow(Vector3 dir)
    {
        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;
        GetComponent<Collider>().enabled = true;

        rb.velocity = Vector3.zero;
        rb.AddForce(dir * throwForce, ForceMode.Impulse);
    }

    public virtual void OnUse(Interact target)
    {
        Debug.Log("用物体 interacting: " + target.name);

        // 默认行为：触发目标的交互
        target.InteractObject();
    }
}