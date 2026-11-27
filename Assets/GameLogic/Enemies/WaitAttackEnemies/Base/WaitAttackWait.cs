using UnityEngine;

public abstract class WaitAttackWait : MonoBehaviour
{
    public abstract void Wait();

    public abstract bool ShouldStopWaiting();

    public abstract void SetWaiting();

    // Interrupt (#238): true in the last seconds of the wait before a charge. A wait that keeps this default is never interrupted.
    public virtual bool IsWindingUp(float seconds)
    {
        return false;
    }
}