﻿using Assets.Animators;
using Assets.Enums;
using UnityEngine;

public class PickupClawStateMachine : MonoBehaviour
{
    [SerializeField]
    private PickupClawState _pickupClawState;
    [SerializeField]
    private PickupClawShooter _pickupClawShooter;
    [SerializeField]
    private PickupClawGrabber _pickupClawGrabber;
    [SerializeField]
    private PickupClawRetractor _pickupClawRetractor;
    [SerializeField]
    private PickupClawAnimator _pickupClawAnimator;

    private PickupClawStateE _lastFramePickupClawState;

    void Start()
    {
        _lastFramePickupClawState = PickupClawStateE.IN_HALALIT;
    }

    void Update()
    {
        _lastFramePickupClawState = _pickupClawState.Value;

        _pickupClawShooter.TryShoot();
        _pickupClawRetractor.TryRetract();

        HandleMovingForwardTransition();
        HandleGrabbingTransition();
        HandleMovingBackwardTransition();
        HandleReturningToHalalitTransition();

        _lastFramePickupClawState = _pickupClawState.Value;
    }

    #region Transition Handler
    private void HandleMovingForwardTransition()
    {
        if (StartMovingForward())
        {
            _pickupClawShooter.Shoot();
            _pickupClawAnimator.StartMovingForward();
        }
    }

    private void HandleGrabbingTransition()
    {
        if (StartGrabbing())
        {
            _pickupClawGrabber.Grab();
            _pickupClawAnimator.StartGrabbing();
        }
        else if (IsGrabbing())
        {
            _pickupClawGrabber.Grab();
        }
    }

    private void HandleMovingBackwardTransition()
    {
        if (StartMovingBackward())
        {
            _pickupClawRetractor.Retract();
            _pickupClawAnimator.StartMovingBackward();
        }
        else if (IsMovingBackward())
        {
            _pickupClawRetractor.Retract();
        }
    }

    private void HandleReturningToHalalitTransition()
    {
        if (ReturningToHalalit())
        {
            _pickupClawRetractor.FinalizeRetraction();
            _pickupClawAnimator.ReturningToHalalit();
        }
    }
    #endregion

    #region State Predicates
    private bool StartMovingForward()
    {
        return _lastFramePickupClawState == PickupClawStateE.IN_HALALIT && _pickupClawState.Value == PickupClawStateE.MOVING_FORWARD;
    }

    private bool StartGrabbing()
    {
        return _pickupClawState.Value == PickupClawStateE.GRABBING && _lastFramePickupClawState != PickupClawStateE.GRABBING;
    }

    private bool IsGrabbing()
    {
        return _pickupClawState.Value == PickupClawStateE.GRABBING && _lastFramePickupClawState == PickupClawStateE.GRABBING;
    }

    private bool StartMovingBackward()
    {
        return _pickupClawState.Value == PickupClawStateE.MOVING_BACKWARD && _lastFramePickupClawState != PickupClawStateE.MOVING_BACKWARD;
    }

    private bool IsMovingBackward()
    {
        return _pickupClawState.Value == PickupClawStateE.MOVING_BACKWARD;
    }

    private bool ReturningToHalalit()
    {
        return _lastFramePickupClawState == PickupClawStateE.MOVING_BACKWARD && _pickupClawState.Value == PickupClawStateE.IN_HALALIT;
    }
    #endregion
}