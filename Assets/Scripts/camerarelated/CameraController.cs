using UnityEngine;

public class CameraControler : MonoBehaviour
{
    private Transform target;
    public Vector3 offset;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void LateUpdate()
    {
        gameObject.transform.position = target.position + offset;
    }
}