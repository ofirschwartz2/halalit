using Assets.Enums;
using Assets.Utils;
using UnityEngine;

// Tether Twins (#237): the line between two twins, one per pair. It harms for _onDuration, then is off for _offDuration, blinking
// without harm for the last _flickerDuration of it. It is also off while a twin is paused or spawning, while the twins are more than
// _maxDistance apart, or while an asteroid is between them. It is drawn centre to centre but harms only between the two insets
// (body radius plus _bodyMargin), because the twins are hurt by enemy shots too. When one twin dies, the survivor swings a stub of
// _flailLength that turns _flailTurnsPerSecond and is always on. When both twins are gone, the tether removes itself.
public class Tether : MonoBehaviour
{
    private const float FULL_TURN_DEGREES = 360f;

    [SerializeField]
    private float _onDuration = 3f;
    [SerializeField]
    private float _offDuration = 1.5f;
    [SerializeField]
    private float _flickerDuration = 0.5f;
    [SerializeField]
    private float _flickerInterval = 0.1f;
    [SerializeField]
    private float _maxDistance = 10f;
    [SerializeField]
    private float _flailLength = 3f;
    [SerializeField]
    private float _flailTurnsPerSecond = 1f;
    [SerializeField]
    private float _bodyMargin = 0.1f;

    private EdgeCollider2D _edgeCollider;
    private LineRenderer _line;
    private Vector2[] _colliderPoints;
    private int _asteroidsLayerMask;
    private TetherTwin _first;
    private TetherTwin _second;
    private TetherTwin _survivor;
    private TetherState _state;
    private float _stateEndTime;
    private Vector2 _lineDirection;
    private Vector2 _flailDirection;

