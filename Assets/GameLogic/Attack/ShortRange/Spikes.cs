using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class Spikes : MonoBehaviour 
{
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
    private float _extractionStartTime, _waitingStartTime, _retractionStartTime, _halalitDiameter;

    void Start()
    {
        _sideSpikes = new GameObject[2];

        InstantiateSpikes();

        _state = SpikesState.EXTRACTING;
        _halalitDiameter = Utils.GetHalalitTransform().localScale.magnitude * 1.8f; // TODO: needs to fix halait's collider side
    }

    void FixedUpdate()
    {
        switch (_state) 
        {
            case SpikesState.EXTRACTING:
                Extracting();
                break;
            case SpikesState.WAITING_OUT:
                WaitingOut();
                break;
            case SpikesState.RETRACTING:
                Retracting();
                break;
        }
    }

    #region EXTRACTING
    private void Extracting()
    {
        ExtractSpikes();
        TryFinishExtracting();
    }

    private void TryFinishExtracting()
    {
        if (IsExtractionFinished())
        {
            _waitingStartTime = Time.time;
            _state = SpikesState.WAITING_OUT;
        }
    }

    private void ExtractSpikes()
    {
        var halalitPosition = Utils.GetHalalitPosition();
        var centerSpikeFinalPosition = Utils.GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(_centerSpike.transform.rotation), _halalitDiameter);
        var sideSpike1FinalPosition = Utils.GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, -_sideSpikesRotaion)), _halalitDiameter);
        var sideSpike2FinalPosition = Utils.GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, _sideSpikesRotaion)), _halalitDiameter);

        _centerSpike.transform.position = Vector2.Lerp(
            halalitPosition,
            centerSpikeFinalPosition,
            _extractionCurve.Evaluate(Utils.GetPortionPassed(_extractionStartTime, _extractionTime)));

        _sideSpikes[0].transform.position = Vector2.Lerp(
            halalitPosition,
            sideSpike1FinalPosition,
            _extractionCurve.Evaluate(Utils.GetPortionPassed(_extractionStartTime, _extractionTime)));

        _sideSpikes[1].transform.position = Vector2.Lerp(
            halalitPosition,
            sideSpike2FinalPosition,
            _extractionCurve.Evaluate(Utils.GetPortionPassed(_extractionStartTime, _extractionTime)));
    }

    private bool IsExtractionFinished()
    {
        return Time.time >= _extractionStartTime + _extractionTime;
    }
    #endregion

    #region WAITING_OUT
    private void WaitingOut()
    {
        WaitSpikes();
        if (IsWaitingOutFinished())
        {
            _retractionStartTime = Time.time;
            _state = SpikesState.RETRACTING;
        }
    }

    private void WaitSpikes()
    {
        var halalitPosition = Utils.GetHalalitPosition();
        var rotation = Utils.GetRotationAsVector2(_centerSpike.transform.rotation);

        _centerSpike.transform.position = Utils.GetDestinationPosition(
            halalitPosition,
            rotation,
            _halalitDiameter);

        _sideSpikes[0].transform.position = Utils.GetDestinationPosition(
            halalitPosition,
            Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, -_sideSpikesRotaion)),
            _halalitDiameter);

        _sideSpikes[1].transform.position = Utils.GetDestinationPosition(
            halalitPosition,
            Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, _sideSpikesRotaion)),
            _halalitDiameter);
    }

    private bool IsWaitingOutFinished()
    {
        return Time.time >= _waitingStartTime + _waitingTime;
    }
    #endregion

    #region RETRACTING
    private void Retracting()
    {
        RetractSpikes();
        if (IsRetractingFinished())
        {
            Die();
        }
    }

    private void RetractSpikes()
    {
        var halalitPosition = Utils.GetHalalitPosition();
        var rotation = Utils.GetRotationAsVector2(_centerSpike.transform.rotation);
        var centerSpikeFinalPosition = Utils.GetDestinationPosition(halalitPosition, rotation, _halalitDiameter);
        var sideSpike1FinalPosition = Utils.GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, -_sideSpikesRotaion)), _halalitDiameter);
        var sideSpike2FinalPosition = Utils.GetDestinationPosition(halalitPosition, Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(transform.rotation, _sideSpikesRotaion)), _halalitDiameter);

        _centerSpike.transform.position = Vector2.Lerp(
            centerSpikeFinalPosition,
            halalitPosition,
            _retractionCurve.Evaluate(Utils.GetPortionPassed(_retractionStartTime, _retractionTime)));

        _sideSpikes[0].transform.position = Vector2.Lerp(
            sideSpike1FinalPosition,
            halalitPosition,
            _retractionCurve.Evaluate(Utils.GetPortionPassed(_retractionStartTime, _retractionTime)));

        _sideSpikes[1].transform.position = Vector2.Lerp(
            sideSpike2FinalPosition,
            halalitPosition,
            _retractionCurve.Evaluate(Utils.GetPortionPassed(_retractionStartTime, _retractionTime)));
    }

    private bool IsRetractingFinished()
    {
        return Time.time >= _retractionStartTime + _retractionTime;
    }
    #endregion

    private void InstantiateSpikes()
    {
        var halalitPosition = Utils.GetHalalitPosition();

        _centerSpike = Instantiate(_centerSpikePrefab, halalitPosition, transform.rotation);
        _sideSpikes[0] = Instantiate(_sideSpikePrefab, halalitPosition, Utils.GetRotationPlusAngle(transform.rotation, -_sideSpikesRotaion));
        _sideSpikes[1] = Instantiate(_sideSpikePrefab, halalitPosition, Utils.GetRotationPlusAngle(transform.rotation, _sideSpikesRotaion));

        _centerSpike.GetComponent<AttackBehaviour>().Copy(GetComponent<AttackBehaviour>());
        _sideSpikes[0].GetComponent<AttackBehaviour>().Copy(GetComponent<AttackBehaviour>());
        _sideSpikes[1].GetComponent<AttackBehaviour>().Copy(GetComponent<AttackBehaviour>());

        _extractionStartTime = Time.time;
    }

    private void Die()
    {
        Destroy(_centerSpike);
        Destroy(_sideSpikes[0]);
        Destroy(_sideSpikes[1]);
        Destroy(gameObject);
    }
}