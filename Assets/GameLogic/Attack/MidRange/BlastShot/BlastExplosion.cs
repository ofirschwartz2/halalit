using Assets.Utils;
using System.Diagnostics.Contracts;
using UnityEngine;

// TODO (refactor): the stats (damage) of a shot (when it's collide with enemy) needs to be on the shot script
// TODO (refactor): move all shots out of the Gun. Enemies also shoot now.
public class BlastExplosion : MonoBehaviour 
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
        if (Utils.ShouldDie(_blastCommon.GetEndOfLifeTime()))
        {
            Die();
        }
        else 
        {
            Blasting();
        }
    }

    private void Blasting()
    {
        transform.localScale = originalScale * (1 + _blastCommon.GetBlastCommonMultiplier());
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}