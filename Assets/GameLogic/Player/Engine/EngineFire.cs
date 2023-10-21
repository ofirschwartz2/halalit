using UnityEngine;
using Assets.Utils;

class EngineFire : MonoBehaviour
{

    [SerializeField]
    private float _lifetime;

    [SerializeField]
    private float _lifetimeVariance;

    private float _endOfLifeTime;

    void Start()
    {
        _endOfLifeTime = Utils.GetEndOfLifeTime(Utils.GetRandomBetween(_lifetime, _lifetimeVariance));
    }

    void FixedUpdate()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            Destroy(gameObject);
        }
    }
}
