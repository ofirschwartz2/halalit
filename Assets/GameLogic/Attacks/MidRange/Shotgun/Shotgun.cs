using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsPlayMode")]
#endif

public class Shotgun : MonoBehaviour 
{
    [SerializeField]
    private MinMaxRange _numberOfShots;
    [SerializeField]
    private float _range;
    [SerializeField]
    private float _lifeTime;
    [SerializeField]
    private GameObject _shotgunShotPrefab;
    
    private float _endOfLifeTime;

    void Start()
    {
        InstantiateShots((int)CombatRandomGenerator.GetSeedfulRandomGenerator().Range(_numberOfShots.min, _numberOfShots.max + 1), _range);
        _endOfLifeTime = Time.time + _lifeTime;
    }

    void FixedUpdate()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            Destroy(gameObject);
        }
    }

    private void InstantiateShots(int numberOfShots, float shootingRange)
    {
        var combatRandomGenerator = CombatRandomGenerator.GetSeedfulRandomGenerator();

        for (var i = 0; i < numberOfShots; i++)
        {
            var angle = Utils.GetRandomAngleAround(combatRandomGenerator, shootingRange);
            GameObject shotgunShot = Instantiate(_shotgunShotPrefab, transform.position, Utils.GetRotationPlusAngle(transform.rotation, angle));
            shotgunShot.GetComponent<ShotgunShot>().SetInitialSeedfulRandomGenerator(combatRandomGenerator.GetNumber());
            shotgunShot.GetComponent<AttackBehaviour>().Copy(GetComponent<AttackBehaviour>());
        }
    }

#if UNITY_EDITOR
    internal MinMaxRange GetNumberOfShots()
    {
        return _numberOfShots;
    }

    internal float GetRange()
    {
        return _range;
    }

#endif
}