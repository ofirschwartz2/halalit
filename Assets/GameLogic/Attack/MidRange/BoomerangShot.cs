using Assets.Enums;
using Assets.Utils;
using System.Xml.Xsl;
using UnityEngine;

// TODO (refactor): the stats (damage) of a shot (when it's collide with enemy) needs to be on the shot script
// TODO (refactor): move all shots out of the Gun. Enemies also shoot now.
public class BoomerangShot : MonoBehaviour 
{

    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private float _timeOfShot;
    [SerializeField]
    Vector2[] bezierCurve6InnerPoints;

    private Vector2 _startPosition, _rightDirection;
    private float _shootingPercentPassed, // Between 0 to 1
        _creationTime, halalitGraceTime = 0.1f;
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
        _shootingPercentPassed += Time.deltaTime / _timeOfShot;
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
        if (other.gameObject.CompareTag(Tag.EXTERNAL_WORLD.GetDescription())) 
        {
            Destroy(gameObject);
        }   
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (RealHalalitHit(other)
            ||
            other.gameObject.CompareTag(Tag.ENEMY.GetDescription()))
        {
            Destroy(gameObject);
        }
    }

    private bool RealHalalitHit(Collider2D other)
    {
        return 
            other.gameObject.CompareTag(Tag.HALALIT.GetDescription()) && 
            (Time.time > _creationTime + halalitGraceTime);
    }
    
}