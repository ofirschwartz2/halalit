using System.Collections.Generic;
using UnityEngine;

public class PotentialValuableDrops : MonoBehaviour
{
    [SerializeField]
    private List<KeyValuePair<GameObject, float>> _potentialValuableDrops;

    public List<KeyValuePair<GameObject, float>> GetValuablesWithChances()
    {
        return _potentialValuableDrops;
    }
}
