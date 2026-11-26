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
    // Seconds before the same target can be harmed again (Tripwire, #213). 0 = off.
    [SerializeField]
    private float _sameTargetCooldown;

    private HashSet<GameObject> _harmedTargets;
    private List<string> _ignoredTargetTagsDescriptions;
    private Dictionary<GameObject, float> _nextHarmTimes;

    private void Awake()
    {
        InitHarmer();
        _harmedTargets = new HashSet<GameObject>();
        _ignoredTargetTagsDescriptions = _ignoredTargetTags.Select(tag => Utils.GetDescription(tag)).ToList();
        _nextHarmTimes = new Dictionary<GameObject, float>();
    }

    public override int GetTriggerHarm(GameObject target)
    {
        if (IsIgnoredTarget(target))
        {
            return 0;
        }

        if (HasSameTargetCooldown())
        {
            if (IsCoolingDownFor(target))
            {
                return 0;
            }

            _nextHarmTimes[target] = Time.time + _sameTargetCooldown;
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

    private bool HasSameTargetCooldown()
    {
        return _sameTargetCooldown > 0;
    }

    private bool IsCoolingDownFor(GameObject target)
    {
        return _nextHarmTimes.TryGetValue(target, out float nextHarmTime) && Time.time < nextHarmTime;
    }
    #endregion

#if UNITY_EDITOR
    internal float GetSameTargetCooldown()
    {
        return _sameTargetCooldown;
    }
#endif
}