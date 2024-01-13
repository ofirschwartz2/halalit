using System;
using UnityEngine;

class DescreteTriggerHarmer : TriggerHarmer
{
    [SerializeReference]
    private IHarmer _harmer;

    private void Awake()
    {
        InitHarmer();
    }

    public override int GetTriggerHarm()
    {
        try
        {
            return _harmer.GetHarm();
        }
        catch (NullReferenceException)
        {
            Debug.LogError("this _harmer is not yet innitialized but is used anyway");
            return 0;
        }
    }

    private void InitHarmer()
    {
        AttackBehaviour harmer = GetComponent<AttackBehaviour>();
        if (harmer != null)
        {
            _harmer = harmer;
        }
    }
}