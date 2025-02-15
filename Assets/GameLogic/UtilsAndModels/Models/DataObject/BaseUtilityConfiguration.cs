using UnityEngine;

namespace Assets.Models
{
    public abstract class BaseUtilityConfiguration : ScriptableObject
    {
        [SerializeField] protected float duration;
        
        public float Duration => duration;
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