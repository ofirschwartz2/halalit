using UnityEngine;

class ConsecutiveTriggerHarmer : TriggerHarmer
{
    [SerializeField]
    private int _triggerHarm;
    [SerializeField]
    private float _harmInterval;

    private float _nextHarmTime;

    private void Start()
    {
        _nextHarmTime = 0;
    }

    public override int GetTriggerHarm()
    {
        if (Time.time >= _nextHarmTime)
        {
            _nextHarmTime = Time.time + _harmInterval;
            return _triggerHarm;
        }

        return 0;
    }
}
