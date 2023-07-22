using Assets.Enums;
using UnityEngine;

class NewShootingLazerRangeEnemy : INewGameObject
{
    public GameObject Prefab;

    public NewShootingLazerRangeEnemy(GameObject prefab)
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