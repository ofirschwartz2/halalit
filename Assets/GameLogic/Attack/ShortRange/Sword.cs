using Assets.Enums;
using Assets.Utils;
using UnityEngine;
using UnityEngine.UIElements;

public class Sword : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _attackTime;
    [SerializeField]
    private float _swordRotationRange;
    [SerializeField]
    private AnimationCurve accelerationCurve;

    private Quaternion _fromRotation, _toRotation;
    private float _attackStartTime;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        SetRotations();
    }

    void FixedUpdate()
    {
        var weaponTransform = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription()).transform;
        _fromRotation = Utils.GetRotation(weaponTransform.rotation, -0.5f * _swordRotationRange);
        _toRotation = Utils.GetRotation(weaponTransform.rotation, 0.5f * _swordRotationRange);
        transform.parent.position = weaponTransform.position;
        transform.parent.rotation = Quaternion.Slerp(_fromRotation, _toRotation, accelerationCurve.Evaluate((Time.time - _attackStartTime) / (_attackTime)));
        TryDie();
    }

    private void SetRotations()
    {
        _attackStartTime = Time.time;
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