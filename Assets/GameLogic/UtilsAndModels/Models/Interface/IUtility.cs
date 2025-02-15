using UnityEngine;

namespace Assets.Models
{
    public interface IUtility
    {
        void Activate(GameObject target);
        void Deactivate();
        bool CanActivate();
        bool IsActive { get; }
    }
} 