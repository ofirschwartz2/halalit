using UnityEngine;

// Charge Lance (#236): an attack the weapon charges while the stick is held and fires on release. The weapon drives it,
// as it drives a ConsecutiveAttack. Only Release changes state; the weapon calls the other methods on the prefab.
public abstract class ChargedAttack : MonoBehaviour
{
    [SerializeField]
    private float _minHoldTime = 0.2f;
    [SerializeField]
    private float _chargeStepTime = 0.4f;
    [SerializeField]
    private int _chargeSteps = 3;
    [SerializeField]
    private float _chargingThrustFactor = 0.6f;
    [SerializeField]
    private Color _fullChargeColor = Color.white;

    public float GetMinHoldTime()
    {
        return _minHoldTime;
    }

    public float GetChargingThrustFactor()
    {
        return _chargingThrustFactor;
    }

    // Charge rises in steps: one per _chargeStepTime held, up to _chargeSteps.
    public int GetChargeLevel(float heldTime)
    {
        return Mathf.Min(_chargeSteps, Mathf.FloorToInt(heldTime / _chargeStepTime));
    }

    public Color GetChargeColor(Color baseColor, int chargeLevel)
    {
        return Color.Lerp(baseColor, _fullChargeColor, GetChargePortion(chargeLevel));
    }

    public abstract void Release(int chargeLevel);

    protected float GetChargePortion(int chargeLevel)
    {
        return (float)chargeLevel / _chargeSteps;
    }

    protected bool IsFullCharge(int chargeLevel)
    {
        return chargeLevel >= _chargeSteps;
    }
}
