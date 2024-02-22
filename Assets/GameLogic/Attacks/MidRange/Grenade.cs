using Assets.Enums;
using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("Tests")]
#endif

public class Grenade : MonoBehaviour 
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _throwForce;
    [SerializeField]
    private float _lifetime;
    [SerializeField]
    private GameObject blastPrefab;

    private float _endOfLifeTime;

    void Start()
    {
        _endOfLifeTime = Utils.GetEndOfLifeTime(_lifetime);
        _rigidBody.AddForce(transform.up * _throwForce, ForceMode2D.Impulse);
    }

    private void FixedUpdate()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            InitiateBlast();
        }
    }

    private void InitiateBlast()
    {
        GameObject blast = Instantiate(blastPrefab, transform.position, transform.rotation, transform.parent);
        blast.GetComponent<AttackBehaviour>().Copy(GetComponent<AttackBehaviour>());
        Destroy(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription())) 
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