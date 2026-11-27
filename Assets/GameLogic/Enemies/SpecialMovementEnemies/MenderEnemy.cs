using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using UnityEngine;

// Mender (#230): a weak enemy that never attacks. It keeps about _followDistance from the most hurt enemy within _followRadius,
// and drifts like Lame while nobody there is hurt. Every _healInterval it heals the most hurt enemy within _healRadius by _healAmount
// (Health caps the heal at max health) and shows _healBeam for _beamDuration. An asteroid between the two loses the heal.
// It never picks itself, so a lone Mender stays hurt and two Menders heal each other. It searches every _searchInterval, not every tick.
public class MenderEnemy : MonoBehaviour, IPausableEnemyBehaviour
{
    private const int HITS_BUFFER_SIZE = 32;

    [SerializeField]
    private LineRenderer _healBeam;
    [SerializeField]
    private float _moveForce = 1.5f;
    [SerializeField]
    private float _speedLimit = 1.5f;
    [SerializeField]
    private float _followRadius = 8f;
    [SerializeField]
    private float _followDistance = 3f;
    [SerializeField]
    private float _followTolerance = 0.5f;
    [SerializeField]
    private float _healRadius = 4f;
    [SerializeField]
    private float _healInterval = 2f;
    [SerializeField]
    private int _healAmount = 2;
    [SerializeField]
    private float _beamDuration = 0.5f;
    [SerializeField]
    private float _searchInterval = 0.25f;

    private readonly Collider2D[] _hits = new Collider2D[HITS_BUFFER_SIZE];
    private Rigidbody2D _rigidBody;
    private EnemySharedBehavior _enemySharedBehavior;
    private int _enemiesLayerMask;
    private int _asteroidsLayerMask;
    private string _enemyTag;
    private Vector2 _driftDirection;
    private Rigidbody2D _patient;
    private Rigidbody2D _beamTarget;
    private float _nextSearchTime;
    private float _nextHealTime;
    private float _beamEndTime;

