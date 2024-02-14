using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class HalalitWeaponTests
{

    private const string SCENE_NAME = "Playground";

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator JoystickUnderAttackTrigger()
    {

        var weapon = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription());
        var weaponAttack = weapon.GetComponent<WeaponAttack>();

        var randomTouchOnAttackJoystick = GetRandomTouchUnderAttackTrigger(weaponAttack);

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());        

        Assert.IsNull(shot);
    }

    [UnityTest]
    public IEnumerator JoystickDirectionToWeaponDirection()
    {

        var weapon = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription());
        var weaponMovement = weapon.GetComponent<WeaponMovement>();
        var acceptedDelta = 0.1f;

        Vector2 randomTouchOnAttackJoystick = Vector2.zero;
        while (randomTouchOnAttackJoystick == Vector2.zero)
        {
            randomTouchOnAttackJoystick = GetRandomTouch();
        } 
        
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);

        yield return null; // TODO: WHEN WE HAVE DRAG WAIT FOR X SECONDS

        Assert.AreEqual(
            Utils.Vector2ToDegrees(randomTouchOnAttackJoystick),
            Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation)),
            acceptedDelta
            );
    }

    [UnityTest]
    public IEnumerator JoystickOverAttackTrigger()
    {
        var weapon = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription());
        var weaponAttack = weapon.GetComponent<WeaponAttack>();
    
        var randomTouchOnAttackJoystick = GetRandomTouchOverAttackTrigger(weaponAttack);
    
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
                
        Assert.IsNotNull(shot);
    }

    #region random touch
    private Vector2 GetRandomTouch()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }

    private Vector2 GetRandomTouchUnderAttackTrigger(WeaponAttack weaponAttack) 
    {
        Vector2 randomTouch;
        do
        {
            randomTouch = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        } while (randomTouch.magnitude >= weaponAttack.GetAttackJoystickEdge());

        return randomTouch;
    }

    private Vector2 GetRandomTouchOverAttackTrigger(WeaponAttack weaponAttack)
    {
        Vector2 randomTouch;
        do
        {
            randomTouch = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        } while (randomTouch.magnitude < weaponAttack.GetAttackJoystickEdge());

        return randomTouch;
    }
    #endregion

}