    #region Init
    void Awake()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
        _line = GetComponent<LineRenderer>();
        _line.useWorldSpace = true;
        _line.positionCount = 2;
        _colliderPoints = new Vector2[2];
        _asteroidsLayerMask = LayerMask.GetMask(Layer.ASTEROIDS.GetDescription());
        _lineDirection = Vector2.right;
        SwitchOff();
    }

    // The first twin owns the line (Grudge). The cycle starts in OFF, so the first flicker comes after the off time.
    public void Connect(TetherTwin first, TetherTwin second)
    {
        _first = first;
        _second = second;
        ShotOwner.Mark(gameObject, first.transform);
        StartState(TetherState.OFF, _offDuration - _flickerDuration);
    }
    #endregion

    void FixedUpdate()
    {
        if (AreBothTwinsGone())
        {
            Destroy(gameObject);
            return;
        }

        if (AreBothTwinsAlive())
        {
            UpdateLineDirection();
        }
        else if (_state != TetherState.FLAIL)
        {
            StartFlail();
        }

        if (ShouldEndCycleState())
        {
            StartNextCycleState();
        }

        switch (_state)
        {
            case TetherState.OFF:
                SwitchOff();
                break;
            case TetherState.FLICKERING:
                ShowTether(IsFlickerShown(), false);
                break;
            case TetherState.ON:
                ShowTether(true, true);
                break;
            case TetherState.FLAIL:
                Flail();
                break;
        }
    }

    // Hides the line and stops its harm at once. A twin calls it when it is switched off.
    public void SwitchOff()
    {
        SetLine(false, false);
    }

    #region Cycle
    // OFF, FLICKERING and ON follow each other on timers. The timers run on while the line is blocked.
    private void StartNextCycleState()
    {
        switch (_state)
        {
            case TetherState.OFF:
                StartState(TetherState.FLICKERING, _flickerDuration);
                break;
            case TetherState.FLICKERING:
                StartState(TetherState.ON, _onDuration);
                break;
            case TetherState.ON:
                StartState(TetherState.OFF, _offDuration - _flickerDuration);
                break;
            case TetherState.FLAIL:
                break;
        }
    }

    private void StartState(TetherState state, float duration)
    {
        _state = state;
        _stateEndTime = Utils.GetEndOfLifeTime(duration);
    }

    // Drawn centre to centre; the harm segment stops one inset short of each twin's centre. A blocked line is hidden and harmless.
    private void ShowTether(bool isVisible, bool isHarmful)
    {
        if (IsPaused(_first) || IsPaused(_second))
        {
            SwitchOff();
            return;
        }

        Vector2 firstPosition = _first.transform.position;
        Vector2 secondPosition = _second.transform.position;
        var distance = Vector2.Distance(firstPosition, secondPosition);
        var firstInset = GetInset(_first);
        var secondInset = GetInset(_second);

        if (IsTooFar(distance) || IsHarmSegmentEmpty(distance, firstInset, secondInset) || IsCutByAsteroid(firstPosition, secondPosition))
        {
            SwitchOff();
            return;
        }

        var direction = (secondPosition - firstPosition) / distance;
        UpdatePoints(firstPosition, secondPosition, firstPosition + direction * firstInset, secondPosition - direction * secondInset);
        SetLine(isVisible, isHarmful);
    }

    // Kept while both twins live: the flail starts along it
    private void UpdateLineDirection()
    {
        Vector2 firstToSecond = _second.transform.position - _first.transform.position;
        if (firstToSecond != Vector2.zero)
        {
            _lineDirection = firstToSecond.normalized;
        }
    }
    #endregion

    #region Flail
    // Starts along the last line direction, from the survivor's side. The survivor now owns the line.
    private void StartFlail()
    {
        if (_first != null)
        {
            _survivor = _first;
            _flailDirection = _lineDirection;
        }
        else
        {
            _survivor = _second;
            _flailDirection = -_lineDirection;
        }

        ShotOwner.Mark(gameObject, _survivor.transform);
        _state = TetherState.FLAIL;
    }

    // Turns every tick. Harms from the survivor's inset to the stub's end, always, except while the survivor is paused.
    private void Flail()
    {
        _flailDirection = Utils.AddAngleToVector(_flailDirection, FULL_TURN_DEGREES * _flailTurnsPerSecond * Time.fixedDeltaTime);

        if (IsPaused(_survivor))
        {
            SwitchOff();
            return;
        }

        Vector2 survivorPosition = _survivor.transform.position;
        var flailEnd = survivorPosition + _flailDirection * _flailLength;
        UpdatePoints(survivorPosition, flailEnd, survivorPosition + _flailDirection * GetInset(_survivor), flailEnd);
        SetLine(true, true);
    }
    #endregion

    #region Line
    // The two line positions and the two edge points (in local space), as in TripwireWire.UpdateWirePoints
    private void UpdatePoints(Vector2 lineStart, Vector2 lineEnd, Vector2 harmStart, Vector2 harmEnd)
    {
        _line.SetPosition(0, lineStart);
        _line.SetPosition(1, lineEnd);

        _colliderPoints[0] = (Vector2)transform.InverseTransformPoint(harmStart);
        _colliderPoints[1] = (Vector2)transform.InverseTransformPoint(harmEnd);
        _edgeCollider.points = _colliderPoints;
    }

    // Visible is the line renderer, harmful is the trigger edge
    private void SetLine(bool isVisible, bool isHarmful)
    {
        _line.enabled = isVisible;
        _edgeCollider.enabled = isHarmful;
    }

    private float GetInset(TetherTwin twin)
    {
        return twin.GetBodyRadius() + _bodyMargin;
    }
    #endregion

    #region Predicates
    private bool AreBothTwinsGone()
    {
        return _first == null && _second == null;
    }

    private bool AreBothTwinsAlive()
    {
        return _first != null && _second != null;
    }

    private bool ShouldEndCycleState()
    {
        return _state != TetherState.FLAIL && Utils.ShouldDie(_stateEndTime);
    }

    // A twin's TetherTwin is off while the twin spawns or retreats
    private bool IsPaused(TetherTwin twin)
    {
        return !twin.enabled;
    }

    private bool IsTooFar(float distance)
    {
        return distance > _maxDistance;
    }

    private bool IsHarmSegmentEmpty(float distance, float firstInset, float secondInset)
    {
        return distance <= firstInset + secondInset;
    }

    private bool IsCutByAsteroid(Vector2 firstPosition, Vector2 secondPosition)
    {
        return Physics2D.Linecast(firstPosition, secondPosition, _asteroidsLayerMask).collider != null;
    }

    // Shown for one _flickerInterval, hidden for the next, and so on, starting shown
    private bool IsFlickerShown()
    {
        var flickerTimePassed = Time.time - (_stateEndTime - _flickerDuration);
        return Mathf.FloorToInt(flickerTimePassed / _flickerInterval) % 2 == 0;
    }
    #endregion

#if UNITY_EDITOR
    internal TetherState GetState()
    {
        return _state;
    }

    internal bool IsHarmful()
    {
        return _edgeCollider.enabled;
    }

    internal bool IsVisible()
    {
        return _line.enabled;
    }

    // The ends of the trigger edge, in world space
    internal Vector2 GetHarmStart()
    {
        return transform.TransformPoint(_edgeCollider.points[0]);
    }

    internal Vector2 GetHarmEnd()
    {
        return transform.TransformPoint(_edgeCollider.points[1]);
    }

    internal float GetOnDuration()
    {
        return _onDuration;
    }

    internal float GetOffDuration()
    {
        return _offDuration;
    }

    internal float GetFlickerDuration()
    {
        return _flickerDuration;
    }

    internal float GetMaxDistance()
    {
        return _maxDistance;
    }

    internal float GetFlailLength()
    {
        return _flailLength;
    }

    internal float GetFlailTurnsPerSecond()
    {
        return _flailTurnsPerSecond;
    }
#endif
}
