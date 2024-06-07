using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    [SerializeField]
    private List<Tag> _pushies;
    [SerializeField]
    private Vector2 _forceDirection;
    [SerializeField]
    private int _slowerForceMagnitude;
    [SerializeField]
    private int _pusherForceMagnitude;
    [SerializeField]
    private bool _impulsePush;
    [SerializeField]
    private float _resistanceMultiplier;

    private List<string> _pushiesDescriptions;
    private List<Rigidbody2D> _activePushies;

    private void Start()
    {
        _pushiesDescriptions = _pushies.Select(tag => Utils.GetDescription(tag)).ToList();
        _activePushies = new();
        _forceDirection.Normalize();
    }

    private void FixedUpdate()
    {
        foreach (Rigidbody2D activePushie in _activePushies)
        {
            if (AgainstForceDirection(activePushie))
            {
                if (_impulsePush)
                {
                    activePushie.AddForce(_forceDirection * (_slowerForceMagnitude + GetActivePushieResistance(activePushie) * _resistanceMultiplier), ForceMode2D.Impulse);
                } 
                else
                {
                    activePushie.AddForce(_forceDirection * (_slowerForceMagnitude + GetActivePushieResistance(activePushie) * _resistanceMultiplier), ForceMode2D.Force);
                }
                    
            }
            else
            {
                if (_impulsePush)
                {
                    activePushie.AddForce(_forceDirection * _pusherForceMagnitude, ForceMode2D.Impulse);
                }
                else
                {
                    activePushie.AddForce(_forceDirection * _pusherForceMagnitude, ForceMode2D.Force);
                }
            }
        }
    }

    private float GetActivePushieResistance(Rigidbody2D activePushie)
    {
        if (_forceDirection.x > 0 && activePushie.velocity.x < 0 || _forceDirection.x < 0 && activePushie.velocity.x > 0)
        {
            return Math.Abs(activePushie.velocity.x);
        }
        else
        {
            return Math.Abs(activePushie.velocity.y);
        }
    }

    private bool AgainstForceDirection(Rigidbody2D activePushie)
    {
        return 
            _forceDirection.x > 0 && activePushie.velocity.x < 0 ||
            _forceDirection.x < 0 && activePushie.velocity.x > 0 ||
            _forceDirection.y > 0 && activePushie.velocity.y < 0 ||
            _forceDirection.y < 0 && activePushie.velocity.y > 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_pushiesDescriptions.Contains(other.gameObject.tag))
        {
            _activePushies.Add(other.gameObject.GetComponent<Rigidbody2D>());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_pushiesDescriptions.Contains(other.gameObject.tag))
        {
            _activePushies.Remove(other.gameObject.GetComponent<Rigidbody2D>());
        }
    }

    public void SetPushies(List<Tag> pushies)
    {
        _pushies = pushies;
    }

    public void SetForceDirection(Vector2 forceDirection)
    {
        _forceDirection = forceDirection;
    }

    public void SetSlowerForceMagnitude(int slowerForceMagnitude)
    {
        _slowerForceMagnitude = slowerForceMagnitude;
    }

    public void SetPusherForceMagnitude(int pusherForceMagnitude)
    {
        _pusherForceMagnitude = pusherForceMagnitude;
    }

    public void SetResistanceMultiplier(float resistanceMultiplier)
    {
        _resistanceMultiplier = resistanceMultiplier;
    }

    public void SetImpulsePush(bool impulsePush)
    {
        _impulsePush = impulsePush;
    }
}