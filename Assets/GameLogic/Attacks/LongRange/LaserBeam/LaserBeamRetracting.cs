using Assets.Utils;
using System;
using UnityEngine;

public class LaserBeamRetracting : LaserBeamBase
{
    [SerializeField]
    private float _maxRange;

    protected override void SetNewBeamDistanceFromStartPosition()
    {
        float maxBeamSize = Math.Min(Utils.GetDistanceBetweenTwoPoints(_startPosition, _target), _maxRange);

        if (_distanceFromStartPosition + _beamSpeed * Time.deltaTime > maxBeamSize)
        {
            _distanceFromStartPosition = maxBeamSize;
            _endVfxParticles.SetActive(true);
        }
        else
        {
            _distanceFromStartPosition += _beamSpeed * Time.deltaTime;
            _endVfxParticles.SetActive(false);
        }
    }

    protected override void FinalizeLaserBeam()
    {
        MinimizeMaxRange();
        UpdateConsecitiveAttack(_startPosition, _lastRotation);
    }

    private void MinimizeMaxRange()
    {
        if (_maxRange - _beamSpeed * Time.deltaTime <= 0)
        {
            Destroy(gameObject);
        }

        _maxRange -= _beamSpeed * Time.deltaTime;
    }
}