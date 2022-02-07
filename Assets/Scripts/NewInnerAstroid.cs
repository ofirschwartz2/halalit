using Assets.Enums;
using UnityEngine;

class NewInnerAstroid : MonoBehaviour, INewGameObject
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

    public GameObjectType GetGameObjectType()
    {
        return GameObjectType.INNER_ASTROID;
    }
}