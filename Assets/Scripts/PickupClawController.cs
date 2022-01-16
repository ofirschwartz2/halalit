using Assets.Common;
using System;
using UnityEngine;

public class PickupClawController : MonoBehaviour
{
    private const float MAX_PROXIMITY = 0.3f;

    public float VelocityMultiplier;
    public float RopeLength;
    public GameObject Halalit;

    private Rigidbody2D _rigidBody;
    private Vector2 _shootDirection;
    private Vector2 _halalitCurrentPosition;
    private Vector2 _halalitLastFramePsotion;
    private Vector2 _shootPoint;
    private float _initialRotationl;
    private bool _movingForward;
    private bool _movingBackward;
    private bool _caughtSomething;


    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _initialRotationl = transform.rotation.eulerAngles.z;
        _movingForward = false;
        _movingBackward = false;
        _caughtSomething = false;
        _halalitLastFramePsotion = Halalit.transform.position;

        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        _halalitCurrentPosition = Halalit.transform.position;

        Shoot();
        Retract();

        _halalitLastFramePsotion = Halalit.transform.position;
    }

    #region Forward controllers

    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0) && IsInHalalit())
        {
            SetShootPointByMousePosition();
           
            if (ShootPointIsValid() && !_movingForward)
            {
                UpdateShootDirection();
                SetShootingVariables();
            }
        }

        if (ReachShootPoint() || AtFullRopeLength())
        {
            _rigidBody.velocity = new Vector2(0, 0); //TODO: remove this
            _movingForward = false;
        }
        else if (_movingForward)
        {
            MoveForward();
        }
    }

    private void SetShootingVariables()
    {
        _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        gameObject.transform.parent = null;
        _movingForward = true;
    }

    private void MoveForward()
    {
        UpdateShootDirection();
        MoveRelativeToHalalit();
        RotateInShootDirectionRelativeToHalalit();
        UpdateVelocityByShootDirection();
    }

    #endregion

    #region Backward controllers

    private void Retract()
    {

    }

    #endregion

    #region Moving

    private void SetShootPointByMousePosition()
    {
        _shootPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void UpdateShootDirection()
    {
        float deltaX = Math.Abs(transform.position.x - _shootPoint.x);
        float deltaY = Math.Abs(transform.position.y - _shootPoint.y);

        float relativeDeltaX = transform.position.x < _shootPoint.x ? deltaX : deltaX * -1;
        float relativeDeltaY = transform.position.y < _shootPoint.y ? deltaY : deltaY * -1;

        float shootDirectionMagnitude = Utils.GetVectorMagnitude(new Vector2(relativeDeltaX, relativeDeltaY));

        _shootDirection = new Vector2(relativeDeltaX / shootDirectionMagnitude, relativeDeltaY / shootDirectionMagnitude);
    }

    private void MoveRelativeToHalalit()
    {
        float halalitToShootPointDistance = Utils.GetDistanceBetweenTwoPoints(_halalitCurrentPosition, _shootPoint);
        float clawToShootPointDistance = Utils.GetDistanceBetweenTwoPoints(transform.position, _shootPoint);
        float relativeMultiplier = clawToShootPointDistance / halalitToShootPointDistance;

        float relativeDeltaX = relativeMultiplier * (_halalitCurrentPosition.x - _halalitLastFramePsotion.x);
        float relativeDeltaY = relativeMultiplier * (_halalitCurrentPosition.y - _halalitLastFramePsotion.y);

        transform.position = new Vector2(transform.position.x + relativeDeltaX, transform.position.y + relativeDeltaY);
    }

    private void RotateInShootDirectionRelativeToHalalit()
    {
        float shootDirectionAngle = Utils.Vector2ToDegree(_shootDirection.x, _shootDirection.y);
        float clawDirectionAngle = transform.rotation.eulerAngles.z - _initialRotationl;
        transform.Rotate(new Vector3(0, 0, shootDirectionAngle - clawDirectionAngle));
    }

    private void UpdateVelocityByShootDirection()
    {
        float horizontalVelocity = _shootDirection.x * VelocityMultiplier;
        float verticalVelocity = _shootDirection.y * VelocityMultiplier;

        _rigidBody.velocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    #endregion

    #region Predicates

    private bool AtFullRopeLength()
    {
        float deltaX = Math.Abs(transform.position.x - _halalitCurrentPosition.x);
        float deltaY = Math.Abs(transform.position.y - _halalitCurrentPosition.y);

        return Utils.GetLengthOfLine(deltaX, deltaY) >= RopeLength;
    }

    private bool ShootPointIsValid()
    {
        // TODO: fix this... this should include all screen but not include the most left and right parts of the canvas.
        return _shootPoint.x > -8 && _shootPoint.x < 8; 
    }

    private bool IsInHalalit()
    {
        return !_movingForward && !_movingBackward;
    }

    private bool ReachShootPoint()
    {
        return 
            transform.position.x < _shootPoint.x + MAX_PROXIMITY &&
            transform.position.x > _shootPoint.x - MAX_PROXIMITY &&
            transform.position.y < _shootPoint.y + MAX_PROXIMITY &&
            transform.position.y > _shootPoint.y - MAX_PROXIMITY;
    }

    #endregion
}
