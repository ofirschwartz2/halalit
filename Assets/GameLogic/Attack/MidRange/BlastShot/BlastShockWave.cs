using Assets.Utils;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UIElements;

// TODO (refactor): the stats (damage) of a shot (when it's collide with enemy) needs to be on the shot script
// TODO (refactor): move all shots out of the Gun. Enemies also shoot now.
public class BlastShockWave : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private BlastCommon _blastCommon;

    private Vector3 originalScale;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        originalScale = transform.localScale;
    }

    private void Update()
    {
        var newLocalScale = originalScale * (1 + _blastCommon.GetBlastCommonMultiplier());

        if (IsContracting(newLocalScale.magnitude, transform.localScale.magnitude))
        {
            Destroy(gameObject);
        }

        transform.localScale = newLocalScale;
    }

    private bool IsContracting(float newlocalScale, float localScale)
    {
        return newlocalScale  < localScale;
    }
}