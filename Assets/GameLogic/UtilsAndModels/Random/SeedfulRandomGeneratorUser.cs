using Assets.Utils;
using UnityEngine;

public class SeedfulRandomGeneratorUser : MonoBehaviour, ISeedfulRandomGeneratorUser
{
    public SeedfulRandomGenerator _seedfulRandomGenerator { get; set; }

    public void SetInitialSeedfulRandomGenerator(int seed)
    {
        if (_seedfulRandomGenerator != null) 
        {
            throw new System.Exception("SeedfulRandomGenerator is already set");
        }
        _seedfulRandomGenerator = new SeedfulRandomGenerator(seed);
    }
}
