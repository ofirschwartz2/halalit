using System.Collections;
using UnityEngine;

public class Fader : MonoBehaviour
{
    [SerializeField]
    private bool _fadeIn;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private float _fadeSpeed;

    private void Start()
    {
        if (_fadeIn)
        {
            ChangeOpacity(0f);
            StartFadeIn();
        }
    }

    private void ChangeOpacity(float newOpacity)
    {
        Color color = _spriteRenderer.material.color;
        color.a = newOpacity;
        _spriteRenderer.material.color = color;
    }

    private void StartFadeIn()
    {
        StartCoroutine(nameof(FaddingIn));
    }

    private IEnumerator FaddingIn() 
    { 
        for (float f = _fadeSpeed; f <= 1; f += _fadeSpeed)
        {
            ChangeOpacity(f);
            yield return new WaitForSeconds(_fadeSpeed);
        }
    }

    public void StartFadeOut()
    {
        StartCoroutine(nameof(FaddingOut));
    }

    private IEnumerator FaddingOut()
    {
        for (float f = 1f; f >= -_fadeSpeed; f -= _fadeSpeed)
        {
            ChangeOpacity(f);
            yield return new WaitForSeconds(_fadeSpeed);
        }
    }
}