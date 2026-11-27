using Assets.Utils;
using System.Collections;
using UnityEngine;

public class SpawnHoleInstantiator : SeedfulRandomGeneratorUser
{
    // Endless Ramp (#248): checks often enough that a gap plays as written, not as the next whole second
    private const float SPAWN_CHECK_SECONDS = 0.1f;
    private const float FAILED_SEARCH_RETRY_SECONDS = 1f;

    [SerializeField]
    private float[] _instantiationRate;
    [SerializeField]
    private GameObject _spawnHolePrefab;
    [SerializeField]
    private GameObject _internalWorld;
    [SerializeField]
    private float _instantiationDistanceFree;
    [SerializeField]
    private int _findPositionRetries;

    private int _instantiationRatesIndex;
    private float _nextInstantiationTime;
    private float _minX, _maxX, _minY, _maxY;
    private Vector3 _instantiationPosition;
    private float _spawnIntervalMultiplier;

    void Start()
    {
        _instantiationRatesIndex = 0;
        _spawnIntervalMultiplier = DailyTwists.Current.GetSpawnInterval(1f);
        _nextInstantiationTime = GetNextInstantiationTime();
        SetInstantiationBounds();

        StartCoroutine(UpdateCoroutine());
    }
    private IEnumerator UpdateCoroutine()
    {
        while (true) 
        {
            if (Time.time >= _nextInstantiationTime)
            {
                InstantiateSpawnHole();
            }

            yield return new WaitForSeconds(SPAWN_CHECK_SECONDS);
        }
    }

    private void InstantiateSpawnHole()
    {
        if (FindObjectOfType<EnemyBank>().NoMoreEnemies())
        {
            Destroy(gameObject);
        }
        else
        {
            TryInstantiatingSpawnHole();
        }
    }

    // The next gap starts only once a hole is placed (#248). A failed search looks again 1 s later and keeps its row of the gap list.
    private void TryInstantiatingSpawnHole()
    {
        if (TryGetRandomInstantiationPosition())
        {
            Instantiate(_spawnHolePrefab, _instantiationPosition, Quaternion.identity);
            _nextInstantiationTime = GetNextInstantiationTime();
        }
        else
        {
            _nextInstantiationTime = Time.time + FAILED_SEARCH_RETRY_SECONDS;
        }
    }

    private float GetNextInstantiationTime()
    {

        var nextInstantiationTime = Time.time + GetInstantiationGap(_instantiationRatesIndex);

        if (_instantiationRatesIndex != _instantiationRate.Length - 1)
        {
            _instantiationRatesIndex ++;
        }

        return nextInstantiationTime;
    }

    // The gap before hole holeIndex: its row of the scene's gap list, holding the last row after the end, times the daily twist's multiplier
#if UNITY_EDITOR
internal
#else
private
#endif
    float GetInstantiationGap(int holeIndex)
    {
        return _instantiationRate[Mathf.Min(holeIndex, _instantiationRate.Length - 1)] * _spawnIntervalMultiplier;
    }

    private bool TryGetRandomInstantiationPosition()
    {
        for (int i = 0; i < _findPositionRetries; i++)
        {
            var potentialPosition = new Vector2(_seedfulRandomGenerator.Range(_minX, _maxX), _seedfulRandomGenerator.Range(_minY, _maxY));
            Collider2D[] colliders = Physics2D.OverlapCircleAll(potentialPosition, _instantiationDistanceFree);
            if (colliders.Length == 1)
            {
                _instantiationPosition = potentialPosition;
                return true;
            }
        }

        return false;
    }

    private void SetInstantiationBounds()
    {
        var internalWorldBounds = _internalWorld.GetComponent<Collider2D>().bounds;

        _minX = internalWorldBounds.min.x + _instantiationDistanceFree;
        _maxX = internalWorldBounds.max.x - _instantiationDistanceFree;
        _minY = internalWorldBounds.min.y + _instantiationDistanceFree;
        _maxY = internalWorldBounds.max.y - _instantiationDistanceFree;
    }

#if UNITY_EDITOR
    internal float GetSpawnIntervalMultiplier()
    {
        return _spawnIntervalMultiplier;
    }
#endif
}