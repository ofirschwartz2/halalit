using Assets.Enums;
using Assets.Utils;
using System;
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
    private AnimationCurve _enemySizeCurve;
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
    private GameObject[] _enemies;
    private Vector2[] _spawnFinalPoints;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _startOfOpeningLifeTime = Time.time;
        _endOfOpeningLifeTime = _startOfOpeningLifeTime + _openingLifetime;
        _originalScale = transform.localScale;
        _enemies = new GameObject[_enemyPrefabs.Length];
        _spawnFinalPoints = new Vector2[_enemyPrefabs.Length];
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
            InstantiateEnemies();
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

        foreach(GameObject enemy in _enemies)
        {
            enemy.transform.localScale = GetNewLocalScale(
                _enemySizeCurve,
                _startOfOpenLifeTime,
                _openLifetime,
                1f
                );

            enemy.transform.position = Vector3.Lerp(
                transform.position,
                _spawnFinalPoints[Array.IndexOf(_enemies, enemy)],
                Utils.GetPortionPassed(_startOfOpenLifeTime, _openLifetime)
                );
        }

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

    private void InstantiateEnemies()
    {
        for (int i=0; i<_enemyPrefabs.Length; i++)
        {
            _enemies[i] = Instantiate(_enemyPrefabs[i], transform.position, Quaternion.identity);
            _enemies[i].transform.localScale = Vector3.zero;
        }

        _spawnFinalPoints = EnemyUtils.GetEvenPositionsAroundCircle(transform, _enemyPrefabs.Length, transform.localScale.magnitude / 4f).ToArray(); // TODO: fix
    }

    public Vector3 GetNewLocalScale(AnimationCurve animationCurve, float startOfLifeTime, float lifetime, float sizeMultiplier)
    {
        var blastMultiplier = animationCurve.Evaluate(Utils.GetPortionPassed(startOfLifeTime, lifetime)) * sizeMultiplier;
        return _originalScale * (blastMultiplier);
    }

    private void Die() 
    {
        Destroy(gameObject);
    }
}