using Assets.Enums;
using UnityEngine;

class NewZigZagEnemy : INewGameObject
{
    public GameObject Prefab;

    public NewZigZagEnemy(GameObject prefab)
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