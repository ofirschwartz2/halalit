using Assets.Utils;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
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
        if (IsShotDied())
        {
            Destroy(gameObject);
            Destroy(transform.parent.gameObject);
        }
    }

    private bool IsShotDied()
    {
        return Time.time >= _endOfLifeTime;
    }
}
