using Assets.Utils;
using UnityEngine;
using Items.Utility;
using Assets.Models;

public class EngineFire : MonoBehaviour, IEngineFireController
{
    [SerializeField]
    private ParticleSystem _engineFireParticleSystem;
    [SerializeField]
    private Joystick _joystick;
    [SerializeField]
    private float _engineFireForceMultiplier;
    [SerializeField]
    private float _engineFireToSpeedMultiplier;
    [SerializeField]
    private Color _normalColor = Color.red;
    [SerializeField]
    private Color _nitroColor = Color.blue;
    [SerializeField]
    private float _colorTransitionSpeed = 2f;

    private Color _currentColor;
    private Color _targetColor;

    private void Start()
    {
        _currentColor = _normalColor;
        _targetColor = _normalColor;
        UpdateParticleSystemColor(_normalColor);
    }

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
            mainModule.startSpeed = SeedlessRandomGenerator.Range(fromSpeed, toSpeed);
        }

        // Smoothly transition color
        if (_currentColor != _targetColor)
        {
            _currentColor = Color.Lerp(_currentColor, _targetColor, Time.fixedDeltaTime * _colorTransitionSpeed);
            UpdateParticleSystemColor(_currentColor);
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

    private void UpdateParticleSystemColor(Color color)
    {
        var main = _engineFireParticleSystem.main;
        main.startColor = color;
    }

    public void SetNitroActive(bool isActive)
    {
        _targetColor = isActive ? _nitroColor : _normalColor;
    }
}