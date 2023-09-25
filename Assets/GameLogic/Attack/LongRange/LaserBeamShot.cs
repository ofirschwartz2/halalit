using UnityEngine;

public class LaserBeamShot : MonoBehaviour
{
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    private float _beamingSpeed;
    [SerializeField]
    private float _maxbeamSize;

    private float _endOfLifeTime;

    void Start()
    {
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
}
