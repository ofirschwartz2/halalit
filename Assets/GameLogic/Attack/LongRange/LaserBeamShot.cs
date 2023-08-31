using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class LaserBeamShot : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    private float _beamingSpeed;
    [SerializeField]
    private float _maxbeamSize;

    private float _endOfLifeTime;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _endOfLifeTime = Time.time + _lifetime;
    }

    void FixedUpdate()
    {
        TryDie();
        BeamLaser();
    }

    private void BeamLaser()
    {
        if (transform.localScale.y < _maxbeamSize) 
        {
            float newYScale = transform.localScale.y + _beamingSpeed;
            float newYPosition = transform.localPosition.y + _beamingSpeed / 2;

            transform.localScale = new Vector2(transform.localScale.x, newYScale);
            transform.localPosition = new Vector2(transform.localPosition.x, newYPosition);
        }
    }

    private void TryDie()
    {
        if (ShouldDie())
        {
            Destroy(gameObject);
            Destroy(transform.parent.gameObject);
        }
    }

    private bool ShouldDie()
    {
        return Time.time >= _endOfLifeTime;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        /*
        if (other.gameObject.CompareTag(Tag.EXTERNAL_WORLD.GetDescription()))
            Destroy(gameObject);
        */
    }
}
