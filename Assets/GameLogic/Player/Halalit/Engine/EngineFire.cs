using Assets.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

public class EngineFire : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _engineFireParticleSystem;
    [SerializeField]
    private Joystick _joystick;
    [SerializeField]
    private float _engineFireForceMultiplier;
    [SerializeField]
    private float _engineFireToSpeedMultiplier;

    void FixedUpdate()
    {
        var fromSpeed = GetEngineFireForce();
        var toSpeed = fromSpeed * _engineFireToSpeedMultiplier;
        var mainModule = _engineFireParticleSystem.main;

        if (fromSpeed == 0)
        {
            TryTurnOffEngineFireParticleSystem();
        }
        else
        {
            TryTurnOnEngineFireParticleSystem();
            mainModule.startSpeed = RandomGenerator.Range(fromSpeed, toSpeed);
        }
    }

    #region Switching Engine Fire Particle System
    private void TryTurnOnEngineFireParticleSystem()
    {
        if (_engineFireParticleSystem.isStopped)
        {
            _engineFireParticleSystem.Clear();
            _engineFireParticleSystem.Play();
        }
    }

    private void TryTurnOffEngineFireParticleSystem()
    {
        if (!_engineFireParticleSystem.isStopped)
        {
            _engineFireParticleSystem.Stop();
        }
    }
    #endregion

    private float GetEngineFireForce()
    {
        return Utils.GetLengthOfLine(_joystick.Horizontal, _joystick.Vertical) * _engineFireForceMultiplier;
    }



}