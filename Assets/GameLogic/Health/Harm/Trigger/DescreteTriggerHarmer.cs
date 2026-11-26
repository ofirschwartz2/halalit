using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class DescreteTriggerHarmer : TriggerHarmer
{
    [SerializeReference]
    private IHarmer _harmer;
    [SerializeReference]
    private bool _singleHitter;
    [SerializeField]
    private List<Tag> _ignoredTargetTags = new();

    private HashSet<GameObject> _harmedTargets;
    private List<string> _ignoredTargetTagsDescriptions;

    private void Awake()
    {
        InitHarmer();
        _harmedTargets = new HashSet<GameObject>();
        _ignoredTargetTagsDescriptions = _ignoredTargetTags.Select(tag => Utils.GetDescription(tag)).ToList();
    }

    public override int GetTriggerHarm(GameObject target)
    {
        if (IsIgnoredTarget(target))
        {
            return 0;
        }

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

    #region Predicates
    private bool IsIgnoredTarget(GameObject target)
    {
        foreach (string ignoredTag in _ignoredTargetTagsDescriptions)
        {
            if (target.CompareTag(ignoredTag))
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}