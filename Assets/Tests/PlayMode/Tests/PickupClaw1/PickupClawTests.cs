using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class PickupClawTests
{

    private const string SCENE_NAME = "TestingPickupClaw";

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(SCENE_NAME);
    }
    
    
    [UnityTest]
    public IEnumerator ClawPressOnNothing()
    {
        // GIVEN
        GameObject pickupClawShooterGameObject = GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW_SHOOTER.GetDescription());
        PickupClawShooter pickupClawShooter = pickupClawShooterGameObject.GetComponent<PickupClawShooter>();
        var position = new Vector2(-1, 0);

        // WHEN
        pickupClawShooter.TryShootClaw(position);
        yield return null;

        // THEN
        var pickupClaw = GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW.GetDescription());
        Assert.IsNull(pickupClaw);
        
    }

    [UnityTest]
    public IEnumerator ClawPressOnJoystick()
    {
        // GIVEN
        GameObject pickupClawShooterGameObject = GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW_SHOOTER.GetDescription());
        PickupClawShooter pickupClawShooter = pickupClawShooterGameObject.GetComponent<PickupClawShooter>();
        GameObject attackJoystick = GameObject.FindGameObjectWithTag(Tag.ATTACK_JOYSTICK.GetDescription());
        
        var position = attackJoystick.transform.position;

        // WHEN
        pickupClawShooter.TryShootClaw(position);
        yield return null;

        // THEN
        var pickupClaw = GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW.GetDescription());
        Assert.IsNull(pickupClaw);

    }

    [UnityTest]
    public IEnumerator ClawPressOnItemInRange()
    {
        // GIVEN
        GameObject pickupClawShooterGameObject = GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW_SHOOTER.GetDescription());
        PickupClawShooter pickupClawShooter = pickupClawShooterGameObject.GetComponent<PickupClawShooter>();
        GameObject attackJoystick = GameObject.FindGameObjectWithTag(Tag.ATTACK_JOYSTICK.GetDescription());

        var position = new Vector2(4,2);

        // WHEN
        pickupClawShooter.TryShootClaw(position);
        yield return null;

        // THEN
        var pickupClaw = GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW.GetDescription());
        Assert.IsNull(pickupClaw);

    }

}