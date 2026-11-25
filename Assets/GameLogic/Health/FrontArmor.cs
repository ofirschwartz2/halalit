using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FrontArmor : MonoBehaviour
{
    [SerializeField]
    private float _arcDegrees = 150f;
    [SerializeField]
    private List<Tag> _blockedTags = new() { Tag.SHOT };

    private List<string> _blockedTagsDescriptions;

    private void Awake()
    {
        _blockedTagsDescriptions = _blockedTags.Select(tag => Utils.GetDescription(tag)).ToList();
    }

    public bool IsBlocking(GameObject harmer)
    {
        return IsBlockedTag(harmer) && IsInsideArc(harmer.transform.position);
    }

    #region Predicates
    private bool IsBlockedTag(GameObject harmer)
    {
        foreach (string blockedTag in _blockedTagsDescriptions)
        {
            if (harmer.CompareTag(blockedTag))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsInsideArc(Vector2 hitFrom)
    {
        return Vector2.Angle((Vector2)transform.up, hitFrom - (Vector2)transform.position) <= _arcDegrees / 2f;
    }
    #endregion
}