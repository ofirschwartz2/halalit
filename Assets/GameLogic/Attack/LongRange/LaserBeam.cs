using Assets.Utils;

using UnityEngine;

public class LaserBeam : ConsecutiveAttack 
{
    [SerializeField]
    private LineRenderer _beamLine;
    [SerializeField]
    private LayerMask _beamInteractionLayers;
    [SerializeField]
    private BoxCollider2D _boxCollider;
    [SerializeField]
    private GameObject _startVfxBeamFlash;
    [SerializeField]
    private GameObject _startVfxParticles;
    [SerializeField]
    private GameObject _endVfxBeamFlash;
    [SerializeField]
    private GameObject _endVfxParticles;
    [SerializeField]
    private float _beamSize;
    [SerializeField]
    private float _beamSpeed;

    private bool _isBeingShoot;
    private float _distanceFromStartPosition;
    private Vector2 _target;
    private Vector2 _startPosition;
    private Vector2 _startPositionAtStopShoot;
    private Quaternion _lastRotation;

    private const float PENETRATION_COLLIDER_EXTENTION = 0.2f;

    private void Start()
    {
        _isBeingShoot = true;
        _distanceFromStartPosition = 0;
        _beamLine.startWidth = _beamSize;
        _beamLine.endWidth = _beamSize;
    }

    private void FixedUpdate()
    {
        if (!_isBeingShoot)
        {
            FinalizeLaserBeam();
        }
    }

    private void FinalizeLaserBeam()
    {
        SetNewStartPosition();
        UpdateConsecitiveAttack(_startPosition, _lastRotation);
    }

    private void SetNewStartPosition()
    {
        Vector2 direction = (_target - _startPosition).normalized;
        float maxBeamSize = Utils.GetDistanceBetweenTwoPoints(_startPosition, _target);

        if (maxBeamSize - _beamSpeed * Time.deltaTime < 0)
        {
            Destroy(gameObject);
        }
        else
        {
            Vector2 startPositionMovement = _beamSpeed * Time.deltaTime * direction;
            _startPosition += startPositionMovement;
            _distanceFromStartPosition -= startPositionMovement.magnitude;
            _startVfxParticles.SetActive(false);
        }
    }

    public override void StartConsecitiveAttack(Vector2 startPosition, Quaternion rotation)
    {   
        _beamLine.enabled = true;
        _boxCollider.enabled = true;
        _startVfxBeamFlash.SetActive(true);
        _startVfxParticles.SetActive(true);
        _endVfxBeamFlash.SetActive(true);

        UpdateConsecitiveAttack(startPosition, rotation);
    }

    public override void UpdateConsecitiveAttack(Vector2 newStartPosition, Quaternion newRotation)
    {
        if (_isBeingShoot)
        {
            transform.SetPositionAndRotation(newStartPosition, newRotation);
        } 
        
        _lastRotation = transform.rotation;
        _target = Physics2D.Raycast(newStartPosition, newRotation * Vector2.up, Mathf.Infinity, _beamInteractionLayers).point;
        _startPosition = newStartPosition;

        SetNewBeamDistanceFromStartPosition();

        Vector2 newEndPosition = GetBeamEndPosition();

        SetBeamLine(newEndPosition);
        SetBeamCollider(newEndPosition);
    }

    private void SetNewBeamDistanceFromStartPosition()
    {
        float maxBeamSize = Utils.GetDistanceBetweenTwoPoints(_startPosition, _target);

        if (_distanceFromStartPosition + _beamSpeed * Time.deltaTime > maxBeamSize) 
        {
            _distanceFromStartPosition = maxBeamSize;
            _endVfxParticles.SetActive(true);
        }
        else
        {
            _distanceFromStartPosition += _beamSpeed * Time.deltaTime;
            _endVfxParticles.SetActive(false);
        }
    }

    private Vector2 GetBeamEndPosition()
    {
        Vector2 direction = (_target - _startPosition).normalized;
        return _startPosition + direction * _distanceFromStartPosition;
    }

    private void SetBeamLine(Vector2 endPosition)
    {
        _beamLine.SetPosition(0, _startPosition);
        _beamLine.SetPosition(1, endPosition);

        _startVfxBeamFlash.transform.position = _startPosition;
        _startVfxParticles.transform.position = _startPosition;
        _endVfxBeamFlash.transform.position = endPosition;
        _endVfxParticles.transform.position = endPosition;
    }
     
    private void SetBeamCollider(Vector2 endPosition)
    {
        float beamSize = Utils.GetDistanceBetweenTwoPoints(_startPosition, endPosition);

        _boxCollider.size = new(_beamSize, beamSize + PENETRATION_COLLIDER_EXTENTION);

        if (_isBeingShoot)
        {
            _boxCollider.offset = new(0, beamSize / 2 + PENETRATION_COLLIDER_EXTENTION / 2);
        }
        else
        {
            _boxCollider.offset = new(0, beamSize / 2 + Utils.GetDistanceBetweenTwoPoints(_startPosition, _startPositionAtStopShoot) + PENETRATION_COLLIDER_EXTENTION / 2);
        }
    }
     
    public override void StopConsecitiveAttack()
    {
        _isBeingShoot = false;
        _startPositionAtStopShoot = new(_startPosition.x, _startPosition.y);
    }
}