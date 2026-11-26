using System;

namespace Assets.Models
{
    public interface IHealth
    {
        // Raised by a hit that hurts but does not kill, with the health left after the hit.
        event Action<int> Wounded;
        int GetCurrentHealth();
        void TryChangeHealth(int healthChange);
        // Shows a health bar that starts hidden (the Mimic's, until it wakes).
        void ShowHealthBar();
    }
}