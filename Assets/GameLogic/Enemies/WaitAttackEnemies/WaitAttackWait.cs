using UnityEngine;

public abstract class WaitAttackWait : MonoBehaviour
{
    public abstract void Wait();

    public abstract bool ShouldStopWaiting();

    public abstract void SetWaiting();
}