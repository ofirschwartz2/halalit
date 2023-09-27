using UnityEngine;

public class ParentRotationCounteractor : MonoBehaviour
{
    private Transform parentTransform;
    private Vector3 offset;

    private void Start()
    {
        parentTransform = transform.parent;
        offset = transform.position - parentTransform.position;
    }

    private void LateUpdate()
    {
        transform.SetPositionAndRotation(parentTransform.position + offset, Quaternion.Euler(0, 0, 0));
    }
}