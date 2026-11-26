using Assets.Enums;

namespace Assets.Models
{
    public interface IScoreWallet
    {
        bool TrySpend(ValuableName valuableName);
    }
}