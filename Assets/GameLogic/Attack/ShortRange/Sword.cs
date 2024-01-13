using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class Sword : MonoBehaviour 
{
    [SerializeField]
    private float _attackTime;
    [SerializeField]
    private float _swordRotationRange;
    [SerializeField]
    private AnimationCurve accelerationCurve; // TODO: fix to _movementCurve

    private float _attackStartTime;

    void Start()
    {
        _attackStartTime = Time.time;
        SetRotationAndPosition();
    }

    void FixedUpdate()
    {
        SetRotationAndPosition(); 
        TryDie();
    }

    private void SetRotationAndPosition()
    {
        var weaponTransform = GetWeaponTransform();
        SetRotation(weaponTransform);
        transform.position = weaponTransform.position;
    }

    private void SetRotation(Transform weaponTransform)
    {
        var fromRotation = Utils.GetRotationPlusAngle(weaponTransform.rotation, -0.5f * _swordRotationRange);
        var toRotation = Utils.GetRotationPlusAngle(weaponTransform.rotation, 0.5f * _swordRotationRange);

        transform.rotation = Quaternion.Slerp(
            fromRotation, 
            toRotation, 
            accelerationCurve.Evaluate(Utils.GetPortionPassed(_attackStartTime, _attackTime)));
    }

    private Transform GetWeaponTransform()
    {
        return GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription()).transform; // TODO (refactor): This is a very expensive performance function, change this to something else
    }

    private void TryDie()
    {
        if (ShouldDie())
        {
            Destroy(gameObject);
        }
    }

    private bool ShouldDie()
    {
        return Time.time >= _attackStartTime + _attackTime;
    }
}