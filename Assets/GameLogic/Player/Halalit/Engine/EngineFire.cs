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

        var mainModule = _engineFireParticleSystem.main;
        mainModule.startSpeed = Random.Range(fromSpeed, fromSpeed * _engineFireToSpeedMultiplier);
    }

    private float GetEngineFireForce()
    {
        return Utils.GetLengthOfLine(_joystick.Horizontal, _joystick.Vertical) * _engineFireForceMultiplier;
    }



}