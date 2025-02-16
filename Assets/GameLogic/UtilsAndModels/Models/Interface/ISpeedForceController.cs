using UnityEngine;

namespace Assets.Models
{
    public interface ISpeedForceController
    {
        float GetSpeedLimit();
        float GetForceMultiplier();
        void SetSpeedLimit(float value);
        void SetForceMultiplier(float value);
        Vector2 GetCurrentForce();
    }
} 