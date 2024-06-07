using Assets.Enums;
using System.Collections.Generic;
using UnityEngine;

class EdgesForceFieldsManager : MonoBehaviour
{
    [SerializeField]
    private List<Tag> _pushies;
    [SerializeField]
    private int _wallSlowerForceMagnitude;
    [SerializeField]
    private int _wallPusherForceMagnitude;
    [SerializeField]
    private float _wallResistanceMultiplier;
    [SerializeField]
    private bool _wallImplsePush;
    [SerializeField]
    private int _cornerSlowerForceMagnitude;
    [SerializeField]
    private int _cornerPusherForceMagnitude;
    [SerializeField]
    private float _cornerResistanceMultiplier;
    [SerializeField]
    private bool _cornerImplsePush;
    [SerializeField]
    private KeyValuePair<GameObject, Vector2> _bottomWallForceField;
    [SerializeField]
    private KeyValuePair<GameObject, Vector2> _leftWallForceField;
    [SerializeField]
    private KeyValuePair<GameObject, Vector2> _rightWallForceField;
    [SerializeField]
    private KeyValuePair<GameObject, Vector2> _topWallForceField;
    [SerializeField]
    private KeyValuePair<GameObject, Vector2> _bottomRightCornerForceField;
    [SerializeField]
    private KeyValuePair<GameObject, Vector2> _bottomLeftCornerForceField;
    [SerializeField]
    private KeyValuePair<GameObject, Vector2> _topLeftCornerForceField;
    [SerializeField]
    private KeyValuePair<GameObject, Vector2> _topRightCornerForceField;

    private List<KeyValuePair<GameObject, Vector2>> _wallForceFields;
    private List<KeyValuePair<GameObject, Vector2>> _cornerForceFields;

    private void Awake()
    {
        _wallForceFields = new() { _bottomWallForceField, _leftWallForceField, _rightWallForceField, _topWallForceField };
        _cornerForceFields = new() { _bottomRightCornerForceField, _bottomLeftCornerForceField, _topLeftCornerForceField, _topRightCornerForceField };

        SetForceFieldsConfiguration();
    }

    private void SetForceFieldsConfiguration()
    {
        foreach (KeyValuePair<GameObject, Vector2> wallForceFieldAndDirection in _wallForceFields)
        {
            ForceField wallForceField = wallForceFieldAndDirection.Key.GetComponent<ForceField>();
            wallForceField.SetPushies(_pushies);
            wallForceField.SetSlowerForceMagnitude(_wallSlowerForceMagnitude);
            wallForceField.SetPusherForceMagnitude(_wallPusherForceMagnitude);
            wallForceField.SetResistanceMultiplier(_wallResistanceMultiplier);
            wallForceField.SetImpulsePush(_wallImplsePush);
            wallForceField.SetForceDirection(wallForceFieldAndDirection.Value);
        }

        foreach (KeyValuePair<GameObject, Vector2> cornerForceFieldAndDirection in _cornerForceFields)
        {
            ForceField cornerForceField = cornerForceFieldAndDirection.Key.GetComponent<ForceField>();
            cornerForceField.SetPushies(_pushies);
            cornerForceField.SetSlowerForceMagnitude(_cornerSlowerForceMagnitude);
            cornerForceField.SetPusherForceMagnitude(_cornerPusherForceMagnitude);
            cornerForceField.SetResistanceMultiplier(_cornerResistanceMultiplier);
            cornerForceField.SetImpulsePush(_cornerImplsePush);
            cornerForceField.SetForceDirection(cornerForceFieldAndDirection.Value);
        }
    }
}
