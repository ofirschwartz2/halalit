using Assets.Utils;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    private float _beamingSpeed;

    private float _finalYScale;
    private float _finalYPosition;
    private float _endOfLifeTime;
    private bool _die;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _finalYScale = transform.localScale.y + _beamingSpeed * _lifetime;
        _finalYPosition = transform.localPosition.y + _beamingSpeed * _lifetime / 2;
        _endOfLifeTime = Time.time + _lifetime;
        
    }

    void Update()
    {
        BeamLaser();
        TryDie();
    }

    private void BeamLaser()
    {
        float newYScale = transform.localScale.y + _beamingSpeed * Time.deltaTime;
        float newYPosition = transform.localPosition.y + _beamingSpeed * Time.deltaTime / 2;

        transform.localScale = new Vector2(transform.localScale.x, newYScale > _finalYScale ? _finalYScale : newYScale);
        transform.localPosition = new Vector2(transform.localPosition.x, newYScale > _finalYScale ? _finalYPosition : newYPosition);

    }

    private void TryDie()
    {
        if (_die)
        {
            Destroy(gameObject);
            Destroy(transform.parent.gameObject);
        }
        else if (ShouldShotDieNextFrame())
        {
            _die = true;
        }
    }

    private bool ShouldShotDieNextFrame()
    {
        return Time.time >= _endOfLifeTime;
    }
}
