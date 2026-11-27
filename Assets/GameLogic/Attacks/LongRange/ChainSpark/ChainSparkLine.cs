using Assets.Utils;
using UnityEngine;

// The short flash of a Chain Spark: one line through the first target and every target it jumped to.
public class ChainSparkLine : MonoBehaviour
{
    [SerializeField]
    private LineRenderer _line;
    [SerializeField]
    private float _lifetime = 0.15f;

    private float _endOfLifeTime;

    private void Awake()
    {
        _endOfLifeTime = Utils.GetEndOfLifeTime(_lifetime);
        _line.useWorldSpace = true;
    }

    public void Show(Vector3[] points)
    {
        _line.positionCount = points.Length;
        _line.SetPositions(points);
    }

    private void FixedUpdate()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    internal float GetLifeTime()
    {
        return _lifetime;
    }
#endif
}
