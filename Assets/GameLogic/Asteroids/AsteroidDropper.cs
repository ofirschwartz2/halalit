using Assets.Enums;
using UnityEngine;

class AsteroidDropper : MonoBehaviour
{
    [SerializeField]
    private Dropper _dropper;

    public Dropper GetDropper()
    {
        return _dropper;
    }
}
