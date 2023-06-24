using Assets.Common;
using UnityEngine;

public class PickupClawRope : MonoBehaviour
{
    public GameObject Halalit;
    public GameObject PickupClawBase;

    private SpriteRenderer _sprite;

    void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        UpdateSize();
        UpdateCenterPosition();
        UpdateRotation();
    }

    private void UpdateSize()
    {
        float ropeLength = Utils.GetDistanceBetweenTwoPoints(Halalit.transform.position, PickupClawBase.transform.position);
        _sprite.size = new Vector2(_sprite.size.x, ropeLength);
    }

    private void UpdateCenterPosition()
    {
        float newXPosition = (Halalit.transform.position.x + PickupClawBase.transform.position.x) / 2;
        float newYPosition = (Halalit.transform.position.y + PickupClawBase.transform.position.y) / 2;

        transform.position = new Vector3(newXPosition, newYPosition, 0);
    }

    private void UpdateRotation()
    {

    }
}
