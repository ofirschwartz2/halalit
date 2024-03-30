using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomSeededNumbers : MonoBehaviour
{
    private List<float> _randomSeededNumbers;

    public void SetRandomSeededNumbers(List<float> randomSeededNumbers) 
    {
        _randomSeededNumbers = randomSeededNumbers;
    }

    public float PopRandomSeededNumber()
    {
        if (_randomSeededNumbers.Count == 0)
        {
            throw new System.Exception("RandomSeededNumbers list is empty");
        }

        var randomSeededNumber = _randomSeededNumbers.First();
        _randomSeededNumbers.RemoveAt(0);
        return randomSeededNumber;
    }
}
