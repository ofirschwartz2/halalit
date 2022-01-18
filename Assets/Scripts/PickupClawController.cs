using Assets.Common;
using System;
using UnityEngine;

public class PickupClawController : MonoBehaviour
{
    private const float MIN_PROXIMITY = 0.3f;

    public float VelocityMultiplier;
    public float RopeLength;
    public GameObject Halalit;

    private Rigidbody2D _rigidBody;
    private Vector2 _pcDirection;
    private Vector2 _halalitCurrentPosition;
    private Vector2 _halalitLastFramePosition;
    private Vector2 _shootPoint;
    private float _initialRotation;
    private bool _movingForward;
    private bool _movingBackward;
    private bool _caughtSomething;


    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _initialRotation = transform.rotation.eulerAngles.z;
        _movingForward = false;
        _movingBackward = false;
        _caughtSomething = false;
        _halalitLastFramePosition = Halalit.transform.position;
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        _halalitCurrentPosition = Halalit.transform.position;

        Shoot();
        Retract();

        _halalitLastFramePosition = Halalit.transform.position;
    }

    #region Shoot & Retract controllers

    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0) && PcBeforeShoot() && ShootPointIsValid())
        {
            InitShooting();
        }

        if (_movingForward)
        {
            if (ReachGoal(_shootPoint) || AtFullRopeLength())
            {
                _caughtSomething = true;
                _movingForward = false;
            }
            else if (_movingForward)
            {
                Move(_shootPoint, false);
            }
        }
    }

    private void Retract()
    {
        if (_caughtSomething || AtFullRopeLength())
        {
            _movingBackward = true;
        }

        if (_movingBackward)
        {
            if (ReachGoal(_halalitCurrentPosition))
            {
                FinalizeRetraction();
            }
            else if (_movingBackward)
            {
                Move(_halalitCurrentPosition, true);
            }
        }
    }

    private void Move(Vector2 goal, bool isBackward)
    {
        UpdatePcDirection(goal);
        MoveRelativeToHalalit();
        RotateInPcDirectionRelativeToHalalit(isBackward);
        UpdateVelocityByPcDirection();
    }

    private void InitShooting()
    {
        _shootPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        _movingForward = true;
        gameObject.transform.parent = null;
    }

    private void FinalizeRetraction()
    {
        
        transform.position = _halalitCurrentPosition;
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
        _rigidBody.velocity = Vector2.zero;
        _caughtSomething = false;
        _movingBackward = false;
        gameObject.transform.SetParent(Halalit.transform);
    }

    #endregion

    #region Moving

    private void UpdatePcDirection(Vector2 goal)
    {
        float deltaX = Math.Abs(transform.position.x - goal.x);
        float deltaY = Math.Abs(transform.position.y - goal.y);

        float relativeDeltaX = transform.position.x < goal.x ? deltaX : deltaX * -1;
        float relativeDeltaY = transform.position.y < goal.y ? deltaY : deltaY * -1;

        float shootDirectionMagnitude = Utils.GetVectorMagnitude(new Vector2(relativeDeltaX, relativeDeltaY));

        _pcDirection = new Vector2(relativeDeltaX / shootDirectionMagnitude, relativeDeltaY / shootDirectionMagnitude);
    }

    private void MoveRelativeToHalalit()
    {
        float halalitToShootPointDistance = Utils.GetDistanceBetweenTwoPoints(_halalitCurrentPosition, _shootPoint);
        float clawToShootPointDistance = Utils.GetDistanceBetweenTwoPoints(transform.position, _shootPoint);
        float relativeMultiplier = clawToShootPointDistance / halalitToShootPointDistance;

        float relativeDeltaX = relativeMultiplier * (_halalitCurrentPosition.x - _halalitLastFramePosition.x);
        float relativeDeltaY = relativeMultiplier * (_halalitCurrentPosition.y - _halalitLastFramePosition.y);

        transform.position = new Vector2(transform.position.x + relativeDeltaX, transform.position.y + relativeDeltaY);
    }

    private void RotateInPcDirectionRelativeToHalalit(bool isBackward)
    {
        float pcDirectionAngle = Utils.Vector2ToDegree(_pcDirection.x, _pcDirection.y);
        float pcRotation = transform.rotation.eulerAngles.z - _initialRotation; ;

        if (isBackward)
        {
            pcRotation += 180;
        }
        
        transform.Rotate(new Vector3(0, 0, pcDirectionAngle - pcRotation));
    }

    private void UpdateVelocityByPcDirection()
    {
        float horizontalVelocity = _pcDirection.x * VelocityMultiplier;
        float verticalVelocity = _pcDirection.y * VelocityMultiplier;

        _rigidBody.velocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    #endregion

    #region Predicates

    private bool AtFullRopeLength()
    {
        return Utils.GetDistanceBetweenTwoPoints(transform.position, _halalitCurrentPosition) >= RopeLength;
    }

    private bool ShootPointIsValid()
    {
        // TODO: fix this... this should include all screen but not include the most left and right parts of the canvas.
        Vector2 potentialShootPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return potentialShootPoint.x > -8 && potentialShootPoint.x < 8; 
    }

    private bool PcBeforeShoot()
    {
        return !_movingForward && !_movingBackward;
    }

    private bool ReachGoal(Vector2 goal)
    {
        return 
            transform.position.x < goal.x + MIN_PROXIMITY &&
            transform.position.x > goal.x - MIN_PROXIMITY &&
            transform.position.y < goal.y + MIN_PROXIMITY &&
            transform.position.y > goal.y - MIN_PROXIMITY;
    }

    #endregion
}
