using Assets.Utils;
using UnityEngine;

// TODO (refactor): the stats (damage) of a shot (when it's collide with enemy) needs to be on the shot script
// TODO (refactor): move all shots out of the Gun. Enemies also shoot now.
public class Blast : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _blastMultiplier;
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    private AnimationCurve blastCurve;
    [SerializeField]
    private GameObject _explosionPrefab, _shockWavePrefab;

    private GameObject _explosion, _shockWave;
    private float _startOfLifeTime, _endOfLifeTime;
    private Vector3 _currentLocalScale, _newLocalScale;
    private Vector3 _originalScale;
    private bool _explosionAlive, _shockWaveAlive;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _originalScale = transform.localScale;
        SetLifeTimes();
        
        InstantiateBlastPrefabs();
    }

    private void Update()
    {
        _currentLocalScale = _explosion.transform.localScale;
        _newLocalScale = GetNewLocalScale();

        if (_explosionAlive) 
        {
            BlastExplosionUpdate(_endOfLifeTime, _newLocalScale);
        }

        if (_shockWaveAlive)
        {
            BlastShockWaveUpdate(_newLocalScale);
        }

        if (!_explosionAlive && !_shockWaveAlive) 
        {
            Die();
        }
    }

    private void InstantiateBlastPrefabs()
    {
        _explosion = Instantiate(_explosionPrefab, transform.position, transform.rotation);
        _explosionAlive = true;

        _shockWave = Instantiate(_shockWavePrefab, transform.position, transform.rotation);
        _shockWaveAlive = true;
    }

    private void BlastExplosionUpdate(float endOfLifeTime, Vector3 newLocalScale)
    {
        if (Utils.ShouldDie(endOfLifeTime))
        {
            DestroyBlastExplosion();
        }
        else
        {
            _explosion.transform.localScale = newLocalScale;
        }
    }

    private void BlastShockWaveUpdate(Vector3 newLocalScale)
    {
        if (IsContracting(newLocalScale.magnitude, _currentLocalScale.magnitude))
        {
            DestroyBlastShockWave();
        }
        else
        {
            _shockWave.transform.localScale = newLocalScale;
        }
    }

    public Vector3 GetNewLocalScale()
    {
        var blastMultiplier = blastCurve.Evaluate(Utils.GetPortionPassed(_startOfLifeTime, _lifetime)) * _blastMultiplier;
        return _originalScale * (1 + blastMultiplier);
    }

    private void SetLifeTimes()
    {
        _startOfLifeTime = Time.time;
        _endOfLifeTime = Utils.GetEndOfLifeTime(_lifetime);
    }

    private bool IsContracting(float newLocalScale, float currentLocalScale)
    {
        return newLocalScale < currentLocalScale;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void DestroyBlastExplosion()
    {
        Destroy(_explosion);
        _explosionAlive = false;
    }

    private void DestroyBlastShockWave()
    {
        Destroy(_shockWave);
        _shockWaveAlive = false;
    }
}