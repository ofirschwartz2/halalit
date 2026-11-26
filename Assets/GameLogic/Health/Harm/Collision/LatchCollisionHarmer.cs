using Assets.Models;
using UnityEngine;

// Leech (#215): its touch does no harm while it can latch or is latched. A Leech that cannot latch (both slots taken, or stunned)
// harms as a FixedCollisionHarmer.
internal class LatchCollisionHarmer : FixedCollisionHarmer
{
    private ILatcher _latcher;

    void Awake()
    {
        _latcher = GetComponent<ILatcher>();
    }

    public override int GetCollisionHarm()
    {
        if (_latcher != null && _latcher.IsHarmlessTouch())
        {
            return 0;
        }

        return base.GetCollisionHarm();
    }
}
