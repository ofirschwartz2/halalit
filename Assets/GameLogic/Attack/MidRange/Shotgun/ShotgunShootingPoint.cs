using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

// TODO (refactor): the stats (damage) of a shot (when it's collide with enemy) needs to be on the shot script
// TODO (refactor): move all shots out of the Gun. Enemies also shoot now.
public class ShotgunShootingPoint : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private int _numberOfShots;
    [SerializeField]
    private float _range;
    [SerializeField]
    private GameObject _shotgunShotPrefab;
    
    private float _endOfLifeTime;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        InstantiateShots(_numberOfShots, _range);
        _endOfLifeTime = Time.time + 10; // TODO: fix this
    }

    void FixedUpdate()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            Destroy(gameObject);
        }
    }

    private void InstantiateShots(int numberOfShots, float range)
    {
        for (var i = 0; i < numberOfShots; i++)
        {
            var angle = Utils.GetRandomAngleAround(range);
            Instantiate(_shotgunShotPrefab, transform.position, Utils.GetRotationPlusAngle(transform.rotation, angle));
        }
    }

}