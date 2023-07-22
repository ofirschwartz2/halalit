using Assets.Enums;
using UnityEngine;

class NewShootingInRangeEnemy : INewGameObject
{
    public GameObject Prefab;

    public NewShootingInRangeEnemy(GameObject prefab)
    {
        Prefab = prefab;
    }

    public int? GetEdgeWidthForInstantiation()
    {
        return 2;
    }

    public GameObject GetPrefab()
    {
        return Prefab;
    }
}