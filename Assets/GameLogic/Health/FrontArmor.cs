using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FrontArmor : MonoBehaviour
{
    [SerializeField]
    private float _arcDegrees = 150f;
    [SerializeField]
    private List<Tag> _blockedTags = new() { Tag.SHOT };
    [SerializeField]
    private bool _reflects;
    [SerializeField]
    private GameObject _reflectedShotPrefab;
    [SerializeField]
    private float _reflectSpawnDistance = 0.2f;

    private List<string> _blockedTagsDescriptions;
    private Collider2D _collider;
    private Transform _projectiles;

    private void Awake()
    {
        _blockedTagsDescriptions = _blockedTags.Select(tag => Utils.GetDescription(tag)).ToList();
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (_reflects)
        {
            var projectiles = GameObject.Find(Constants.PROJECTILES_GAME_OBJECT_NAME);
            _projectiles = projectiles != null ? projectiles.transform : null;
        }
    }

    public bool IsBlocking(GameObject harmer)
    {
        return IsBlockedTag(harmer) && IsInsideArc(harmer.transform.position) && !(_reflects && IsMelee(harmer));
    }

    #region Reflect
    // Mirror Shield (#200): a blocked single-hit shot is destroyed and an enemy bullet flies back out of the plate.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ShouldReflect(other.gameObject))
        {
            Reflect(other);
        }
    }

    private void Reflect(Collider2D shot)
    {
        Vector2 direction = GetReflectedDirection(shot);
        Vector2 spawnPosition = (Vector2)transform.position + direction * (_collider.bounds.extents.magnitude + _reflectSpawnDistance);

        Destroy(shot.gameObject);
        var reflectedShot = Instantiate(_reflectedShotPrefab, spawnPosition, Quaternion.LookRotation(Vector3.forward, direction), _projectiles);
        ShotOwner.Mark(reflectedShot, transform);
    }

    // A shot flying into the plate bounces off it like a mirror. A shot with no such velocity (moved by its transform,
    // or already turned) goes straight out from the plate's centre through the point where it hit.
    private Vector2 GetReflectedDirection(Collider2D shot)
    {
        Vector2 shotVelocity = shot.attachedRigidbody != null ? shot.attachedRigidbody.velocity : Vector2.zero;

        if (Vector2.Dot(shotVelocity, transform.up) < 0)
        {
            return Vector2.Reflect(shotVelocity.normalized, transform.up);
        }

        return Utils.GetDirectionFromCollision(shot.transform.position, transform.position);
    }
    #endregion

    #region Predicates
    private bool ShouldReflect(GameObject harmer)
    {
        return _reflects && IsBlocking(harmer) && IsDescrete(harmer);
    }

    private bool IsMelee(GameObject harmer)
    {
        var attackBehaviour = harmer.GetComponent<AttackBehaviour>();
        return attackBehaviour != null && attackBehaviour.IsMelee;
    }

    private bool IsDescrete(GameObject harmer)
    {
        var attackBehaviour = harmer.GetComponent<AttackBehaviour>();
        return attackBehaviour != null && attackBehaviour.ShotType == AttackShotType.DESCRETE;
    }

    private bool IsBlockedTag(GameObject harmer)
    {
        foreach (string blockedTag in _blockedTagsDescriptions)
        {
            if (harmer.CompareTag(blockedTag))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsInsideArc(Vector2 hitFrom)
    {
        return Vector2.Angle((Vector2)transform.up, hitFrom - (Vector2)transform.position) <= _arcDegrees / 2f;
    }
    #endregion

#if UNITY_EDITOR
    internal bool IsReflecting()
    {
        return _reflects;
    }

    internal float GetArcDegrees()
    {
        return _arcDegrees;
    }
#endif
}