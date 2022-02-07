using Assets.Enums;
using UnityEngine;

interface INewGameObject
{    
    int? GetEdgeWidthForInstantiation();
    GameObject GetPrefab();
}