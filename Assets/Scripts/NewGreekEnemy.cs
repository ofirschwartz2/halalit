using Assets.Enums;
using UnityEngine;

class NewGreekEnemy : INewGameObject
{
    public GameObject Prefab;

    public NewGreekEnemy(GameObject prefab)
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