using Assets.Enums;
using Assets.Utils;
using UnityEngine;

// Charge Lance (#236): released by the weapon, a straight line that hits every enemy and asteroid on it once, then disappears.
// Charge makes it longer and harder-hitting, in steps. A partly charged lance stops just short of the first asteroid or facing
// front plate and hurts neither; a full one goes through both and breaks the plate.
public class ChargeLance : ChargedAttack
{
    [SerializeField]
    private float _minLength = 4f;
    [SerializeField]
    private float _maxLength = 8f;
    [SerializeField]
    private float _minHarmMultiplier = 1f;
    [SerializeField]
    private float _maxHarmMultiplier = 3f;
    [SerializeField]
    private float _width = 0.4f;
    [SerializeField]
    private float _lifetime = 0.2f;

    private const float STOP_MARGIN = 0.2f;
    private const float CAST_THICKNESS = 0.01f;
    private const int HITS_BUFFER_SIZE = 32;
    private const int COLLIDER_OFF_AT_FIXED_UPDATE = 2;

    private AttackBehaviour _attackBehaviour;
    private BoxCollider2D _boxCollider;
    private LineRenderer _line;
    private RaycastHit2D[] _hits;
    private int _blockersLayerMask;
    private string _asteroidTag;
    private float _length;
    private float _endOfLifeTime;
    private int _fixedUpdates;

    // Awake, not Start: the weapon releases the lance in the tick it makes it, before its Start.
    private void Awake()
    {
        _attackBehaviour = GetComponent<AttackBehaviour>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _line = GetComponent<LineRenderer>();
        _hits = new RaycastHit2D[HITS_BUFFER_SIZE];
        _blockersLayerMask = LayerMask.GetMask(Layer.ENEMIES.GetDescription(), Layer.ASTEROIDS.GetDescription());
        _asteroidTag = Tag.ASTEROID.GetDescription();

        _boxCollider.enabled = false;
        _line.useWorldSpace = true;
        _line.positionCount = 2;
        _line.startWidth = _width;
        _line.endWidth = _width;
    }

    public override void Release(int chargeLevel)
    {
        float chargePortion = GetChargePortion(chargeLevel);
        bool isFullCharge = IsFullCharge(chargeLevel);

        _attackBehaviour.SetHarmMultiplier(Mathf.Lerp(_minHarmMultiplier, _maxHarmMultiplier, chargePortion));
        _attackBehaviour.SetArmorBreaking(isFullCharge);
        _length = Mathf.Lerp(_minLength, _maxLength, chargePortion);

        if (!isFullCharge)
        {
            _length = GetLengthBeforeBlocker(_length);
        }

        _boxCollider.size = new(_width, _length);
        _boxCollider.offset = new(0, _length / 2);
        _line.SetPosition(0, transform.position);
        _line.SetPosition(1, transform.position + transform.up * _length);
        _boxCollider.enabled = _length > 0;
        _endOfLifeTime = Utils.GetEndOfLifeTime(_lifetime);
    }

    // Counts calls, not time: in play the lance is released before its tick's physics step, in tests after it.
    // Switching the collider off on the second call gives both at least one step to hit what is on the line.
    private void FixedUpdate()
    {
        _fixedUpdates++;

        if (_fixedUpdates == COLLIDER_OFF_AT_FIXED_UPDATE)
        {
            _boxCollider.enabled = false;
        }

        if (Utils.ShouldDie(_endOfLifeTime))
        {
            Destroy(gameObject);
        }
    }

    // The distance to the nearest asteroid or facing front plate on the line, less a margin. The order of the hits does not matter.
    private float GetLengthBeforeBlocker(float length)
    {
        int hitsCount = Physics2D.BoxCastNonAlloc(transform.position, new Vector2(_width, CAST_THICKNESS), transform.eulerAngles.z, transform.up, _hits, length, _blockersLayerMask);
        float lengthBeforeBlocker = length;

        for (int i = 0; i < hitsCount; i++)
        {
            Collider2D hitCollider = _hits[i].collider;
            if (hitCollider.isTrigger || hitCollider.attachedRigidbody == null || !IsBlocker(hitCollider.attachedRigidbody))
            {
                continue;
            }

            lengthBeforeBlocker = Mathf.Min(lengthBeforeBlocker, Mathf.Max(0f, _hits[i].distance - STOP_MARGIN));
        }

        return lengthBeforeBlocker;
    }

    #region Predicates
    private bool IsBlocker(Rigidbody2D body)
    {
        return body.CompareTag(_asteroidTag) || IsBlockingArmor(body);
    }

    private bool IsBlockingArmor(Rigidbody2D body)
    {
        var frontArmor = body.GetComponent<FrontArmor>();
        return frontArmor != null && frontArmor.IsBlocking(gameObject);
    }
    #endregion

#if UNITY_EDITOR
    internal float GetLength()
    {
        return _length;
    }

    internal float GetLifetime()
    {
        return _lifetime;
    }
#endif
}
