using System.Collections.Generic;
using UnityEngine;

public abstract class MoveAimAttackAttack : MonoBehaviour
{
    [SerializeField]
    private float _attackingInterval;

    protected float _attackingTime;
    protected bool _didShoot;
    private Dictionary<string, GameObject> _weapons = new();

    public void SetAttacking()
    {
        _attackingTime = Time.time + _attackingInterval;
        _didShoot = false;
    }

    public bool DidAttackingTimePass()
    {
        return Time.time > _attackingTime;
    }

    public virtual void AttackingState(Transform transform)
    {
        if (!_didShoot)
        {
            Shoot(transform);
            _didShoot = true;
        }
    }

    public abstract void Shoot(Transform transform);

    private GameObject GetWeapon(string weaponNumber)
    {        
        if (!_weapons.ContainsKey(Constants.WEAPON_GAME_OBJECT_NAME + weaponNumber))
        {
            _weapons.Add(Constants.WEAPON_GAME_OBJECT_NAME + weaponNumber, transform.Find(Constants.WEAPON_GAME_OBJECT_NAME + weaponNumber).gameObject);
        }

        return _weapons[Constants.WEAPON_GAME_OBJECT_NAME + weaponNumber];
    }

    protected Vector2 GetShootingPoint(string weaponNumber = "")
    {
        return GetWeapon(weaponNumber).transform.position;
    }

    protected Quaternion GetShootingRotation(string weaponNumber = "")
    {
        return GetWeapon(weaponNumber).transform.rotation;
    }
}