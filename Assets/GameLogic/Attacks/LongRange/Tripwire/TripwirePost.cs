using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

public class TripwirePost : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private TripwireWire _wire;
    [SerializeField]
    private float _speed = 8f;
    [SerializeField]
    private float _flightTime = 0.4f;
    [SerializeField]
    private float _lifetime = 8f;
    [SerializeField]
    private int _maxPosts = 2;

    // Live posts, oldest first. The newest post owns the wire to the one before it.
    private static readonly List<TripwirePost> _posts = new();

    private float _plantTime;
    private float _endOfLifeTime;
    private bool _isPlanted;

    #region Init
    // Awake, not Start: the wire can touch in its first physics step, so it must know its posts by then.
    private void Awake()
    {
        RemoveOldestPosts();
        _wire.Connect(GetNewestPost(), transform);
        _posts.Add(this);
        _isPlanted = false;
    }

    private void RemoveOldestPosts()
    {
        while (_posts.Count > 0 && _posts.Count >= _maxPosts)
        {
            TripwirePost oldestPost = _posts[0];
            _posts.RemoveAt(0);
            Destroy(oldestPost.gameObject);
        }
    }

    private Transform GetNewestPost()
    {
        if (_posts.Count > 0)
        {
            return _posts[_posts.Count - 1].transform;
        }

        return null;
    }

    void Start()
    {
        _rigidBody.velocity = transform.up * _speed;
        _plantTime = Utils.GetEndOfLifeTime(_flightTime);
        _endOfLifeTime = Utils.GetEndOfLifeTime(_lifetime);
    }
    #endregion

    #region Destroy
    private void OnDestroy()
    {
        _posts.Remove(this);
    }
    #endregion

    #region Logic
    private void FixedUpdate()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            Destroy(gameObject);
            return;
        }

        if (ShouldPlant())
        {
            Plant();
        }
    }

    private void Plant()
    {
        _rigidBody.velocity = Vector2.zero;
        _isPlanted = true;
    }

    // The wire's collider shares this body, so its exits arrive here too, also when it is switched off
    // (Physics 2D callbacks on disable). Only a post that is itself outside the world is removed.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()) && !IsInside(other))
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Predicates
    private bool ShouldPlant()
    {
        return !_isPlanted && Time.time >= _plantTime;
    }

    private bool IsInside(Collider2D area)
    {
        return area.OverlapPoint(_rigidBody.position);
    }
    #endregion

#if UNITY_EDITOR
    internal float GetSpeed()
    {
        return _speed;
    }

    internal float GetFlightTime()
    {
        return _flightTime;
    }

    internal float GetLifeTime()
    {
        return _lifetime;
    }

    internal TripwireWire GetWire()
    {
        return _wire;
    }
#endif
}
