using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class SpawnHole : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private GameObject[] _enemyPrefabs;
    [SerializeField]
    private AnimationCurve _spawningHoleOpeningCurve;
    [SerializeField]
    private AnimationCurve _spawningHoleOpenCurve;
    [SerializeField]
    private AnimationCurve _spawningHoleClosingCurve;
    [SerializeField]
    private float _openingLifetime;
    [SerializeField]
    private float _openLifetime;
    [SerializeField]
    private float _closingLifetime;
    [SerializeField]
    private float _spawnHoleMultiplier;

    private float 
        _startOfOpeningLifeTime, _endOfOpeningLifeTime,
        _startOfOpenLifeTime, _endOfOpenLifeTime,
        _startOfClosingLifeTime, _endOfClosingLifeTime;
    private Vector3 _originalScale;
    private SpawnHoleState _state;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _startOfOpeningLifeTime = Time.time;
        _endOfOpeningLifeTime = _startOfOpeningLifeTime + _openingLifetime;
        _originalScale = transform.localScale;
    }

    void FixedUpdate()
    {
        switch (_state)
        {
            case SpawnHoleState.OPENING:
                OPENING();
                break;
            case SpawnHoleState.OPEN:
                OPEN();
                break;
            case SpawnHoleState.CLOSING:
                CLOSING();
                break;
        }
    }

    private void OPENING()
    {
        transform.localScale = GetNewLocalScale(
            _spawningHoleOpeningCurve,
            _startOfOpeningLifeTime,
            _openingLifetime,
            _spawnHoleMultiplier
            );

        if (Time.time >= _endOfOpeningLifeTime)
        {
            _state = SpawnHoleState.OPEN;
            _startOfOpenLifeTime = Time.time;
            _endOfOpenLifeTime = _startOfOpenLifeTime + _openLifetime;
        }
    }

    private void OPEN()
    {
        transform.localScale = GetNewLocalScale(
            _spawningHoleOpenCurve,
            _startOfOpenLifeTime,
            _openLifetime,
            _spawnHoleMultiplier
            );

        if (Time.time >= _endOfOpenLifeTime)
        {
            _state = SpawnHoleState.CLOSING;
            _startOfClosingLifeTime = Time.time;
            _endOfClosingLifeTime = _startOfClosingLifeTime + _closingLifetime;
        }
    }

    private void CLOSING()
    {
        transform.localScale = GetNewLocalScale(
            _spawningHoleClosingCurve,
            _startOfClosingLifeTime,
            _closingLifetime,
            _spawnHoleMultiplier
            );

        if (Time.time >= _endOfClosingLifeTime)
        {
            Die();
        }
    }

    public Vector3 GetNewLocalScale(AnimationCurve animationCurve, float startOfLifeTime, float lifetime, float spawnHoleMultiplier)
    {
        var blastMultiplier = animationCurve.Evaluate(Utils.GetPortionPassed(startOfLifeTime, lifetime)) * spawnHoleMultiplier;
        return _originalScale * (blastMultiplier);
    }

    private void Die() 
    {
        Destroy(gameObject);
    }
}