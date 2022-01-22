using Assets.Common;
using Assets.Enums;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PickupClawController : MonoBehaviour
{
    private const float POSITION_MIN_PROXIMITY = 0.2f;
    private const float ROTATION_MIN_PROXIMITY = 1;

    public float VelocityMultiplier;
    public float RopeLength;
    public GameObject Halalit;

    private PickupClawStatus _pcStatus;
    private Rigidbody2D _rigidBody;
    private Animator _animator;
    private GameObject _item;
    private Vector2 _pcDirection;
    private Vector2 _halalitCurrentPosition;
    private Vector2 _halalitLastFramePosition;
    private Vector2 _shootPoint;
    private float _initialRotation;
    private float _grabDelayTimer;
    private bool _perfectRotationToHalalit;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _initialRotation = transform.rotation.eulerAngles.z;
        _grabDelayTimer = 0;
        _pcStatus = PickupClawStatus.IN_HALALIT;
        _item = null;
        _halalitLastFramePosition = Halalit.transform.position;
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
        _perfectRotationToHalalit = false;
    }

    void Update()
    {
        _halalitCurrentPosition = Halalit.transform.position;

        ShootingController();
        GrabbingController();
        RetractionController();

        _halalitLastFramePosition = Halalit.transform.position;
    }

    #region Shooting & Retraction controllers

    private void ShootingController()
    {
        if (ShouldShoot())
        {
            InitShooting();
            Move(_shootPoint);
        }

        if (ShouldRetract())
        {
            _animator.SetBool("isShooting", false);
            _animator.SetBool("isGrabbing", false);
        }
    }

    private void GrabbingController()
    {
        if (_pcStatus == PickupClawStatus.GRABBING)
        {
            _grabDelayTimer += Time.deltaTime;
            _animator.SetBool("isGrabbing", true);
            //TODO: move a little towards item
            //TODO: rotate towards item
            //TODO: do a grab animation

            if (_grabDelayTimer >= 0.4)
            {
                _item.transform.SetParent(transform);
                _pcStatus = PickupClawStatus.MOVING_BACKWARD;
            }
        }
    }

    private void RetractionController()
    {
        if (ShouldRetract())
        {
            if (_pcStatus == PickupClawStatus.MOVING_FORWARD)
            {
                _animator.SetBool("isNotGrabbing", true);
            }

            _pcStatus = PickupClawStatus.MOVING_BACKWARD;
        }

        if (_pcStatus == PickupClawStatus.MOVING_BACKWARD)
        {
            if (ReachGoal(_halalitCurrentPosition))
            {
                FinalizeRetraction();
            }
            else
            {
                Move(_halalitCurrentPosition);
            }
        }
    }

    private void InitShooting()
    {
        _animator.SetBool("isShooting", true);
        _shootPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        _pcStatus = PickupClawStatus.MOVING_FORWARD;
        gameObject.transform.parent = null;
        transform.position = new Vector3(transform.position.x, transform.position.y, 1);
        _perfectRotationToHalalit = false;
    }

    private void FinalizeRetraction()
    {
        _animator.SetBool("isNotGrabbing", false);
        gameObject.transform.SetParent(Halalit.transform);
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
        _rigidBody.velocity = Vector2.zero;
        transform.position = new Vector3(_halalitCurrentPosition.x, _halalitCurrentPosition.y, 1);
        _pcStatus = PickupClawStatus.IN_HALALIT;

        if (_item != null)
        {
            // TODO: Load the Item (Each item should implement an interface with a function LoadItem - OOP)
            Destroy(_item);
        }
    }

    #endregion

    #region Moving

    private void Move(Vector2 goal)
    {
        UpdatePcDirection(goal);

        if (_pcStatus == PickupClawStatus.MOVING_BACKWARD)
        {
            MoveRelativeToHalalit();
        }

        if (_pcStatus == PickupClawStatus.MOVING_FORWARD || _perfectRotationToHalalit)
        {
            RotateToDirectionInstantly();
        }
        else if (!_perfectRotationToHalalit)
        {
            RotateOpositeToPcDirectionSlowly();
        }

        UpdateVelocityByPcDirection();
    }

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

        transform.position = new Vector3(transform.position.x + relativeDeltaX, transform.position.y + relativeDeltaY, 1);
    }

    private void RotateToDirectionInstantly()
    {
        float pcDirectionAngle = Utils.Vector2ToDegree(_pcDirection.x, _pcDirection.y);
        float pcRotation = transform.rotation.eulerAngles.z - _initialRotation; ;

        if (_pcStatus == PickupClawStatus.MOVING_BACKWARD)
        {
            pcRotation += 180;
        }

        transform.Rotate(new Vector3(0, 0, pcDirectionAngle - pcRotation));
    }

    private void RotateOpositeToPcDirectionSlowly()
    {
        float opositePcRotation = Utils.AngleNormalizationBy360(transform.rotation.eulerAngles.z - _initialRotation + 180);
        float pcDirectionRotation = Utils.AngleNormalizationBy360(Utils.Vector2ToDegree(_pcDirection.x, _pcDirection.y));
        float rotationAntiClockwiseDeference = Utils.AngleNormalizationBy360(pcDirectionRotation - opositePcRotation);
        float rotationClockwiseDeference = 360 - rotationAntiClockwiseDeference;

        if (rotationAntiClockwiseDeference < ROTATION_MIN_PROXIMITY || rotationClockwiseDeference < ROTATION_MIN_PROXIMITY)
        {
            _perfectRotationToHalalit = true;
        }
        else if (rotationAntiClockwiseDeference <= rotationClockwiseDeference)
        {
            transform.Rotate(0, 0, 0.5f);
        }
        else
        {
            transform.Rotate(0, 0, -0.5f);
        }
    }

    private void UpdateVelocityByPcDirection()
    {
        float horizontalVelocity = _pcDirection.x * VelocityMultiplier;
        float verticalVelocity = _pcDirection.y * VelocityMultiplier;

        _rigidBody.velocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    #endregion

    #region Grabbing init

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Item") && _pcStatus == PickupClawStatus.MOVING_FORWARD)
        {
            _pcStatus = PickupClawStatus.GRABBING;
            _grabDelayTimer = 0;
            _rigidBody.velocity = Vector2.zero;
            _item = other.gameObject;
        }
    }

    #endregion

    #region Predicates

    private bool ShouldShoot()
    {
        return Input.GetMouseButtonDown(0) && _pcStatus == PickupClawStatus.IN_HALALIT && ShootPointIsValid();
    }

    private bool ShouldRetract()
    {
        return _pcStatus == PickupClawStatus.MOVING_FORWARD && (ReachGoal(_shootPoint) || AtFullRopeLength()) ||
               _pcStatus == PickupClawStatus.MOVING_BACKWARD;
    }

    private bool AtFullRopeLength()
    {
        return Utils.GetDistanceBetweenTwoPoints(transform.position, _halalitCurrentPosition) >= RopeLength;
    }

    private bool ShootPointIsValid()
    {
        return !EventSystem.current.IsPointerOverGameObject();
    }

    private bool ReachGoal(Vector2 goal)
    {
        return Utils.GetDistanceBetweenTwoPoints(transform.position, goal) <= POSITION_MIN_PROXIMITY;
    }

    #endregion
}
