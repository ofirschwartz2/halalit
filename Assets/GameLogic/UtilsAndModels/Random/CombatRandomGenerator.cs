namespace Assets.Utils
{
    public static class CombatRandomGenerator
    {
        private static SeedfulRandomGenerator _seedfulRandomGenerator;

        public static void SetInitialSeed(int seed)
        {
            _seedfulRandomGenerator = new SeedfulRandomGenerator(seed);
        }

        public static SeedfulRandomGenerator GetSeedfulRandomGenerator()
        {
            return _seedfulRandomGenerator;
        }
    }
}
