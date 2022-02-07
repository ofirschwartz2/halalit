using Assets.Common;
using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PickupClawController : MonoBehaviour
{
    private const float POSITION_MIN_PROXIMITY = 0.2f;
    private const float ROTATION_MIN_PROXIMITY = 1;
    private const float DRAG_AT_FULL_ROPE_SPEED = 0.1f;

    public bool UseConfigFile;
    public float VelocityMultiplier;
    public float RotationToItemMultiplier;
    public float RotationToHalalitMultiplier;
    public float MoveToItemMultiplier;
    public float GrabDelay;
    public float RopeLength;
    public GameObject Halalit;
    public GameObject Gun;

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

        if (UseConfigFile)
        {
            string[] props = { "VelocityMultiplier", "RotationToItemMultiplier", "RotationToHalalitMultiplier", "MoveToItemMultiplier", "GrabDelay", "RopeLength" };
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            VelocityMultiplier = propsFromConfig["VelocityMultiplier"];
            RotationToItemMultiplier = propsFromConfig["RotationToItemMultiplier"];
            RotationToHalalitMultiplier = propsFromConfig["RotationToHalalitMultiplier"];
            MoveToItemMultiplier = propsFromConfig["MoveToItemMultiplier"];
            GrabDelay = propsFromConfig["GrabDelay"];
            RopeLength = propsFromConfig["RopeLength"];
        }
    }

    void Update()
    {
        _halalitCurrentPosition = Halalit.transform.position;

        ShootingController();
        GrabbingController();
        RetractionController();

        _halalitLastFramePosition = Halalit.transform.position;
    }

    #region Controllers

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
            _animator.SetBool("isShooting", false);

            MoveToItem();

            if (_grabDelayTimer >= GrabDelay)
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

    #endregion

    #region Initializes & finalizes 

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
            _item.SendMessage("LoadItem", Gun);
            Destroy(_item);
        }
    }

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

    #region Moving

    private void Move(Vector2 goal)
    {
        UpdatePcDirection(goal);

        if (_pcStatus == PickupClawStatus.MOVING_BACKWARD && AtFullRopeLength())
        {
            Drag();
        }

        if (_pcStatus == PickupClawStatus.MOVING_FORWARD || _perfectRotationToHalalit)
        {
            RotateToPcDirectionInstantly();
        }
        else if (!_perfectRotationToHalalit)
        {
            RotateToDirectionSlowly(_pcDirection, RotationToHalalitMultiplier, true);
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

    private void Drag()
    {
        float halalitDeltaX = (_halalitCurrentPosition.x - _halalitLastFramePosition.x);
        float halalitDeltaY = (_halalitCurrentPosition.y - _halalitLastFramePosition.y);
        Vector3 newPosition = new Vector3(transform.position.x + halalitDeltaX, transform.position.y + halalitDeltaY, transform.position.z);

        transform.position = Vector3.MoveTowards(transform.position, newPosition, DRAG_AT_FULL_ROPE_SPEED);
    }

    private void RotateToPcDirectionInstantly()
    {
        float pcDirectionAngle = Utils.Vector2ToDegree(_pcDirection.x, _pcDirection.y);
        float pcRotation = transform.rotation.eulerAngles.z - _initialRotation; ;

        if (_pcStatus == PickupClawStatus.MOVING_BACKWARD)
        {
            pcRotation += 180;
        }

        transform.Rotate(new Vector3(0, 0, pcDirectionAngle - pcRotation));
    }

    private void RotateToDirectionSlowly(Vector2 goal, float rotationMultiplier, bool oposite = false)
    {
        float pcRotation = !oposite ? 
            Utils.AngleNormalizationBy360(transform.rotation.eulerAngles.z - _initialRotation) :
            Utils.AngleNormalizationBy360(transform.rotation.eulerAngles.z - _initialRotation + 180);
        float directionRotation = Utils.AngleNormalizationBy360(Utils.Vector2ToDegree(goal.x, goal.y));
        float rotationAntiClockwiseDeference = Utils.AngleNormalizationBy360(directionRotation - pcRotation);
        float rotationClockwiseDeference = 360 - rotationAntiClockwiseDeference;
        
        if (rotationAntiClockwiseDeference < ROTATION_MIN_PROXIMITY || rotationClockwiseDeference < ROTATION_MIN_PROXIMITY)
        {
            _perfectRotationToHalalit = true;
        }
        else if (rotationAntiClockwiseDeference <= rotationClockwiseDeference)
        {
            transform.Rotate(0, 0, rotationMultiplier);
        }
        else
        {
            transform.Rotate(0, 0, -rotationMultiplier);
        }
    }

    private void UpdateVelocityByPcDirection()
    {
        float horizontalVelocity = _pcDirection.x * VelocityMultiplier;
        float verticalVelocity = _pcDirection.y * VelocityMultiplier;

        _rigidBody.velocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    private void MoveToItem()
    {
        Vector2 directionToItem = Utils.GetDirectionVector(transform.position, _item.transform.position);

        MoveCloserToItem();
        RotateToDirectionSlowly(directionToItem, RotationToItemMultiplier);
    }

    private void MoveCloserToItem()
    {
        Vector3 directionToItem = Utils.GetDirectionVector(transform.position, _item.transform.position);
        transform.position += directionToItem * Time.deltaTime * MoveToItemMultiplier;
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