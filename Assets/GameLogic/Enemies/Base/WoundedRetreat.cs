using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Wounded Retreat: when a hit leaves the enemy at _woundedHealth or less, it pauses its driving scripts (IPausableEnemyBehaviour)
// and flees straight away from the Halalit. After _retreatDuration it heals to _healTo and its scripts come back.
// If it is stuck against an edge for _corneredTime, the retreat ends at once, without healing. It happens once per enemy.
public class WoundedRetreat : MonoBehaviour
{
    [SerializeField]
    private int _woundedHealth = 2;
    [SerializeField]
    private float _retreatDuration = 4f;
    [SerializeField]
    private float _fleeForce = 3f;
    [SerializeField]
    private float _fleeSpeedLimit = 3f;
    [SerializeField]
    private int _healTo = 4;
    [SerializeField]
    private float _corneredTime = 0.5f;

    private Rigidbody2D _rigidBody;
    private IHealth _health;
    private Behaviour[] _pausables;
    private readonly List<Behaviour> _pausedBehaviours = new();
    private Transform _halalitTransform;
    private WoundedRetreatState _state;
    private float _endOfRetreatTime;
    private int _edgeContacts;
    private float _firstEdgeContactTime;

    #region Init
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _health = GetComponent<IHealth>();
        _pausables = GetComponents<IPausableEnemyBehaviour>().Select(pausable => (Behaviour)pausable).ToArray();
        _state = WoundedRetreatState.READY;
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        _health.Wounded += OnWounded;
    }

    void Start()
    {
        _halalitTransform = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription()).transform;
    }
    #endregion

    #region Destroy
    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    private void DestroyEventListeners()
    {
        _health.Wounded -= OnWounded;
    }
    #endregion

    void FixedUpdate()
    {
        switch (_state)
        {
            case WoundedRetreatState.READY:
            case WoundedRetreatState.DONE:
                break;
            case WoundedRetreatState.RETREATING:
                Retreating();
                break;
        }

        SpeedLimiter.LimitSpeed(_rigidBody);
    }

    private void OnWounded(int healthLeft)
    {
        if (ShouldStartRetreat(healthLeft))
        {
            StartRetreat();
        }
    }

    private void StartRetreat()
    {
        _pausedBehaviours.Clear();
        foreach (Behaviour pausable in _pausables)
        {
            if (pausable.enabled)
            {
                _pausedBehaviours.Add(pausable);
                pausable.enabled = false;
            }
        }

        _endOfRetreatTime = Utils.GetEndOfLifeTime(_retreatDuration);
        _state = WoundedRetreatState.RETREATING;
    }

    private void Retreating()
    {
        KeepPaused();

        if (IsCornered())
        {
            EndRetreat();
        }
        else if (Utils.ShouldDie(_endOfRetreatTime))
        {
            _health.TryChangeHealth(Mathf.Max(0, _healTo - _health.GetCurrentHealth()));
            EndRetreat();
        }
        else
        {
            Flee();
        }
    }

    // SpawnHole.EnableEnemyScripts switches every script on; a paused one is switched off again.
    private void KeepPaused()
    {
        foreach (Behaviour pausedBehaviour in _pausedBehaviours)
        {
            if (pausedBehaviour.enabled)
            {
                pausedBehaviour.enabled = false;
            }
        }
    }

    private void Flee()
    {
        Vector2 awayFromHalalit = transform.position - _halalitTransform.position;
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, awayFromHalalit.normalized, _fleeForce, _fleeSpeedLimit);
    }

    // Switches back on only the scripts that were on when the retreat started (a test may keep a driver off).
    private void EndRetreat()
    {
        _state = WoundedRetreatState.DONE;
        foreach (Behaviour pausedBehaviour in _pausedBehaviours)
        {
            pausedBehaviour.enabled = true;
        }
        _pausedBehaviours.Clear();
    }

    #region Predicates
    private bool ShouldStartRetreat(int healthLeft)
    {
        return enabled && _state == WoundedRetreatState.READY && healthLeft <= _woundedHealth;
    }

    private bool IsCornered()
    {
        return _edgeContacts > 0 && Time.time - _firstEdgeContactTime >= _corneredTime;
    }
    #endregion

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.DidHitEdge(other.gameObject.tag))
        {
            if (_edgeContacts == 0)
            {
                _firstEdgeContactTime = Time.time;
            }
            _edgeContacts++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (Utils.DidHitEdge(other.gameObject.tag) && _edgeContacts > 0)
        {
            _edgeContacts--;
        }
    }
    #endregion

#if UNITY_EDITOR
    internal int GetWoundedHealth()
    {
        return _woundedHealth;
    }

    internal bool IsRetreating()
    {
        return _state == WoundedRetreatState.RETREATING;
    }

    internal float GetRetreatDuration()
    {
        return _retreatDuration;
    }

    internal int GetHealTo()
    {
        return _healTo;
    }

    internal float GetCorneredTime()
    {
        return _corneredTime;
    }
#endif
}