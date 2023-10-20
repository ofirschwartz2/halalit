using UnityEngine;
using Assets.Utils;

class EngineFire : MonoBehaviour
{

    [SerializeField]
    private float _lifetime;

    private float _endOfLifeTime;

    void Start()
    {
        _endOfLifeTime = Utils.GetEndOfLifeTime(_lifetime);
    }

    void FixedUpdate()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            Destroy(gameObject);
        }
    }
}
