using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsPlayMode")]
#endif

// Side Step: an enemy with this component jumps sideways when a straight player shot flies at it.
// A straight shot is a collider on the PlayerShots layer, tagged Shot, whose rigidbody moves by velocity.
public class ShotDodger : MonoBehaviour, IPausableEnemyBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _detectionRadius = 3f;
    [SerializeField]
    private float _checkInterval = 0.1f;
    [SerializeField]
    private float _dodgeImpulse = 6f;
    [SerializeField]
    private float _cooldown = 1.5f;
    [SerializeField]
    private float _threatHalfWidth = 1f;
    [SerializeField]
    private float _minShotSpeed = 1f;

    private const int SHOT_HITS_BUFFER_SIZE = 16;
    private const float DEAD_CENTRE_DISTANCE = 0.01f;

    private readonly Collider2D[] _shotHits = new Collider2D[SHOT_HITS_BUFFER_SIZE];
    private EnemySharedBehavior _enemySharedBehavior;
    private int _playerShotsLayerMask;
    private string _shotTag;
    private float _nextCheckTime;

    void Awake()
    {
        _enemySharedBehavior = GetComponent<EnemySharedBehavior>();
        _playerShotsLayerMask = LayerMask.GetMask(Layer.PLAYER_SHOTS.GetDescription());
        _shotTag = Tag.SHOT.GetDescription();
    }

    void FixedUpdate()
    {
        if (ShouldCheckForShots())
        {
            TryDodge();
        }

        SpeedLimiter.LimitSpeed(_rigidBody);
    }

    private void TryDodge()
    {
        _nextCheckTime = Utils.GetEndOfLifeTime(_checkInterval);

        if (TryFindThreat(out var shotPosition, out var shotVelocity))
        {
            Dodge(shotPosition, shotVelocity);
            _nextCheckTime = Utils.GetEndOfLifeTime(_cooldown);
        }
    }

    private bool TryFindThreat(out Vector2 shotPosition, out Vector2 shotVelocity)
    {
        var hitsCount = Physics2D.OverlapCircleNonAlloc(_rigidBody.position, _detectionRadius, _shotHits, _playerShotsLayerMask);
        var nearestDistance = float.MaxValue;
        shotPosition = Vector2.zero;
        shotVelocity = Vector2.zero;

        for (int i = 0; i < hitsCount; i++)
        {
            if (!IsThreat(_shotHits[i]))
            {
                continue;
            }

            var shotRigidbody = _shotHits[i].attachedRigidbody;
            var distance = Vector2.Distance(_rigidBody.position, shotRigidbody.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                shotPosition = shotRigidbody.position;
                shotVelocity = shotRigidbody.velocity;
            }
        }

        return nearestDistance < float.MaxValue;
    }

    private void Dodge(Vector2 shotPosition, Vector2 shotVelocity)
    {
        var side = GetOffsetFromShotLine(_rigidBody.position - shotPosition, shotVelocity);

        if (side.magnitude < DEAD_CENTRE_DISTANCE)
        {
            var leftOfShot = Vector2.Perpendicular(shotVelocity.normalized);
            side = _enemySharedBehavior._seedfulRandomGenerator.IsTrueIn50Precent() ? leftOfShot : -leftOfShot;
        }

        _rigidBody.AddForce(side.normalized * _dodgeImpulse, ForceMode2D.Impulse);
    }

    // The part of shotToDodger that is at right angles to the shot's flight: it points from the shot's line to the dodger.
    private static Vector2 GetOffsetFromShotLine(Vector2 shotToDodger, Vector2 shotVelocity)
    {
        var shotDirection = shotVelocity.normalized;
        return shotToDodger - Vector2.Dot(shotToDodger, shotDirection) * shotDirection;
    }

    #region Predicates
    private bool ShouldCheckForShots()
    {
        return Utils.ShouldDie(_nextCheckTime);
    }

    private bool IsThreat(Collider2D hit)
    {
        var shotRigidbody = hit.attachedRigidbody;
        if (shotRigidbody == null || !hit.CompareTag(_shotTag))
        {
            return false;
        }

        var shotVelocity = shotRigidbody.velocity;
        if (shotVelocity.magnitude <= _minShotSpeed)
        {
            return false;
        }

        var shotToDodger = _rigidBody.position - shotRigidbody.position;
        return Vector2.Dot(shotToDodger, shotVelocity) > 0 && GetOffsetFromShotLine(shotToDodger, shotVelocity).magnitude <= _threatHalfWidth;
    }
    #endregion

#if UNITY_EDITOR
    internal float GetCooldown()
    {
        return _cooldown;
    }
#endif
}