    #region Init
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _enemySharedBehavior = GetComponent<EnemySharedBehavior>();
        _enemiesLayerMask = LayerMask.GetMask(Layer.ENEMIES.GetDescription());
        _asteroidsLayerMask = LayerMask.GetMask(Layer.ASTEROIDS.GetDescription());
        _enemyTag = Tag.ENEMY.GetDescription();
        _healBeam.useWorldSpace = true;
        _healBeam.positionCount = 2;
        HideBeam();
    }

    void Start()
    {
        _driftDirection = Utils.GetRandomVector2OnCircle(_enemySharedBehavior._seedfulRandomGenerator);
        _nextHealTime = Utils.GetEndOfLifeTime(_healInterval);
    }
    #endregion

    #region Destroy
    // Wounded Retreat and the spawn hole switch this script off: the beam must not stay on while it is off.
    // When the whole scene is destroyed, the beam's child object may go first, hence the check.
    private void OnDisable()
    {
        if (_healBeam != null)
        {
            HideBeam();
        }
    }
    #endregion

    void FixedUpdate()
    {
        if (ShouldSearch())
        {
            Search();
        }

        Move();
        UpdateBeam();

        SpeedLimiter.LimitSpeed(_rigidBody);
    }

    #region Search and heal
    // Every search picks the patient to follow: the most hurt enemy within the follow radius, or nobody. A heal that is due goes to
    // the most hurt enemy within the heal radius; with nobody there, it stays due and is tried again at the next search.
    private void Search()
    {
        _nextSearchTime = Utils.GetEndOfLifeTime(_searchInterval);
        TryFindPatient(_followRadius, out _patient);

        if (ShouldHeal() && TryFindPatient(_healRadius, out var healTarget))
        {
            TryHeal(healTarget);
        }
    }

    // The live, hurt enemy in the radius that misses the most health, never itself. A tie goes to Utils.IsNearerThan
    // (the nearer, then the lower x, then the lower y), so the order of the overlap results never decides.
    private bool TryFindPatient(float radius, out Rigidbody2D patient)
    {
        var hitsCount = Physics2D.OverlapCircleNonAlloc(_rigidBody.position, radius, _hits, _enemiesLayerMask);
        var patientMissingHealth = 0;
        patient = null;

        for (int i = 0; i < hitsCount; i++)
        {
            var candidate = _hits[i].attachedRigidbody;
            if (candidate == null || !candidate.CompareTag(_enemyTag) || candidate == _rigidBody)
            {
                continue;
            }

            var candidateHealth = candidate.GetComponent<IHealth>();
            if (!IsHurtAndAlive(candidateHealth))
            {
                continue;
            }

            var missingHealth = GetMissingHealth(candidateHealth);
            if (patient == null || IsMoreUrgent(missingHealth, candidate.position, patientMissingHealth, patient.position))
            {
                patient = candidate;
                patientMissingHealth = missingHealth;
            }
        }

        return patient != null;
    }

    // An asteroid in the way loses the heal. Either way, the next heal waits a whole interval.
    private void TryHeal(Rigidbody2D target)
    {
        _nextHealTime = Utils.GetEndOfLifeTime(_healInterval);

        if (IsBlockedByAsteroid(target))
        {
            return;
        }

        target.GetComponent<IHealth>().TryChangeHealth(_healAmount);
        ShowBeam(target);
    }

    private static int GetMissingHealth(IHealth health)
    {
        return health.GetMaxHealth() - health.GetCurrentHealth();
    }
    #endregion

    #region Movement
    private void Move()
    {
        if (_patient == null)
        {
            Drift();
        }
        else
        {
            Follow();
        }
    }

    private void Drift()
    {
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, _driftDirection, _moveForce, _speedLimit);
    }

    // Closes in when further than the follow distance plus the tolerance, backs off when nearer than it minus the tolerance.
    private void Follow()
    {
        var toPatient = _patient.position - _rigidBody.position;
        var distance = toPatient.magnitude;

        if (IsTooFar(distance))
        {
            EnemyUtils.MoveUnderSpeedLimit(_rigidBody, toPatient.normalized, _moveForce, _speedLimit);
        }
        else if (IsTooNear(distance))
        {
            EnemyUtils.MoveUnderSpeedLimit(_rigidBody, -toPatient.normalized, _moveForce, _speedLimit);
        }
    }
    #endregion

    #region Beam
    private void ShowBeam(Rigidbody2D target)
    {
        _beamTarget = target;
        _beamEndTime = Utils.GetEndOfLifeTime(_beamDuration);
        SetBeamPositions();
        _healBeam.enabled = true;
    }

    // While it shows, the beam follows both ends. It goes off when its time is up or the healed enemy is gone.
    private void UpdateBeam()
    {
        if (!_healBeam.enabled)
        {
            return;
        }

        if (_beamTarget == null || Utils.ShouldDie(_beamEndTime))
        {
            HideBeam();
        }
        else
        {
            SetBeamPositions();
        }
    }

    private void SetBeamPositions()
    {
        _healBeam.SetPosition(0, _rigidBody.position);
        _healBeam.SetPosition(1, _beamTarget.position);
    }

    private void HideBeam()
    {
        _healBeam.enabled = false;
        _beamTarget = null;
    }
    #endregion

    #region Predicates
    private bool ShouldSearch()
    {
        return Utils.ShouldDie(_nextSearchTime);
    }

    private bool ShouldHeal()
    {
        return Utils.ShouldDie(_nextHealTime);
    }

    private static bool IsHurtAndAlive(IHealth health)
    {
        return health != null && health.GetCurrentHealth() > 0 && GetMissingHealth(health) > 0;
    }

    // More missing health wins. A tie goes to Utils.IsNearerThan.
    private bool IsMoreUrgent(int missingHealth, Vector2 position, int bestMissingHealth, Vector2 bestPosition)
    {
        if (missingHealth != bestMissingHealth)
        {
            return missingHealth > bestMissingHealth;
        }

        return Utils.IsNearerThan(_rigidBody.position, position, bestPosition);
    }

    private bool IsBlockedByAsteroid(Rigidbody2D target)
    {
        return Physics2D.Linecast(_rigidBody.position, target.position, _asteroidsLayerMask).collider != null;
    }

    private bool IsTooFar(float distance)
    {
        return distance > _followDistance + _followTolerance;
    }

    private bool IsTooNear(float distance)
    {
        return distance < _followDistance - _followTolerance;
    }
    #endregion

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.DidHitEdge(other.gameObject.tag))
        {
            _driftDirection = EnemyUtils.GetAnotherDirectionFromEdge(_enemySharedBehavior._seedfulRandomGenerator, _rigidBody, other.gameObject.tag);
        }
    }
    #endregion

#if UNITY_EDITOR
    internal float GetHealInterval()
    {
        return _healInterval;
    }

    internal int GetHealAmount()
    {
        return _healAmount;
    }

    internal float GetFollowDistance()
    {
        return _followDistance;
    }

    internal bool IsBeamShown()
    {
        return _healBeam.enabled;
    }
#endif
}
