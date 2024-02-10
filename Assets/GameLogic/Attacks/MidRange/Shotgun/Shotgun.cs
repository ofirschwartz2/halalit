using Assets.Utils;
using UnityEngine;

public class Shotgun : MonoBehaviour 
{
    [SerializeField]
    private MinMaxRange _numberOfShots;
    [SerializeField]
    private float _range;
    [SerializeField]
    private GameObject _shotgunShotPrefab;
    
    private float _endOfLifeTime;

    void Start()
    {
        InstantiateShots((int)Random.Range(_numberOfShots.min, _numberOfShots.max + 1), _range);
        _endOfLifeTime = Time.time + 10; // TODO (DEV): fix this
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
            GameObject shotgunShot = Instantiate(_shotgunShotPrefab, transform.position, Utils.GetRotationPlusAngle(transform.rotation, angle));
            shotgunShot.GetComponent<AttackBehaviour>().Copy(GetComponent<AttackBehaviour>());
        }
    }
}