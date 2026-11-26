using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using UnityEngine;

// Mimic: a wait step that looks like a weapon item. Hidden, it only spins, and the pickup claw can grab and carry it.
// It wakes when the Halalit comes within _wakeRadius (also while carried), when it is hit, or when a claw lets go of it:
// it drops its item disguise, shows its health bar and, after _wakeDelay, the state machine starts its first charge.
// Awake, it waits exactly like _awakeWait (the prefab's WaitAttackIntervalWait).
public class WaitAttackDisguisedWait : WaitAttackWait, IGrabbable
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private WaitAttackIntervalWait _awakeWait;
    [SerializeField]
    private GameObject _disguise;
    [SerializeField]
    private float _wakeRadius = 1.5f;
    [SerializeField]
    private float _wakeDelay = 0.3f;
    [SerializeField]
    private float _hiddenSpinSpeed = 30f;

    private IHealth _health;
    private SpriteRenderer _spriteRenderer;
    private Transform _halalitTransform;
    private string _sortingLayerBeforeGrab;
    private bool _isHidden;
    private bool _isCarried;
    private float _endOfWakeDelayTime;

    #region Init
    void Awake()
    {
        _health = GetComponent<IHealth>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isHidden = true;
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

    public override void Wait()
    {
        if (!_isHidden)
        {
            _awakeWait.Wait();
        }
        else if (ShouldWake())
        {
            Wake();
        }
        else if (!_isCarried)
        {
            Spin();
        }
    }

    // Until the first charge, _awakeWait was never set waiting, so it is ready at once: the first charge waits only for _wakeDelay.
    public override bool ShouldStopWaiting()
    {
        return !_isHidden && DidWakeDelayPass() && _awakeWait.ShouldStopWaiting();
    }

    public override void SetWaiting()
    {
        if (!_isHidden)
        {
            _awakeWait.SetWaiting();
        }
    }

    private void Spin()
    {
        _rigidBody.MoveRotation(_rigidBody.rotation + _hiddenSpinSpeed * Time.deltaTime);
    }

    private void Wake()
    {
        _isHidden = false;
        Release();
        _disguise.SetActive(false);
        _health.ShowHealthBar();
        _endOfWakeDelayTime = Time.time + _wakeDelay;
    }

    // Wakes at once, inside the hit, so that a hit that also starts WoundedRetreat never leaves it disguised or carried.
    private void OnWounded(int healthLeft)
    {
        if (_isHidden)
        {
            Wake();
        }
    }

    #region Grabbing
    // The ItemMovement shape. Only a hidden Mimic can be carried.
    public void Grabbed(Transform grabber)
    {
        if (!_isHidden || _isCarried)
        {
            return;
        }

        _isCarried = true;
        _rigidBody.isKinematic = true;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.angularVelocity = 0f;
        transform.position = grabber.position;
        transform.SetParent(grabber);
        _sortingLayerBeforeGrab = _spriteRenderer.sortingLayerName;
        _spriteRenderer.sortingLayerName = Assets.Enums.SortingLayer.FRONT.GetDescription();
    }

    // A claw that lets go of a hidden Mimic wakes it.
    public void UnGrabbed()
    {
        if (_isHidden)
        {
            Wake();
        }
        else
        {
            Release();
        }
    }

    private void Release()
    {
        if (!_isCarried)
        {
            return;
        }

        _isCarried = false;
        _rigidBody.isKinematic = false;
        transform.parent = null;
        _spriteRenderer.sortingLayerName = _sortingLayerBeforeGrab;
    }
    #endregion

    #region Predicates
    private bool ShouldWake()
    {
        return IsHalalitInWakeRadius();
    }

    private bool IsHalalitInWakeRadius()
    {
        return Vector2.Distance(transform.position, _halalitTransform.position) <= _wakeRadius;
    }

    private bool DidWakeDelayPass()
    {
        return Time.time > _endOfWakeDelayTime;
    }
    #endregion

#if UNITY_EDITOR
    internal float GetWakeRadius()
    {
        return _wakeRadius;
    }

    internal float GetWakeDelay()
    {
        return _wakeDelay;
    }

    internal bool IsHidden()
    {
        return _isHidden;
    }
#endif
}
