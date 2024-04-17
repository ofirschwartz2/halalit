using Assets.Utils;
using UnityEngine;

public class SeedfulRandomGeneratorUser : MonoBehaviour, ISeedfulRandomGeneratorUser
{
    public SeedfulRandomGenerator _seedfulRandomGenerator { get; set; }

    public void SetInitialSeedfulRandomGenerator(int seed)
    {
        _seedfulRandomGenerator = new SeedfulRandomGenerator(seed);
    }
}
