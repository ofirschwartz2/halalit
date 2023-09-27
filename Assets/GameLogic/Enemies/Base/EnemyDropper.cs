using Assets.Enums;
using UnityEngine;

class EnemyDropper : MonoBehaviour
{
    [SerializeField]
    private Dropper _dropper;

    public Dropper GetDropper()
    {
        return _dropper;
    }
}
