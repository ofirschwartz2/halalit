using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField]
    private Transform _background;

    [SerializeField]
    private float _parallaxFactor;

    private Vector3 previousCameraPosition;

    void Start()
    {
        previousCameraPosition = Camera.main.transform.position;
    }

    void Update()
    {
        Vector3 deltaMovement = Camera.main.transform.position - previousCameraPosition;
        Vector3 parallaxPosition = _background.position + deltaMovement * _parallaxFactor;
        _background.position = new Vector3(parallaxPosition.x, parallaxPosition.y, _background.position.z);

        previousCameraPosition = Camera.main.transform.position;
    }
}