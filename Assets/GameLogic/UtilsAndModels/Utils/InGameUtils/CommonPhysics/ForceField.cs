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
                activePushie.AddForce(_forceDirection * _slowerForceMagnitude);
            }
            else
            {
                activePushie.AddForce(_forceDirection * _pusherForceMagnitude);
            }
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
}