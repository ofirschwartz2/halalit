using Assets.Enums;
using UnityEngine;

class NewShootingLazerAsteriskEnemy : INewGameObject
{
    public GameObject Prefab;

    public NewShootingLazerAsteriskEnemy(GameObject prefab)
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