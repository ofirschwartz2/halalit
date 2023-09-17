using Assets.Utils;
using System.Diagnostics.Contracts;
using UnityEngine;

// TODO (refactor): the stats (damage) of a shot (when it's collide with enemy) needs to be on the shot script
// TODO (refactor): move all shots out of the Gun. Enemies also shoot now.
public class BlastCommon : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _blastMultiplier;
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    private AnimationCurve blastCurve;

    private float _startOfLifeTime, _endOfLifeTime;
    private float _blastCommonMultiplier;
    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        SetLifeTimes();
    }

    private void Update()
    {
        _blastCommonMultiplier = GetBlastMultiplier();
    }

    public float GetBlastMultiplier() // returns between 0 and _blastMultiplier
    {
        return blastCurve.Evaluate(Utils.GetPortionPassed(_startOfLifeTime, _lifetime)) * _blastMultiplier;
    }

    private void SetLifeTimes()
    {
        _startOfLifeTime = Time.time;
        _endOfLifeTime = Utils.GetEndOfLifeTime(_lifetime);
    }

    public float GetEndOfLifeTime() 
    {
        return _endOfLifeTime;
    }

    public float GetBlastCommonMultiplier()
    {
        return _blastCommonMultiplier;
    }

}