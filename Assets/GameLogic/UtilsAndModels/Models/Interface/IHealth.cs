using System;
using UnityEngine;

namespace Assets.Models
{
    public interface IHealth
    {
        // Raised by a hit that hurts but does not kill, with the health left after the hit.
        event Action<int> Wounded;
        // Raised by a shot or laser that hurts but does not kill, with the object that hit (Grudge, #216).
        event Action<GameObject> HarmedBy;
        int GetCurrentHealth();
        // The current max health: a heal through TryChangeHealth never goes above it (Mender, #230).
        int GetMaxHealth();
        void TryChangeHealth(int healthChange);
        // Shows a health bar that starts hidden (the Mimic's, until it wakes).
        void ShowHealthBar();
    }
}