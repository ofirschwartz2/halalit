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

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
        _sideSpikes = new GameObject[2];
        InstantiateSpikes();
        _state = SpikesState.DETRACTING;
    }

    private void InstantiateSpikes()
    {
        _centerSpike = Instantiate(_centerSpikePrefab, Utils.GetHalalitPosition(), transform.rotation);
        _sideSpikes[0] = Instantiate(_sideSpikePrefab, Utils.GetHalalitPosition(), Utils.GetRotationPlusAngle(transform.rotation, -_sideSpikesRotaion));
        _sideSpikes[1] = Instantiate(_sideSpikePrefab, Utils.GetHalalitPosition(), Utils.GetRotationPlusAngle(transform.rotation, _sideSpikesRotaion));
    }

    void Update()
    {
        switch (_state) 
        {
            case SpikesState.EXTRACTING:
                ExtractSpikes();
                break;
            case SpikesState.OUT:
                WaitSpikes();
                break;
            case SpikesState.DETRACTING:
                RetractSpikes();
                break;
        }
    }
}