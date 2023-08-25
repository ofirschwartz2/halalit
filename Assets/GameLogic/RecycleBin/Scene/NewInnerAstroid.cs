using Assets.Enums;
using UnityEngine;

class NewInnerAstroid : INewGameObject
{
    public GameObject Prefab;

    public NewInnerAstroid(GameObject prefab)
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