using Assets.Enums;
using UnityEngine;

class NewAstroid : INewGameObject
{
    public GameObject Prefab;

    public NewAstroid(GameObject prefab)
    {
        Prefab = prefab;
    }

    public int? GetEdgeWidthForInstantiation()
    {
        return null;
    }

    public GameObject GetPrefab()
    {
        return Prefab;
    }
}