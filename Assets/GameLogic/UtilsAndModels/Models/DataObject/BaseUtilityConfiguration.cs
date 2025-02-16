using UnityEngine;

namespace Assets.Models
{
    public abstract class BaseUtilityConfiguration : ScriptableObject
    {
        public bool IsActive { get; private set; }

        public void Activate()
        {
            IsActive = true;
        }

        public void Reset()
        {
            IsActive = false;
        }
    }
} 