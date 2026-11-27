using System;

namespace Assets.Models
{
    // An enemy driver whose attack wind-up a player hit can cancel (Interrupt, #238). Today: both enemy state machines.
    public interface IInterruptibleEnemyBehaviour
    {
        // True only in the last windupTime seconds before the attack starts.
        bool IsInInterruptWindow(float windupTime);
        // Cancels the wind-up. The enemy stands still for stunDuration, then starts over.
        void InterruptAttack(float stunDuration);
        // Raised when an attack completes normally.
        event Action AttackFinished;
    }
}
