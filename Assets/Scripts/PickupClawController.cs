using Assets.Common;
using System;
using UnityEngine;

public class PickupClawController : MonoBehaviour
{
    public bool MovingForward;
    public float ForceMultiplier;
    public float SpeedLimit;
    public float RopeLength;
    public GameObject Halalit;

    private Rigidbody2D _rigidBody;
    private Vector2 _sootingDirection;


    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        MovingForward = false;

        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        Shoot();

    }

    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePositionRelativeToHalalit = getMousePositionRelativeToHalalit();

            if (MousePositionIsValid(mousePositionRelativeToHalalit) && !MovingForward)
            {
                RotateInShootDirection(mousePositionRelativeToHalalit);
                SetShootingVariables(mousePositionRelativeToHalalit);
            }
        }

        if (MovingForward && !AtFullRopeLength())
        {
            MoveForward();
        }

        if (AtFullRopeLength())
        {
            _rigidBody.velocity = new Vector2(0, 0);
        }
    }

    private Vector2 getMousePositionRelativeToHalalit()
    {
        Vector2 mousePositionRelativeToWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        float deltaX = Math.Abs(transform.position.x - mousePositionRelativeToWorld.x);
        float deltaY = Math.Abs(transform.position.y - mousePositionRelativeToWorld.y);

        float relativeX = transform.position.x < mousePositionRelativeToWorld.x ? deltaX : deltaX * -1;
        float relativeY = transform.position.y < mousePositionRelativeToWorld.y ? deltaY : deltaY * -1;

        return new Vector2(relativeX, relativeY);
    }

    private bool MousePositionIsValid(Vector2 mousePosition)
    {
        return mousePosition.x > -10 && mousePosition.x < 10;
    }

    private void RotateInShootDirection(Vector2 mousePositionRelativeToHalalit)
    {
        float mouseDirection = Utils.Vector2ToDegree(mousePositionRelativeToHalalit.x, mousePositionRelativeToHalalit.y);
        float halatitDirection = transform.parent.transform.rotation.eulerAngles.z;
        transform.Rotate(new Vector3(0, 0, mouseDirection - halatitDirection));
    }

    private void SetShootingVariables(Vector2 mousePos)
    {
        _sootingDirection = mousePos;
        _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        gameObject.transform.parent = null;
        MovingForward = true;
    }

    private void MoveForward()
    {
        if (IsUnderSpeedLimit())
        {
            float horizontalForce = _sootingDirection.x * ForceMultiplier;
            float verticalForce = _sootingDirection.y * ForceMultiplier;

            _rigidBody.AddForce(new Vector2(horizontalForce, verticalForce));
        }
    }

    private bool IsUnderSpeedLimit()
    {
        return Utils.VectorToAbsoluteValue(_rigidBody.velocity) < SpeedLimit;
    } 

    private bool AtFullRopeLength()
    {
        float deltaX = Math.Abs(transform.position.x - Halalit.transform.position.x);
        float deltaY = Math.Abs(transform.position.y - Halalit.transform.position.y);

        return Utils.GetLengthOfLine(deltaX, deltaY) >= RopeLength;
    }
}
