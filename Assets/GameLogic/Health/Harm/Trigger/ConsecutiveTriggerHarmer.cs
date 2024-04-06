using UnityEngine;

class ConsecutiveTriggerHarmer : TriggerHarmer
{
    [SerializeReference]
    private IHarmer _harmer;

    private float _nextHarmTime;

    private void Start()
    {
        _nextHarmTime = Time.time;
        InitHarmer();
    }

    private void InitHarmer()
    {
        AttackBehaviour harmer = GetComponent<AttackBehaviour>();
        if (harmer != null)
        {
            _harmer = harmer;
        }
    }

    public override int GetTriggerHarm(GameObject target)
    {
        if (Time.time >= _nextHarmTime)
        {
            _nextHarmTime += ((AttackBehaviour)_harmer).AttackStats.Rate;
            return _harmer.GetHarm();
        }

        return 0;
    }
}
