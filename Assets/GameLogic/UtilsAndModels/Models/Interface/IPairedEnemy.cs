using UnityEngine;

namespace Assets.Models
{
    // An enemy that comes as a pair from one enemy bank entry (Tether Twins, #237). The spawn hole makes both bodies,
    // each with its own seed, then calls PairWith on the first with the second.
    public interface IPairedEnemy
    {
        void PairWith(GameObject partner);
    }
}
