using Assets.Models;
using Assets.Utils;
using UnityEngine;

// Tether Twins (#237): one enemy bank entry spawns two twins joined by a Tether. The twins keep _minSpacing to _maxSpacing apart,
// and the point between them drifts toward the enemy target. Both get the same drift, so the line sweeps without folding.
// When one twin dies, the survivor drifts on its own and the Tether becomes its spinning flail.
public class TetherTwin : MonoBehaviour, IPausableEnemyBehaviour, IPairedEnemy
{
    [SerializeField]
    private Tether _tetherPrefab;
    [SerializeField]
    private float _minSpacing = 6f;
    [SerializeField]
    private float _maxSpacing = 9f;
    [SerializeField]
    private float _spacingForce = 2f;
    [SerializeField]
    private float _driftForce = 1.5f;
    [SerializeField]
    private float _speedLimit = 2f;

    private Rigidbody2D _rigidBody;
    private Collider2D[] _ownColliders;
    private TetherTwin _partner;
    private Tether _tether;

    #region Init
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _ownColliders = GetComponents<Collider2D>();
    }

    // The spawn hole calls this on the first twin, with the second. The two share one tether, made from the prefab.
    public void PairWith(GameObject partner)
    {
        var partnerTwin = partner.GetComponent<TetherTwin>();
        var tether = Instantiate(_tetherPrefab, transform.position, Quaternion.identity, transform.parent);
        Link(partnerTwin, tether);
        partnerTwin.Link(this, tether);
        tether.Connect(this, partnerTwin);
    }

    private void Link(TetherTwin partner, Tether tether)
    {
        _partner = partner;
        _tether = tether;
    }
    #endregion

    #region Destroy
    // Wounded Retreat, the spawn hole and death switch this script off: the line drops in the same tick.
    // When the whole scene is destroyed, the tether may go first, hence the check.
    private void OnDisable()
    {
        if (_tether != null)
        {
            _tether.SwitchOff();
        }
    }
    #endregion

    void FixedUpdate()
    {
        if (_partner != null)
        {
            KeepSpacing();
            DriftFrom(GetMidpoint());
        }
        else
        {
            DriftFrom(transform.position);
        }

        SpeedLimiter.LimitSpeed(_rigidBody);
    }

    #region Movement
    private void KeepSpacing()
    {
        Vector2 toPartner = _partner.transform.position - transform.position;
        var spacing = toPartner.magnitude;

        if (IsTooClose(spacing))
        {
            EnemyUtils.MoveUnderSpeedLimit(_rigidBody, -toPartner.normalized, _spacingForce, _speedLimit);
        }
        else if (IsTooFar(spacing))
        {
            EnemyUtils.MoveUnderSpeedLimit(_rigidBody, toPartner.normalized, _spacingForce, _speedLimit);
        }
    }

    // Toward the enemy target, measured from the given point
    private void DriftFrom(Vector2 from)
    {
        Vector2 targetPosition = EnemyTarget.GetPosition(transform);
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, (targetPosition - from).normalized, _driftForce, _speedLimit);
    }

    private Vector2 GetMidpoint()
    {
        return (transform.position + _partner.transform.position) / 2f;
    }
    #endregion

    // The radius of a circle around the twin's centre that holds all its colliders, at any angle.
    // Measured from the body's position, where physics has the colliders, not from the transform.
    public float GetBodyRadius()
    {
        var bodyRadius = 0f;
        foreach (var ownCollider in _ownColliders)
        {
            var bounds = ownCollider.bounds;
            bodyRadius = Mathf.Max(bodyRadius, Vector2.Distance(_rigidBody.position, (Vector2)bounds.center) + ((Vector2)bounds.extents).magnitude);
        }

        return bodyRadius;
    }

    #region Predicates
    private bool IsTooClose(float spacing)
    {
        return spacing < _minSpacing;
    }

    private bool IsTooFar(float spacing)
    {
        return spacing > _maxSpacing;
    }
    #endregion

#if UNITY_EDITOR
    internal TetherTwin GetPartner()
    {
        return _partner;
    }

    internal Tether GetTether()
    {
        return _tether;
    }

    internal Tether GetTetherPrefab()
    {
        return _tetherPrefab;
    }

    internal float GetMinSpacing()
    {
        return _minSpacing;
    }

    internal float GetMaxSpacing()
    {
        return _maxSpacing;
    }
#endif
}
