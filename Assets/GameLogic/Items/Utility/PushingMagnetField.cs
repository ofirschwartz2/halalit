using Assets.Models;
using System.Collections;
using UnityEngine;

public class PushingMagnetField : MonoBehaviour
{
    private PushingMagnetConfiguration _config;
    private float _endTime;
    private SpriteRenderer _circleRenderer;
    private bool _fading = false;
    private static readonly int EnemyLayer = 8; // Adjust if needed
    private static readonly float MinDistance = 0.2f;
    private static readonly float FadeDuration = 0.3f;

    public void Initialize(PushingMagnetConfiguration config)
    {
        _config = config;
        _endTime = Time.time + _config.Duration;
        _circleRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_circleRenderer != null)
        {
            _circleRenderer.color = new Color(1, 1, 1, 0.2f); // 80% transparent
            _circleRenderer.transform.localScale = Vector3.one * _config.Radius * 2f;
        }
    }

    private void FixedUpdate()
    {
        if (_fading || _config == null) return;
        if (Time.time >= _endTime)
        {
            FadeAndDestroy();
            return;
        }
        ApplyMagneticForce();
    }

    private void ApplyMagneticForce()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _config.Radius);
        foreach (var hit in hits)
        {
            if (hit.attachedRigidbody == null) continue;
            if (!IsEnemy(hit.gameObject)) continue;
            Vector2 dir = (hit.transform.position - transform.position);
            float dist = Mathf.Max(dir.magnitude, MinDistance);
            dir.Normalize();
            float force = _config.ForceStrength / Mathf.Pow(dist, 3);
            if (_config.ForceCurve != null)
                force *= _config.ForceCurve.Evaluate(1f - Mathf.Clamp01(dist / _config.Radius));
            hit.attachedRigidbody.AddForce(dir * force, ForceMode2D.Force);
        }
    }

    private bool IsEnemy(GameObject obj)
    {
        // You may want to check tag or layer here
        return obj.CompareTag("Enemy");
    }

    public void FadeAndDestroy()
    {
        if (_fading) return;
        _fading = true;
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        if (_circleRenderer != null)
        {
            float startAlpha = _circleRenderer.color.a;
            float t = 0f;
            while (t < FadeDuration)
            {
                float alpha = Mathf.Lerp(startAlpha, 0f, t / FadeDuration);
                _circleRenderer.color = new Color(1, 1, 1, alpha);
                t += Time.deltaTime;
                yield return null;
            }
            _circleRenderer.color = new Color(1, 1, 1, 0f);
        }
        Destroy(gameObject);
    }
} 