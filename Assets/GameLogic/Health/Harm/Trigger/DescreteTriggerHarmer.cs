using UnityEngine;

class DescreteTriggerHarmer : TriggerHarmer
{
    [SerializeField]
    private int _triggerHarm;

    public override int GetTriggerHarm()
    {
        return _triggerHarm;
    }
}