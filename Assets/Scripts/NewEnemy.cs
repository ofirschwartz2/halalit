using Assets.Enums;
using UnityEngine;

class NewEnemy : INewGameObject
{
    public GameObject Prefab;

    public NewEnemy(GameObject prefab)
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