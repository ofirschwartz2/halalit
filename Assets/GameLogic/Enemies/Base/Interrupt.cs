using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using UnityEngine;

// Interrupt (#238): a player shot or laser that hurts this enemy while it winds up an attack (IInterruptibleEnemyBehaviour)
// cancels that attack. The enemy flashes _flashColor for _flashDuration and stands still for _stunDuration. Its next wind-up
// is braced: tinted by _bracedTint, and it cannot be interrupted until one of its attacks finishes normally.
// Enemy shots, asteroids and crashes never interrupt.
public class Interrupt : MonoBehaviour
{
    [SerializeField]
    private float _windupTime = 1f;
    [SerializeField]
    private float _stunDuration = 0.5f;
    [SerializeField]
    private float _flashDuration = 0.3f;
    [SerializeField]
    private Color _flashColor = Color.white;
    [SerializeField]
    private Color _bracedTint = new(0.5f, 0.5f, 0.5f, 1f);

    private Rigidbody2D _rigidBody;
    private IHealth _health;
    private IInterruptibleEnemyBehaviour _interruptible;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private string _enemyShotTag;
    private bool _isBraced;
    private bool _isFlashing;
    private float _flashEndTime;

    #region Init
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _health = GetComponent<IHealth>();
        _interruptible = GetComponent<IInterruptibleEnemyBehaviour>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color;
        _enemyShotTag = Tag.ENEMY_SHOT.GetDescription();
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        _health.HarmedBy += OnHarmedBy;
        _interruptible.AttackFinished += OnAttackFinished;
    }
    #endregion

    #region Destroy
    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    private void DestroyEventListeners()
    {
        _health.HarmedBy -= OnHarmedBy;
        _interruptible.AttackFinished -= OnAttackFinished;
    }
    #endregion

    void FixedUpdate()
    {
        if (ShouldEndFlash())
        {
            _isFlashing = false;
            ApplyColor();
        }

        SpeedLimiter.LimitSpeed(_rigidBody);
    }

    private void OnHarmedBy(GameObject harmer)
    {
        if (ShouldInterrupt(harmer))
        {
            _interruptible.InterruptAttack(_stunDuration);
            _isBraced = true;
            StartFlash();
        }
    }

    private void OnAttackFinished()
    {
        _isBraced = false;
        ApplyColor();
    }

    private void StartFlash()
    {
        _isFlashing = true;
        _flashEndTime = Utils.GetEndOfLifeTime(_flashDuration);
        ApplyColor();
    }

    // The only place that writes the colour, and only when the flash or the brace changes. Braced keeps the enemy's own hue, darker.
    private void ApplyColor()
    {
        if (_isFlashing)
        {
            _spriteRenderer.color = _flashColor;
        }
        else if (_isBraced)
        {
            _spriteRenderer.color = _originalColor * _bracedTint;
        }
        else
        {
            _spriteRenderer.color = _originalColor;
        }
    }

    #region Predicates
    // The enabled check matters: SpawnHole switches this script off while the enemy spawns, and C# events still reach it.
    private bool ShouldInterrupt(GameObject harmer)
    {
        return enabled && !_isBraced && IsPlayerAttack(harmer) && _interruptible.IsInInterruptWindow(_windupTime);
    }

    // Enemy attacks carry a ShotOwner (player shots have none). The tag also catches an enemy shot that has no owner.
    private bool IsPlayerAttack(GameObject harmer)
    {
        return harmer != null && !harmer.CompareTag(_enemyShotTag) && harmer.GetComponentInParent<ShotOwner>() == null;
    }

    private bool ShouldEndFlash()
    {
        return _isFlashing && Utils.ShouldDie(_flashEndTime);
    }
    #endregion

#if UNITY_EDITOR
    internal bool IsBraced()
    {
        return _isBraced;
    }

    internal float GetStunDuration()
    {
        return _stunDuration;
    }

    internal float GetWindupTime()
    {
        return _windupTime;
    }
#endif
}
