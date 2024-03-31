using Assets.Enums;
using System.Collections.Generic;
using UnityEngine;

public class PotentialValuableDrops : MonoBehaviour
{
    [SerializeField]
    private List<KeyValuePair<ValuableName, float>> _potentialValuableDrops;

    public List<KeyValuePair<ValuableName, float>> GetValuablesToChances()
    {
        return _potentialValuableDrops;
    }
}
