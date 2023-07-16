using Assets.Enums;
using UnityEngine;

class NewFollowingEnemy : INewGameObject
{
    public GameObject Prefab;

    public NewFollowingEnemy(GameObject prefab)
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