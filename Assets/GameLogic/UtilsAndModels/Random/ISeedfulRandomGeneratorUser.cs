using Assets.Utils;

public interface ISeedfulRandomGeneratorUser
{
    SeedfulRandomGenerator _seedfulRandomGenerator { get; set; }
    void SetInitialSeedfulRandomGenerator(int seed);
}
