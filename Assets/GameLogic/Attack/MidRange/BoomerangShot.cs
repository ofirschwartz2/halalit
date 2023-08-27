using Assets.Enums;
using Assets.Utils;
using UnityEngine;

// TODO (refactor): the stats (damage) of a shot (when it's collide with enemy) needs to be on the shot script
// TODO (refactor): move all shots out of the Gun. Enemies also shoot now.
public class BoomerangShot : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed; 
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private float _widthOfShot;
    [SerializeField]
    private float _lengthOfShot;

    private Vector3 _startPosition;
    private Vector3 _firstSlurpPosition;
    private float _slurpOutOfShot;
    private float _slurpPassed;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _startPosition = transform.position;
        _slurpOutOfShot = 2f/3f;
        _firstSlurpPosition = _startPosition + -transform.right * _widthOfShot / 2 + transform.up * _slurpOutOfShot;
        _slurpPassed = 0;

    }

    void FixedUpdate()
    {
        Shooting();
        TryDie();
    }

    private void Shooting()
    {
        transform.Rotate(0, 0, _rotationSpeed);

        _slurpPassed += Time.deltaTime / 2;
        if (_slurpPassed > 1) 
        {
            Destroy(gameObject);
        }

        transform.position = GetPointOnBezierCurve(
            new Vector2(0,0), 
            new Vector2(-6, 8) / 4, 
            new Vector2(-7, 21) / 4, 
            new Vector2(-1, 27) / 4,
            new Vector2(1, 27) / 4, 
            new Vector2(7, 21) / 4,
            new Vector2(6, 8) / 4,
            new Vector2(0, 0), 
            _slurpPassed);
        
        /*
        if (!Returning()) 
        {
            
        }
        else
        {
        }
        */
    }

    public static Vector2 GetPointOnBezierCurve(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5, Vector2 p6, Vector2 p7, Vector2 p8, float progress)
    {
        // Use De Casteljau's algorithm to calculate a point on the Bezier curve

        // First, calculate the points on the first set of four cubic Bezier curves
        Vector2 q1 = Vector2.Lerp(p1, p2, progress);
        Vector2 q2 = Vector2.Lerp(p2, p3, progress);
        Vector2 q3 = Vector2.Lerp(p3, p4, progress);

        Vector2 q4 = Vector2.Lerp(p5, p6, progress);
        Vector2 q5 = Vector2.Lerp(p6, p7, progress);
        Vector2 q6 = Vector2.Lerp(p7, p8, progress);

        // Next, calculate the points on the second set of four cubic Bezier curves
        Vector2 r1 = Vector2.Lerp(q1, q2, progress);
        Vector2 r2 = Vector2.Lerp(q2, q3, progress);

        Vector2 r3 = Vector2.Lerp(q4, q5, progress);
        Vector2 r4 = Vector2.Lerp(q5, q6, progress);

        // Finally, calculate the point on the final cubic Bezier curve
        Vector2 finalPoint = Vector2.Lerp(r1, r2, progress);
        Vector2 finalPoint2 = Vector2.Lerp(r3, r4, progress);

        return Vector2.Lerp(finalPoint, finalPoint2, progress);
    }

    public Vector2 GetPointOnBezierCurve2(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5, Vector2 p6, Vector2 p7, Vector2 p8, float progress)
    {
        // Ensure progress is within the [0, 1] range
        progress = Mathf.Clamp01(progress);

        // Calculate blending functions
        float u = 1 - progress;
        float tt = progress * progress;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * progress;

        // Calculate the point on the curve using the interpolation formula
        Vector2 point = 
            uuu * p1 + 
            3 * uu * progress * p2 + 
            3 * u * tt * p3 + 
            ttt * p4 + 
            3 * uu * progress * p5 + 
            3 * u * tt * p6 + 
            ttt * p7 + 
            progress * progress * progress * p8;

        return point;
    }

    private void TryDie()
    {
        if (ShouldDie())
        {
            Destroy(gameObject);
        }
    }

    private bool ShouldDie()
    {
        return false;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.EXTERNAL_WORLD.GetDescription()))
            Destroy(gameObject);
    }
}