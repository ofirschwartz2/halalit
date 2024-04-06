using System;
using System.Collections.Generic;
using UnityEngine;

class DescreteTriggerHarmer : TriggerHarmer
{
    [SerializeReference]
    private IHarmer _harmer;
    [SerializeReference]
    private bool _singleHitter;

    private HashSet<GameObject> _harmedTargets;

    private void Awake()
    {
        InitHarmer();
        _harmedTargets = new HashSet<GameObject>();
    }

    public override int GetTriggerHarm(GameObject target)
    {
        try
        {
            if (_singleHitter)
            {
                if (!_harmedTargets.Contains(target))
                {
                    _harmedTargets.Add(target);
                    return _harmer.GetHarm();
                }
                return 0;
            }
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