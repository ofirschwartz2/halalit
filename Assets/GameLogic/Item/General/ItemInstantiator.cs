using Assets.Enums;
using System.Collections.Generic;
using UnityEngine;

public class ItemInstantiator : MonoBehaviour
{
    [SerializeField]
    private List<KeyValuePair<UpgradeName, GameObject>> _upgrades;

    private Dictionary<UpgradeName, GameObject> _upgradesDictionary;

    void Start()
    {
        InitItemDictionaries();

        InstantiateUpgrade(UpgradeName.FIRE_RATE, new Vector3(4, 4, 0)); // TODO: remove usage example
    }

    private void InitItemDictionaries()
    {
        _upgradesDictionary = new();

        foreach (KeyValuePair<UpgradeName, GameObject> upgrade in _upgrades)
        {
            _upgradesDictionary.Add(upgrade.Key, upgrade.Value);
        }
    }

    public void InstantiateUpgrade(UpgradeName upgradeName, Vector3 position)
    {
        GameObject upgrade = Instantiate(_upgradesDictionary[upgradeName], position, Quaternion.identity);

        upgrade.transform.SetParent(transform.parent);
    }
}
