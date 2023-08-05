using Assets.Utils;
using UnityEngine;

public class KnockbackWave : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _lifetime;
    
    private float _endOfLifeTime;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _endOfLifeTime = Time.time + _lifetime;
    }

    void Update()
    {
        transform.localScale = new Vector3(transform.localScale.x * _speed, transform.localScale.y * _speed, 1);

        if (IsShotDied())
        {
            Destroy(gameObject);
        }
    }

    private bool IsShotDied()
    {
        return Time.time >= _endOfLifeTime;
    }
}
