using Assets.Enums;
using Assets.Utils;
using UnityEngine;
using UnityEngine.UIElements;

public class Sword : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _attackTime;
    [SerializeField]
    private float _swordRotationRange;
    [SerializeField]
    private AnimationCurve accelerationCurve; // TODO: fix to _movementCurve

    private float _attackStartTime;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

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
        transform.parent.position = weaponTransform.position;

    }

    private void SetRotation(Transform weaponTransform)
    {
        var fromRotation = Utils.GetRotationPlusAngle(weaponTransform.rotation, -0.5f * _swordRotationRange);
        var toRotation = Utils.GetRotationPlusAngle(weaponTransform.rotation, 0.5f * _swordRotationRange);

        transform.parent.rotation = Quaternion.Slerp(
            fromRotation, 
            toRotation, 
            accelerationCurve.Evaluate(Utils.GetPortionPassed(_attackStartTime, _attackTime)));
    }

    private Transform GetWeaponTransform()
    {
        return GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription()).transform;
    }

    private void TryDie()
    {
        if (ShouldDie())
        {
            Destroy(gameObject);
            Destroy(transform.parent.gameObject);
        }
    }

    private bool ShouldDie()
    {
        return Time.time >= _attackStartTime + _attackTime;
    }
}