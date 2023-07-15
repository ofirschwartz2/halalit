using Assets.Enums;
using UnityEngine;

class NewSinusEnemy : INewGameObject
{
    public GameObject Prefab;

    public NewSinusEnemy(GameObject prefab)
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