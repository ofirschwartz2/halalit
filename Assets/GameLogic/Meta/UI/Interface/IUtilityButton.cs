using UnityEngine;
using UnityEngine.Events;

namespace Meta.UI
{
    public interface IUtilityButton
    {
        void SetUtilityText(string text);
        void ClearUtility();
        void SetInteractable(bool interactable);
        void AddClickListener(UnityAction action);
        void RemoveClickListener(UnityAction action);
    }
} 