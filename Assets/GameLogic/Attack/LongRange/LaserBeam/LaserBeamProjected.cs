using Assets.Utils;
using UnityEngine;

public class LaserBeamProjected : LaserBeamBase 
{
    protected override void SetNewBeamDistanceFromStartPosition()
    {
        float maxBeamSize = Utils.GetDistanceBetweenTwoPoints(_startPosition, _target);

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
        SetNewStartPosition();
        UpdateConsecitiveAttack(_startPosition, _lastRotation);
    }
}