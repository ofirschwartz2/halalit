using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Coin Thief: hunts the nearest free valuable, carries it behind itself while it flees from the Halalit, and eats it after a delay.
// A hit from anything that hurts enemies makes it drop the valuable. The Halalit can take the valuable from its tail.
public class WaitAttackStealAttack : WaitAttackAttack
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _lootRadius = 8f;
    [SerializeField]
    private float _huntSpeed = 3f;
    [SerializeField]
    private float _fleeSpeed = 1.5f;
    [SerializeField]
    private float _movementAmplitude = 10f;
    [SerializeField]
    private float _grabDistance = 0.5f;
    [SerializeField]
    private float _carryDistance = 1.2f;
    [SerializeField]
    private float _eatDelay = 8f;
    [SerializeField]
    private float _rotationSpeed = 5f;
    [SerializeField]
    private List<Tag> _coinDroppers = new() { Tag.SHOT, Tag.KNOCKBACK_WAVE, Tag.ENEMY_SHOT, Tag.ASTEROID, Tag.EXPLOSION };

    private const int LOOT_HITS_BUFFER_SIZE = 16;

    private readonly Collider2D[] _lootHits = new Collider2D[LOOT_HITS_BUFFER_SIZE];
    private List<string> _coinDroppersDescriptions;
    private int _itemTriggersLayerMask;
    private Transform _halalitTransform;
    private GameObject _target;
    private IGrabbable _targetGrabbable;
    private float _eatTime;
    private bool _wasHit;

    #region Init
    void Awake()
    {
        _coinDroppersDescriptions = _coinDroppers.Select(tag => tag.GetDescription()).ToList();
        _itemTriggersLayerMask = LayerMask.GetMask(Layer.ITEM_TRIGGERS.GetDescription());
    }

    void Start()
    {
        _halalitTransform = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription()).transform;
    }
    #endregion

    public override void SetAttacking()
    {
        _wasHit = false;
        _target = GetNearestFreeValuable();
        _targetGrabbable = null;
    }

    public override void Attack()
    {
        if (IsCarrying())
        {
            Flee();
            TryEat();
        }
        else
        {
            Hunt();
        }
    }

    public override bool ShouldStopAttacking()
    {
        return _wasHit || _target == null || IsHeldBySomeoneElse();
    }

    private void Hunt()
    {
        Vector2 toTarget = _target.transform.position - transform.position;
        if (toTarget.magnitude <= _grabDistance)
        {
            TryGrab();
            return;
        }

        var direction = toTarget.normalized;
        RotateTowards(direction);
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, direction, _movementAmplitude, _huntSpeed);
    }

    private void TryGrab()
    {
        _targetGrabbable = _target.GetComponent<IGrabbable>();
        if (_targetGrabbable == null)
        {
            _target = null;
            return;
        }

        _targetGrabbable.Grabbed(transform);
        _target.transform.position = transform.position - transform.up * _carryDistance;
        _eatTime = Time.time + _eatDelay;
    }

    private void Flee()
    {
        Vector2 awayFromHalalit = transform.position - _halalitTransform.position;
        var direction = awayFromHalalit.normalized;
        RotateTowards(direction);
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, direction, _movementAmplitude, _fleeSpeed);
    }

    private void TryEat()
    {
        if (Time.time >= _eatTime)
        {
            Destroy(_target);
            _target = null;
        }
    }

    private void TryDrop()
    {
        if (IsCarrying())
        {
            _targetGrabbable.UnGrabbed();
        }
    }

    private void RotateTowards(Vector2 direction)
    {
        var targetRotation = Quaternion.LookRotation(Vector3.forward, direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private GameObject GetNearestFreeValuable()
    {
        return Utils.GetNearestFreeValuable(transform.position, _lootRadius, _lootHits, _itemTriggersLayerMask);
    }

    #region Predicates
    public bool IsLootInReach()
    {
        return GetNearestFreeValuable() != null;
    }

    private bool IsCarrying()
    {
        return _target != null && _target.transform.parent == transform;
    }

    private bool IsHeldBySomeoneElse()
    {
        var holder = _target.transform.parent;
        return holder != null && holder != transform;
    }

    private bool IsCoinDropper(GameObject other)
    {
        return _coinDroppersDescriptions.Contains(other.tag);
    }
    #endregion

    #region Events
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCoinDropper(other.gameObject))
        {
            TryDrop();
            _wasHit = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (IsCoinDropper(other.gameObject))
        {
            TryDrop();
            _wasHit = true;
        }
    }
    #endregion

#if UNITY_EDITOR
    internal float GetEatDelay()
    {
        return _eatDelay;
    }
#endif
}
