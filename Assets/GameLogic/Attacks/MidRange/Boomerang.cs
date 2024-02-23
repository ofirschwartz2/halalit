using Assets.Enums;
using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("Tests")]
#endif

public class Boomerang : MonoBehaviour 
{
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    Vector2[] bezierCurve6InnerPoints;

    private Vector2 _startPosition, _rightDirection;
    private float _shootingPercentPassed; // Between 0 to 1
    private float _creationTime;
    private GameObject _halalit;

    void Start()
    {
        _rightDirection = transform.right;
        _startPosition = transform.position;
        _halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        _creationTime = Time.time;
    }

    void FixedUpdate()
    {
        _shootingPercentPassed += Time.deltaTime / _lifetime;
        Shooting();
    }

    private void Shooting()
    {
        transform.Rotate(0, 0, _rotationSpeed);

        transform.position = Utils.GetPointOnBezierCurveOf8(
            _startPosition,
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[0], _rightDirection),
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[1], _rightDirection),
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[2], _rightDirection),
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[3], _rightDirection),
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[4], _rightDirection),
            _startPosition + Utils.RotateVector2WithVector2(bezierCurve6InnerPoints[5], _rightDirection),
            _halalit.transform.position, 
            _shootingPercentPassed);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription())) 
        {
            Destroy(gameObject);
        }   
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (HalalitHitOnReturn(other) || other.gameObject.CompareTag(Tag.ENEMY.GetDescription()) || other.gameObject.CompareTag(Tag.ASTEROID.GetDescription()))
        {
            Destroy(gameObject);
        }
    }

    private bool HalalitHitOnReturn(Collider2D other)
    {
        return other.gameObject.CompareTag(Tag.HALALIT.GetDescription()) && (Time.time > _creationTime + Constants.BOOMERANG_OUT_OF_HALALIT_TIME);
    }

    #if UNITY_EDITOR
    internal float GetLifetime()
    {
        return _lifetime;
    }
    #endif
}