using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class Spikes : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _sideSpikesRotaion;
    [SerializeField]
    private float _extractionTime;
    [SerializeField]
    private AnimationCurve _extractionCurve;
    [SerializeField]
    private float _waitingTime;
    [SerializeField] 
    private float _retractionTime;
    [SerializeField]
    private AnimationCurve _retractionCurve;
    [SerializeField]
    private GameObject _centerSpikePrefab; 
    [SerializeField]
    private GameObject _sideSpikePrefab;

    private GameObject _centerSpike;
    private GameObject[] _sideSpikes;
    private SpikesState _state;
    private Vector2 _centerSpikeFinalPosition;
    private Vector2[] _sideSpikeFinalPosition;
    private float _extractionStartTime, _waitingStartTime, _retractionStartTime, _halfHalalit;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
        _sideSpikes = new GameObject[2];
        _sideSpikeFinalPosition = new Vector2[2];
        InstantiateSpikes();
        _state = SpikesState.EXTRACTING;
        _halfHalalit = 0.75f;
    }

    private void InstantiateSpikes()
    {
        var halalitTransform = Utils.GetHalalitTransform();
        var halalitPosition = Utils.GetHalalitPosition();

        _centerSpike = Instantiate(_centerSpikePrefab, halalitPosition, transform.rotation);

        _sideSpikes[0] = Instantiate(_sideSpikePrefab, halalitPosition, Utils.GetRotationPlusAngle(transform.rotation, -_sideSpikesRotaion));
        _sideSpikeFinalPosition[0] = GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(transform.rotation), 1);

        _sideSpikes[1] = Instantiate(_sideSpikePrefab, halalitPosition, Utils.GetRotationPlusAngle(transform.rotation, _sideSpikesRotaion));

        _extractionStartTime = Time.time;
    }

    void FixedUpdate()
    {
        switch (_state) 
        {
            case SpikesState.EXTRACTING:
                ExtractSpikes();
                if (IsExtractionFinished())
                {
                    _waitingStartTime = Time.time;
                    _state = SpikesState.OUT;
                }
                break;
            case SpikesState.OUT:
                WaitingSpikes();
                if (IsWaitingFinished()) 
                {
                    _retractionStartTime = Time.time;
                    _state = SpikesState.RETRACTING;
                }
                break;
            case SpikesState.RETRACTING:
                RetractingSpikes();
                if (IsRetractingFinished()) 
                {
                    Destroy(_centerSpike);
                    Destroy(_sideSpikes[0]);
                    Destroy(_sideSpikes[1]);
                    Destroy(gameObject);
                }
                break;
        }
    }

    private void ExtractSpikes()
    {
        var halalitPosition = Utils.GetHalalitPosition();
        var rotation = Utils.GetRotationAsVector2(_centerSpike.transform.rotation);
        _centerSpikeFinalPosition = GetDestinationPosition(halalitPosition, rotation, _halfHalalit);
        _sideSpikeFinalPosition[0] = GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, -_sideSpikesRotaion)), _halfHalalit);
        _sideSpikeFinalPosition[1] = GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, _sideSpikesRotaion)), _halfHalalit);

        _centerSpike.transform.position = Vector2.Lerp(
            halalitPosition, 
            _centerSpikeFinalPosition, 
            _extractionCurve.Evaluate(Utils.GetPortionPassed(_extractionStartTime, _extractionTime)));

        _sideSpikes[0].transform.position = Vector2.Lerp(
            halalitPosition,
            _sideSpikeFinalPosition[0], 
            _extractionCurve.Evaluate(Utils.GetPortionPassed(_extractionStartTime, _extractionTime)));

        _sideSpikes[1].transform.position = Vector2.Lerp(
            halalitPosition,
            _sideSpikeFinalPosition[1], 
            _extractionCurve.Evaluate(Utils.GetPortionPassed(_extractionStartTime, _extractionTime)));

    }

    private void WaitingSpikes()
    {
        
        var halalitPosition = Utils.GetHalalitPosition();
        var rotation = Utils.GetRotationAsVector2(_centerSpike.transform.rotation);
        _centerSpike.transform.position = GetDestinationPosition(halalitPosition, rotation, _halfHalalit);
        _sideSpikes[0].transform.position = GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, -_sideSpikesRotaion)), _halfHalalit);
        _sideSpikes[1].transform.position = GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, _sideSpikesRotaion)), _halfHalalit);
        
    }

    private void RetractingSpikes()
    {
        var halalitPosition = Utils.GetHalalitPosition();
        var rotation = Utils.GetRotationAsVector2(_centerSpike.transform.rotation);
        _centerSpikeFinalPosition = GetDestinationPosition(halalitPosition, rotation, _halfHalalit);
        _sideSpikeFinalPosition[0] = GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, -_sideSpikesRotaion)), _halfHalalit);
        _sideSpikeFinalPosition[1] = GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, _sideSpikesRotaion)), _halfHalalit);

        _centerSpike.transform.position = Vector2.Lerp(
            _centerSpikeFinalPosition,
            halalitPosition,
            _retractionCurve.Evaluate(Utils.GetPortionPassed(_retractionStartTime, _retractionTime)));

        _sideSpikes[0].transform.position = Vector2.Lerp(
            _sideSpikeFinalPosition[0],
            halalitPosition,
            _retractionCurve.Evaluate(Utils.GetPortionPassed(_retractionStartTime, _retractionTime)));

        _sideSpikes[1].transform.position = Vector2.Lerp(
            _sideSpikeFinalPosition[1],
            halalitPosition,
            _retractionCurve.Evaluate(Utils.GetPortionPassed(_retractionStartTime, _retractionTime)));
    }

    private bool IsExtractionFinished()
    {
        return Time.time >= _extractionStartTime + _extractionTime;
    }

    private bool IsWaitingFinished()
    {
        return Time.time >= _waitingStartTime + _waitingTime;
    }

    private bool IsRetractingFinished()
    {
        return Time.time >= _retractionStartTime + _retractionTime;
    }

    private Vector2 GetDestinationPosition(Vector2 startPosition, Vector2 rotation, float distance) // TODO: move to Utils
    {
        // Convert the rotation vector to radians
        float angleInRadians = Vector2ToRadians(rotation);

        // Calculate the endpoint's position
        float x = startPosition.x + Mathf.Cos(angleInRadians) * distance;
        float y = startPosition.y + Mathf.Sin(angleInRadians) * distance;

        return new Vector2(x, y);
    }

    float Vector2ToRadians(Vector2 direction)
    {
        // Use Mathf.Atan2 to convert the direction vector to radians
        float radians = Mathf.Atan2(direction.y, direction.x);

        // Mathf.Atan2 returns radians in the range of -π to π
        // To convert it to a full 0 to 2π range, you can add 2π to negative values
        if (radians < 0)
        {
            radians += 2 * Mathf.PI;
        }

        return radians;
    }


}