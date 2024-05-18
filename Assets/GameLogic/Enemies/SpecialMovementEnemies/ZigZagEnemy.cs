using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class ZigZagEnemy : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _changeZigZagDirectionInterval;
    [SerializeField]
    private float _changeFromDirectionAngle;

    private Vector2 _direction;
    private ZigZagDirection _zigZagDirectionFlag;
    private float _changeZigZagDirectionTime, _startMovementTime;

    void Start()
    {
        _zigZagDirectionFlag = ZigZagDirection.ZAG;
        _direction = Utils.GetRandomVector2OnCircle();
        SetChangeZigZagDirectionTime();
    }

    void FixedUpdate()
    {
        if (DidZigZagTimePass())
        {
            ChangeZigZagDirection();
        }

        EnemyMovementUtils.MoveInStraightLine(_rigidBody, _direction, _movementAmplitude, _changeZigZagDirectionInterval, _startMovementTime);

        SpeedLimiter.LimitSpeed(_rigidBody);
    }

    private void ChangeZigZagDirection()
    {
        if (_zigZagDirectionFlag == ZigZagDirection.ZIG)
        {
            _direction = Utils.AddAngleToVector(_direction, _changeFromDirectionAngle);
            _zigZagDirectionFlag = ZigZagDirection.ZAG;
        }
        else
        {
            _direction = Utils.AddAngleToVector(_direction, _changeFromDirectionAngle * (-1));
            _zigZagDirectionFlag = ZigZagDirection.ZIG;
        }
        SetChangeZigZagDirectionTime();
    }

    private void SetChangeZigZagDirectionTime()
    {
        _changeZigZagDirectionTime = Time.time + _changeZigZagDirectionInterval;
        _startMovementTime = Time.time;
    }

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.DidHitEdge(other.gameObject.tag))
        {
            SetChangeZigZagDirectionTime();
            _direction = EnemyUtils.GetAnotherDirectionFromEdge(_rigidBody, other.gameObject.tag);
        }
    }
    #endregion

    #region Predicates
    private bool DidZigZagTimePass()
    {
        return Time.time > _changeZigZagDirectionTime;
    }
    #endregion
}