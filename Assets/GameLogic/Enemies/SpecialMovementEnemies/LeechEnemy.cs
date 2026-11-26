using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using UnityEngine;

// Leech (#215): drifts toward the target and latches onto the Halalit when it touches it, with no touch damage (LatchCollisionHarmer).
// While latched it rides the ship (snapped to its spot on the hull each tick, its collisions with the ship ignored), harms it every
// _harmInterval and slows its thrust (ShipLatches). It falls off, stunned for _stunDuration, when the ship goes faster than
// _shakeOffSpeed, jumps more than _teleportDistance in one tick (Swap Shot), or touches an asteroid after flying at _scrapeSpeed or more.
public class LeechEnemy : MonoBehaviour, IPausableEnemyBehaviour, ILatcher
{
    private const int COLLIDERS_BUFFER_SIZE = 8;
    private const int CONTACTS_BUFFER_SIZE = 16;

    [SerializeField]
    private float _driftForce = 2f;
    [SerializeField]
    private float _driftSpeedLimit = 2f;
    [SerializeField]
    private int _latchedHarm = 1;
    [SerializeField]
    private float _harmInterval = 2f;
    [SerializeField]
    private float _thrustFactor = 0.7f;
    [SerializeField]
    private float _stunDuration = 1.5f;
    [SerializeField]
    private float _scrapeSpeed = 3f;
    [SerializeField]
    private float _shakeOffSpeed = 17f;
    [SerializeField]
    private float _teleportDistance = 1.5f;

    private Rigidbody2D _rigidBody;
    private readonly Collider2D[] _ownColliders = new Collider2D[COLLIDERS_BUFFER_SIZE];
    private int _ownCollidersCount;
    private readonly Collider2D[] _shipColliders = new Collider2D[COLLIDERS_BUFFER_SIZE];
    private int _shipCollidersCount;
    private readonly Collider2D[] _shipContacts = new Collider2D[CONTACTS_BUFFER_SIZE];
    private string _halalitTag;
    private string _asteroidTag;
    private LeechState _state;
    private Rigidbody2D _shipRigidbody;
    private IHealth _shipHealth;
    private Vector2 _latchOffset;
    private Vector2 _lastShipPosition;
    private float _lastShipSpeed;
    private float _nextHarmTime;
    private float _stunEnd;

    #region Init
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _ownCollidersCount = _rigidBody.GetAttachedColliders(_ownColliders);
        _halalitTag = Tag.HALALIT.GetDescription();
        _asteroidTag = Tag.ASTEROID.GetDescription();
        _state = LeechState.DRIFTING;
    }
    #endregion

    #region Destroy
    // Wounded Retreat pauses this script and death destroys it: either way the slot and the slow must not outlive the latch.
    private void OnDisable()
    {
        TryRelease();
    }

    private void OnDestroy()
    {
        TryRelease();
    }
    #endregion

    void FixedUpdate()
    {
        switch (_state)
        {
            case LeechState.DRIFTING:
                Drifting();
                break;
            case LeechState.LATCHED:
                Latched();
                break;
            case LeechState.STUNNED:
                Stunned();
                break;
        }

        SpeedLimiter.LimitSpeed(_rigidBody);
    }

    private void Drifting()
    {
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, EnemyTarget.GetDirection(transform).normalized, _driftForce, _driftSpeedLimit);
    }

    private void Stunned()
    {
        if (Utils.ShouldDie(_stunEnd))
        {
            _state = LeechState.DRIFTING;
        }
    }

    private void Latched()
    {
        if (ShouldFallOff())
        {
            Release();
            return;
        }

        if (Utils.ShouldDie(_nextHarmTime))
        {
            _shipHealth.TryChangeHealth(-_latchedHarm);
            _nextHarmTime = Utils.GetEndOfLifeTime(_harmInterval);
        }

        SnapToShip();
        _lastShipPosition = _shipRigidbody.position;
        _lastShipSpeed = _shipRigidbody.velocity.magnitude;
    }

    private void SnapToShip()
    {
        _rigidBody.position = _shipRigidbody.position + (Vector2)(Quaternion.Euler(0, 0, _shipRigidbody.rotation) * _latchOffset);
        _rigidBody.velocity = _shipRigidbody.velocity;
    }

    private void Latch(Collision2D collision)
    {
        if (_shipRigidbody == null)
        {
            _shipRigidbody = collision.rigidbody;
            _shipHealth = _shipRigidbody.GetComponent<IHealth>();
            _shipCollidersCount = _shipRigidbody.GetAttachedColliders(_shipColliders);
        }

        if (!ShipLatches.TryLatch(transform, _thrustFactor))
        {
            return;
        }

        _latchOffset = Quaternion.Euler(0, 0, -_shipRigidbody.rotation) * (_rigidBody.position - _shipRigidbody.position);
        _lastShipPosition = _shipRigidbody.position;
        _lastShipSpeed = _shipRigidbody.velocity.magnitude;
        _nextHarmTime = Utils.GetEndOfLifeTime(_harmInterval);
        SetIgnoreShipCollisions(true);
        _state = LeechState.LATCHED;
    }

    private void TryRelease()
    {
        if (_state == LeechState.LATCHED)
        {
            Release();
        }
    }

    private void Release()
    {
        ShipLatches.Release(transform);
        SetIgnoreShipCollisions(false);
        _stunEnd = Utils.GetEndOfLifeTime(_stunDuration);
        _state = LeechState.STUNNED;
    }

    private void SetIgnoreShipCollisions(bool ignore)
    {
        for (int i = 0; i < _ownCollidersCount; i++)
        {
            for (int j = 0; j < _shipCollidersCount; j++)
            {
                if (_ownColliders[i] != null && _shipColliders[j] != null)
                {
                    Physics2D.IgnoreCollision(_ownColliders[i], _shipColliders[j], ignore);
                }
            }
        }
    }

    public bool IsHarmlessTouch()
    {
        return CanLatch() || _state == LeechState.LATCHED;
    }

    #region Predicates
    private bool CanLatch()
    {
        return _state == LeechState.DRIFTING && ShipLatches.HasFreeSlot();
    }

    private bool ShouldFallOff()
    {
        return _shipRigidbody == null || IsShakenOff() || DidShipTeleport() || IsScrapedOff();
    }

    private bool IsShakenOff()
    {
        return _shipRigidbody.velocity.magnitude > _shakeOffSpeed;
    }

    private bool DidShipTeleport()
    {
        return Vector2.Distance(_lastShipPosition, _shipRigidbody.position) > _teleportDistance;
    }

    private bool IsScrapedOff()
    {
        if (_lastShipSpeed < _scrapeSpeed)
        {
            return false;
        }

        int contactsCount = _shipRigidbody.GetContacts(_shipContacts);
        for (int i = 0; i < contactsCount; i++)
        {
            if (_shipContacts[i].CompareTag(_asteroidTag))
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Collisions
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(_halalitTag) && CanLatch())
        {
            Latch(collision);
        }
    }
    #endregion

#if UNITY_EDITOR
    internal bool IsLatched()
    {
        return _state == LeechState.LATCHED;
    }

    internal bool IsStunned()
    {
        return _state == LeechState.STUNNED;
    }

    internal int GetLatchedHarm()
    {
        return _latchedHarm;
    }

    internal float GetHarmInterval()
    {
        return _harmInterval;
    }

    internal float GetStunDuration()
    {
        return _stunDuration;
    }

    internal float GetShakeOffSpeed()
    {
        return _shakeOffSpeed;
    }
#endif
}
