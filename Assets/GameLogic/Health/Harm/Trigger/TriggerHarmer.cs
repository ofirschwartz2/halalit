using UnityEngine;

class TriggerHarmer : MonoBehaviour
{
    [SerializeField]
    private int _triggerHarm;

    public int GetTriggerHarm()
    {
        return _triggerHarm;
    }
}