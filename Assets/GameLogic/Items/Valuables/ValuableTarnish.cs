using UnityEngine;

// Tarnish (#231): a valuable lying free loses value over time. After _stepDuration seconds it turns into _nextStepPrefab
// (Diamond to Gold, Gold to Silver) at the same spot and speed; with no next step it vanishes. It blinks for the last
// _blinkDuration seconds. The clock counts physics steps only and pauses while the item is carried (ItemMovement.IsGrabbed).
public class ValuableTarnish : MonoBehaviour
{
    [SerializeField]
    private GameObject _nextStepPrefab;
    [SerializeField]
    private float _stepDuration = 8f;
    [SerializeField]
    private float _blinkDuration = 2f;
    [SerializeField]
    private float _blinkInterval = 0.25f;

    private Rigidbody2D _rigidBody;
    private SpriteRenderer _spriteRenderer;
    private ItemMovement _itemMovement;
    private float _elapsed;

    #region Init
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _itemMovement = GetComponent<ItemMovement>();
    }
    #endregion

    void FixedUpdate()
    {
        if (_itemMovement.IsGrabbed())
        {
            _spriteRenderer.enabled = true;
            return;
        }

        _elapsed += Time.deltaTime;
        if (_elapsed >= _stepDuration)
        {
            TarnishStep();
            return;
        }

        _spriteRenderer.enabled = !IsBlinkHidden();
    }

    private void TarnishStep()
    {
        if (_nextStepPrefab != null)
        {
            var nextStep = Instantiate(_nextStepPrefab, _rigidBody.position, transform.rotation);
            var nextStepRigidBody = nextStep.GetComponent<Rigidbody2D>();
            nextStepRigidBody.velocity = _rigidBody.velocity;
            nextStepRigidBody.angularVelocity = _rigidBody.angularVelocity;
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    #region Predicates
    private bool IsBlinkHidden()
    {
        var remaining = _stepDuration - _elapsed;
        return remaining <= _blinkDuration && Mathf.FloorToInt(remaining / _blinkInterval) % 2 == 1;
    }
    #endregion

#if UNITY_EDITOR
    internal GameObject GetNextStepPrefab()
    {
        return _nextStepPrefab;
    }

    internal float GetStepDuration()
    {
        return _stepDuration;
    }

    internal float GetBlinkDuration()
    {
        return _blinkDuration;
    }
#endif
}
