using UnityEngine;

class ConsecutiveTriggerHarmer : TriggerHarmer
{
    [SerializeReference]
    private IHarmer _harmer;
    [SerializeField]
    private float _harmInterval;

    private float _nextHarmTime;

    private void Start()
    {
        _nextHarmTime = 0;
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

    public override int GetTriggerHarm()
    {
        if (Time.time >= _nextHarmTime)
        {
            _nextHarmTime = Time.time + _harmInterval;
            return _harmer.GetHarm();
        }

        return 0;
    }
}
