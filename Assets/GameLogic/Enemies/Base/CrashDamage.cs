using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using UnityEngine;

// Crash Damage (#232): an enemy that slams into an asteroid, a world edge or another enemy hard enough is hurt by its own speed.
// The speed is the one going into the physics step, cached by Knockbackee. Against another enemy it is the speed of the two
// relative to each other, so both enemies in a crash take the same harm. The ship is not a crash: it takes the enemy's touch damage.
public class CrashDamage : MonoBehaviour
{
    [SerializeField]
    private float _crashSpeed = 6f;
    [SerializeField]
    private int _baseHarm = 2;
    [SerializeField]
    private float _speedPerExtraHarm = 2f;
    [SerializeField]
    private int _maxHarm = 5;
    [SerializeField]
    private float _cooldown = 0.5f;

    private IHealth _health;
    private Knockbackee _knockbackee;
    private string _enemyTag;
    private string _asteroidTag;
    private float _nextCrashTime;

    #region Init
    void Awake()
    {
        _health = GetComponent<IHealth>();
        _knockbackee = GetComponent<Knockbackee>();
        _enemyTag = Tag.ENEMY.GetDescription();
        _asteroidTag = Tag.ASTEROID.GetDescription();
    }
    #endregion

    #region Events
    // Unity sends collision messages to disabled scripts too; a disabled CrashDamage (as in a spawn hole) does nothing.
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!enabled)
        {
            return;
        }

        if (other.gameObject.CompareTag(_enemyTag))
        {
            TryCrash((_knockbackee.GetVelocityBeforeKnockback() - GetOtherEnemyVelocity(other)).magnitude);
        }
        else if (Utils.DidHitEdge(other.gameObject.tag) || other.gameObject.CompareTag(_asteroidTag))
        {
            TryCrash(_knockbackee.GetSpeedBeforeKnockback());
        }
    }
    #endregion

#if UNITY_EDITOR
internal
#else
private
#endif
    void TryCrash(float speed)
    {
        if (IsCrashSpeed(speed) && Utils.ShouldDie(_nextCrashTime))
        {
            _health.TryChangeHealth(-GetCrashHarm(speed));
            _nextCrashTime = Utils.GetEndOfLifeTime(_cooldown);
        }
    }

    private Vector2 GetOtherEnemyVelocity(Collision2D other)
    {
        var otherKnockbackee = other.gameObject.GetComponent<Knockbackee>();
        return otherKnockbackee != null ? otherKnockbackee.GetVelocityBeforeKnockback() : other.rigidbody.velocity;
    }

    private int GetCrashHarm(float speed)
    {
        return Mathf.Min(_maxHarm, _baseHarm + Mathf.FloorToInt((speed - _crashSpeed) / _speedPerExtraHarm));
    }

    #region Predicates
    private bool IsCrashSpeed(float speed)
    {
        return speed >= _crashSpeed;
    }
    #endregion
}
