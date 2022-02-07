using Assets.Enums;
using UnityEngine;

class NewItem : MonoBehaviour, INewGameObject
{
    public GameObject Prefab;

    public NewItem(GameObject prefab)
    {
        Prefab = prefab;
    }

    public int? GetEdgeWidthForInstantiation()
    {
        return 4;
    }

    public GameObject GetPrefab()
    {
        return Prefab;
    }
}