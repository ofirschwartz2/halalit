using Assets.Enums;
using UnityEngine;

interface INewGameObject
{
    //GameObject Prefab {get;}
    
    int? GetEdgeWidthForInstantiation();
    GameObject GetPrefab();
    GameObjectType GetGameObjectType();
}