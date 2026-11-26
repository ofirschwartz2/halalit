using Assets.Models;
using Assets.Utils;
using UnityEngine;

// Grudge (#216): when a shot or laser fired by another enemy hurts this enemy, it aims at that enemy instead of the Halalit
// for _duration seconds (EnemyTarget). A newer grudge replaces the old one. Enemies that do not aim are pushed towards
// the target by _steerForce; state-machine enemies set it to 0 in their prefab and only re-aim.
public class Grudge : MonoBehaviour, IPausableEnemyBehaviour
{
    [SerializeField]
    private float _duration = 4f;
    [SerializeField]
    private float _steerForce = 3f;
    [SerializeField]
    private float _steerSpeedLimit = 3f;

    private Rigidbody2D _rigidBody;
    private IHealth _health;

    #region Init
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _health = GetComponent<IHealth>();
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        _health.HarmedBy += OnHarmedBy;
    }
    #endregion

    #region Destroy
    private void OnDestroy()
    {
        DestroyEventListeners();
        EnemyTarget.ClearGrudge(transform);
    }

    private void DestroyEventListeners()
    {
        _health.HarmedBy -= OnHarmedBy;
    }
    #endregion

    void FixedUpdate()
    {
        if (ShouldSteer())
        {
            Steer();
        }

        SpeedLimiter.LimitSpeed(_rigidBody);
    }

    private void OnHarmedBy(GameObject harmer)
    {
        if (harmer == null)
        {
            return;
        }

        var shotOwner = harmer.GetComponentInParent<ShotOwner>();
        if (shotOwner == null)
        {
            return;
        }

        var owner = shotOwner.GetOwner();
        if (IsGrudgeTarget(owner))
        {
            EnemyTarget.SetGrudge(transform, owner, Utils.GetEndOfLifeTime(_duration));
        }
    }

    private void Steer()
    {
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, EnemyTarget.GetDirection(transform).normalized, _steerForce, _steerSpeedLimit);
    }

    #region Predicates
    private bool ShouldSteer()
    {
        return _steerForce > 0 && EnemyTarget.HasGrudge(transform);
    }

    private bool IsGrudgeTarget(Transform owner)
    {
        return owner != null && owner != transform;
    }
    #endregion

#if UNITY_EDITOR
    internal float GetDuration()
    {
        return _duration;
    }
#endif
}
