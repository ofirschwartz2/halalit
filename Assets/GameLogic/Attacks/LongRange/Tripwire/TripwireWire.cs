using UnityEngine;

// Sits on the newer post's child. Draws the line and moves the trigger edge between the two posts.
public class TripwireWire : MonoBehaviour
{
    [SerializeField]
    private AttackBehaviour _postAttackBehaviour;
    [SerializeField]
    private AttackBehaviour _wireAttackBehaviour;
    [SerializeField]
    private EdgeCollider2D _edgeCollider;
    [SerializeField]
    private LineRenderer _line;
    [SerializeField]
    private float _maxLength = 9f;

    private const float MIN_POST_MOVEMENT = 0.01f;

    private Transform _olderPost;
    private Transform _newerPost;
    private Vector2 _lastOlderPostPosition;
    private Vector2 _lastNewerPostPosition;
    private Vector2[] _colliderPoints;
    private bool _isOn;

    #region Init
    // Awake, not Start: the wire can touch in its first physics step, and its harm comes from these stats.
    private void Awake()
    {
        _wireAttackBehaviour.Copy(_postAttackBehaviour);
        _colliderPoints = new Vector2[2];
        _line.useWorldSpace = true;
        _line.positionCount = 2;
        SetWireOn(false);
    }

    public void Connect(Transform olderPost, Transform newerPost)
    {
        _olderPost = olderPost;
        _newerPost = newerPost;
    }
    #endregion

    #region Logic
    private void FixedUpdate()
    {
        if (!ArePostsInReach())
        {
            SetWireOn(false);
            return;
        }

        if (!_isOn || DidAPostMove())
        {
            UpdateWirePoints();
        }

        SetWireOn(true);
    }

    private void UpdateWirePoints()
    {
        _lastOlderPostPosition = _olderPost.position;
        _lastNewerPostPosition = _newerPost.position;

        _line.SetPosition(0, _lastOlderPostPosition);
        _line.SetPosition(1, _lastNewerPostPosition);

        _colliderPoints[0] = (Vector2)transform.InverseTransformPoint(_lastOlderPostPosition);
        _colliderPoints[1] = (Vector2)transform.InverseTransformPoint(_lastNewerPostPosition);
        _edgeCollider.points = _colliderPoints;
    }

    private void SetWireOn(bool isOn)
    {
        _isOn = isOn;
        _edgeCollider.enabled = isOn;
        _line.enabled = isOn;
    }
    #endregion

    #region Predicates
    private bool ArePostsInReach()
    {
        return _olderPost != null && _newerPost != null && Vector2.Distance(_olderPost.position, _newerPost.position) <= _maxLength;
    }

    private bool DidAPostMove()
    {
        return Vector2.Distance(_olderPost.position, _lastOlderPostPosition) > MIN_POST_MOVEMENT
            || Vector2.Distance(_newerPost.position, _lastNewerPostPosition) > MIN_POST_MOVEMENT;
    }
    #endregion

#if UNITY_EDITOR
    internal float GetMaxLength()
    {
        return _maxLength;
    }

    internal bool IsWireOn()
    {
        return _isOn;
    }
#endif
}
