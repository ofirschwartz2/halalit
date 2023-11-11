using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnHole : MonoBehaviour
{
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
    [SerializeField]
    private List<string> _alwaysEnabledScripts;

    private float 
        _startOfOpeningLifeTime, _endOfOpeningLifeTime,
        _startOfOpenLifeTime, _endOfOpenLifeTime,
        _startOfClosingLifeTime, _endOfClosingLifeTime;
    private Vector3 _originalScale;
    private SpawnHoleState _state;
    private List<GameObject> _enemyPrefabs;
    private List<GameObject> _enemies;
    private List<Vector2> _enemiesSpawnFinalPoints;

    void Start()
    {
        SetOpeningTimes();
        SetLists();

        _originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        _state = SpawnHoleState.OPENING;
    }

    void FixedUpdate()
    {
        switch (_state)
        {
            case SpawnHoleState.OPENING:
                Opening();
                break;
            case SpawnHoleState.OPEN:
                Open();
                break;
            case SpawnHoleState.CLOSING:
                Close();
                break;
        }
    }

    #region OPENING
    private void Opening()
    {
        transform.localScale = GetNewLocalScale(
            _spawningHoleOpeningCurve,
            _startOfOpeningLifeTime,
            _openingLifetime,
            _spawnHoleMultiplier
            );

        if (Time.time >= _endOfOpeningLifeTime)
        {
            EndOpening();
        }
    }

    private void EndOpening()
    {
        _state = SpawnHoleState.OPEN;
        SetOpenTimes();
        InstantiateEnemies();
    }

    private void SetOpenTimes()
    {
        _startOfOpenLifeTime = Time.time;
        _endOfOpenLifeTime = _startOfOpenLifeTime + _openLifetime;
    }

    private void SetOpeningTimes()
    {
        _startOfOpeningLifeTime = Time.time;
        _endOfOpeningLifeTime = _startOfOpeningLifeTime + _openingLifetime;
    }
    #endregion

    #region OPEN
    private void Open()
    {
        transform.localScale = GetNewLocalScale(
            _spawningHoleOpenCurve,
            _startOfOpenLifeTime,
            _openLifetime,
            _spawnHoleMultiplier
            );

        SpawningEnemies();

        if (Time.time >= _endOfOpenLifeTime)
        {
            EndOpen();
        }
    }

    private void EndOpen()
    {
        _state = SpawnHoleState.CLOSING;
        SetClosingTimes();
        EnableEnemyScripts();
    }

    #endregion

    #region CLOSING
    private void Close()
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

    private void SetClosingTimes()
    {
        _startOfClosingLifeTime = Time.time;
        _endOfClosingLifeTime = _startOfClosingLifeTime + _closingLifetime;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
    #endregion

    #region enemies
    private void InstantiateEnemies()
    {
        if (_enemyPrefabs == null) 
        {
            throw new System.Exception("No enemies to spawn");
        }

        foreach (GameObject enemyPrefab in _enemyPrefabs)
        {
            var newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            newEnemy.transform.localScale = Vector3.zero;
            newEnemy.transform.SetParent(transform.parent);

            DisableEnemyScripts(newEnemy);
            
            _enemies.Add(newEnemy);
        }
        _enemiesSpawnFinalPoints = EnemyUtils.GetEvenPositionsAroundCircle(transform, _enemyPrefabs.Count, transform.localScale.magnitude);
    }

    private void SpawningEnemies()
    {
        foreach (GameObject enemy in _enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            enemy.transform.localScale = GetNewLocalScale(
                _enemySizeCurve,
                _startOfOpenLifeTime,
                _openLifetime
                );

            enemy.transform.position = Vector3.Lerp(
                transform.position,
                _enemiesSpawnFinalPoints[_enemies.IndexOf(enemy)],
                Utils.GetPortionPassed(_startOfOpenLifeTime, _openLifetime)
                );
        }
    }

    private void DisableEnemyScripts(GameObject newEnemy) 
    {
        Behaviour[] scripts = newEnemy.GetComponents<Behaviour>();
        foreach (Behaviour script in scripts)
        {
            if (!_alwaysEnabledScripts.Any(alwaysEnabledScript => script.GetType().Name.Contains(alwaysEnabledScript)))
            {
                script.enabled = false;
            }
        }
    }

    private void EnableEnemyScripts()
    {
        foreach (GameObject enemy in _enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            foreach (Behaviour script in enemy.GetComponents<Behaviour>())
            {
                script.enabled = true;
            }
        }
    }
    #endregion

    public Vector3 GetNewLocalScale(AnimationCurve animationCurve, float startOfLifeTime, float lifetime, float sizeMultiplier = 1f)
    {
        var blastMultiplier = animationCurve.Evaluate(Utils.GetPortionPassed(startOfLifeTime, lifetime)) * sizeMultiplier;
        return _originalScale * (blastMultiplier);
    }

    private void SetLists()
    {
        _enemyPrefabs = FindObjectOfType<EnemyBank>().GetNextSpawnHoleEnemiesList();
        _enemies = new List<GameObject>();
        _enemiesSpawnFinalPoints = new List<Vector2>();
    }

}