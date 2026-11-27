using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

// Chain Spark (#229): a Ball Shot whose hit jumps on to the nearest unhit enemy or asteroid, a few times, for part of its power.
public class ChainSpark : BallShot
{
    [SerializeField]
    private float _jumpRadius = 3f;
    [SerializeField]
    private int _maxJumps = 3;
    [SerializeField]
    private float _jumpPowerFactor = 0.7f;
    [SerializeField]
    private int _minJumpHarm = 1;
    [SerializeField]
    private ChainSparkLine _jumpLinePrefab;

    private const int OVERLAP_BUFFER_SIZE = 64;

    private AttackBehaviour _attackBehaviour;
    private Collider2D[] _overlapBuffer;
    private int _targetsLayerMask;
    private string _enemyTag;
    private string _asteroidTag;
    private bool _didSpark;

    // Awake, not Start: the shot can hit in its first physics step, before its Start.
    private void Awake()
    {
        _attackBehaviour = GetComponent<AttackBehaviour>();
        _overlapBuffer = new Collider2D[OVERLAP_BUFFER_SIZE];
        _targetsLayerMask = LayerMask.GetMask(Layer.ENEMIES.GetDescription(), Layer.ASTEROIDS.GetDescription());
        _enemyTag = Tag.ENEMY.GetDescription();
        _asteroidTag = Tag.ASTEROID.GetDescription();
    }

    // One spark per bolt: Destroy waits for the end of the frame, so a bolt can enter more targets. The first hit decides, even a blocked one.
    protected override void OnHit(Collider2D other)
    {
        if (_didSpark)
        {
            return;
        }

        _didSpark = true;
        var firstTarget = other.attachedRigidbody;
        if (firstTarget == null || !IsTarget(firstTarget) || IsBlockedByArmor(firstTarget))
        {
            return;
        }

        RunChain(firstTarget);
    }

    private void RunChain(Rigidbody2D firstTarget)
    {
        var hitTargets = new HashSet<GameObject> { firstTarget.gameObject };
        var points = new List<Vector3> { firstTarget.position };
        var jumpHarm = GetJumpHarm();
        Vector2 origin = firstTarget.position;

        for (int jump = 0; jump < _maxJumps; jump++)
        {
            var nextTarget = FindNextTarget(origin, hitTargets);
            if (nextTarget == null)
            {
                break;
            }

            nextTarget.GetComponent<IHealth>().TryChangeHealth(-jumpHarm);
            hitTargets.Add(nextTarget.gameObject);
            origin = nextTarget.position;
            points.Add(origin);
        }

        if (points.Count > 1)
        {
            ShowJumpLine(points.ToArray());
        }
    }

    // The nearest live, unhit target in reach. Ties fall to x, then y, so the order of the overlap results never decides.
    private Rigidbody2D FindNextTarget(Vector2 origin, HashSet<GameObject> hitTargets)
    {
        var hitsCount = Physics2D.OverlapCircleNonAlloc(origin, _jumpRadius, _overlapBuffer, _targetsLayerMask);
        Rigidbody2D bestTarget = null;

        for (int i = 0; i < hitsCount; i++)
        {
            var candidate = _overlapBuffer[i].attachedRigidbody;
            if (candidate == null || !IsTarget(candidate) || hitTargets.Contains(candidate.gameObject) || !IsAlive(candidate))
            {
                continue;
            }

            if (bestTarget == null || Utils.IsNearerThan(origin, candidate.position, bestTarget.position))
            {
                bestTarget = candidate;
            }
        }

        return bestTarget;
    }

    private int GetJumpHarm()
    {
        return Mathf.Max(_minJumpHarm, Mathf.FloorToInt(_attackBehaviour.AttackStats.Power * _jumpPowerFactor));
    }

    private void ShowJumpLine(Vector3[] points)
    {
        var jumpLine = Instantiate(_jumpLinePrefab, transform.parent);
        jumpLine.Show(points);
    }

    #region Predicates
    private bool IsTarget(Rigidbody2D target)
    {
        return target.CompareTag(_enemyTag) || target.CompareTag(_asteroidTag);
    }

    // A bolt stopped by a front plate (Shield Ram, Mirror Shield) gives no spark.
    private bool IsBlockedByArmor(Rigidbody2D target)
    {
        var frontArmor = target.GetComponent<FrontArmor>();
        return frontArmor != null && frontArmor.IsBlocking(gameObject);
    }

    private bool IsAlive(Rigidbody2D target)
    {
        var health = target.GetComponent<IHealth>();
        return health != null && health.GetCurrentHealth() > 0;
    }
    #endregion

#if UNITY_EDITOR
    internal float GetJumpRadius()
    {
        return _jumpRadius;
    }

    internal int GetMaxJumps()
    {
        return _maxJumps;
    }

    internal ChainSparkLine GetJumpLinePrefab()
    {
        return _jumpLinePrefab;
    }
#endif
}
