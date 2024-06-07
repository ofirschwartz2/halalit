using Assets.Utils;
using UnityEngine;

class UtilsGameObjectsInitializer : MonoBehaviour
{
    [SerializeField]
    private GameObject _internalWorld;
    [SerializeField]
    private GameObject _externalSafeIsland;

    private void Awake()
    {
        Utils.SetGameObjects(_internalWorld, _externalSafeIsland);
    }
}
