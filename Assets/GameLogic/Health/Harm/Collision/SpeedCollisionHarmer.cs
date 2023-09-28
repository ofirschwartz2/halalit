using System;
using UnityEngine;

class SpeedCollisionHarmer : CollisionHarmer
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private bool _useLinearSpeed;
    [SerializeField]
    private float _linearHarmMultiplier;
    [SerializeField]
    private bool _useAngularSpeed;
    [SerializeField]
    private float _angularHarmMultiplier;

    public override int GetCollisionHarm()
    {
        float harm = 0;

        if (_useLinearSpeed)
        {
            harm += GetLinearSpeed() * _linearHarmMultiplier;
        }

        if (_useAngularSpeed)
        {
            harm += GetAngularSpeed() * _angularHarmMultiplier;
        }

        return (int) Math.Floor(harm);
    }

    private float GetLinearSpeed()
    {
        return _rigidBody.isKinematic ? GetComponent<KinematicMovement>().GetSpeed() : _rigidBody.velocity.magnitude;
    }

    private float GetAngularSpeed()
    {
        return _rigidBody.isKinematic ? GetComponent<KinematicMovement>().GetRotationSpeed() : _rigidBody.angularVelocity;
    }
}