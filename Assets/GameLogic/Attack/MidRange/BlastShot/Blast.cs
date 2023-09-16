using Assets.Enums;
using Assets.Utils;
using UnityEngine;

// TODO (refactor): the stats (damage) of a shot (when it's collide with enemy) needs to be on the shot script
// TODO (refactor): move all shots out of the Gun. Enemies also shoot now.
public class Blast : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _blastMaxSize;
    [SerializeField]
    private float _blastTime;
    [SerializeField]
    private GameObject blastPrefab;
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    private AnimationCurve blastCurve;

    private float _endOfLifeTime;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _endOfLifeTime = Time.time + _lifetime;
    }

    private void Update()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            Die();
        }
        else 
        {
            Blast();
        }
    }

    private void Blast()
    {
        transform.localScale = transform.localScale * _blastMaxSize * blastCurv.Evaluate();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

}