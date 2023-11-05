using System.Collections;
using UnityEngine;
using UnityEngine.U2D;

public class EnemySpriteLoader : MonoBehaviour
{
    [SerializeField]
    private SpriteAtlas spriteAtlas;

    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;
    
    void Start()
    {
        Setsprite();
        SetPoligonCollider2D();
    }

    private void Setsprite()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteAtlas != null && spriteAtlas.spriteCount > 0)
        {
            Sprite[] spritesInAtlas = new Sprite[spriteAtlas.spriteCount];
            spriteAtlas.GetSprites(spritesInAtlas);

            int randomIndex = Random.Range(0, spritesInAtlas.Length);
            Sprite randomSprite = spritesInAtlas[randomIndex];
            spriteRenderer.sprite = randomSprite;
        }
        else
        {
            Debug.LogError("Sprites array is empty. Please assign sprites to the script in the Unity Editor.");
        }
    }

    private void SetPoligonCollider2D()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();

        if (polygonCollider != null)
        {
            polygonCollider.SetPath(0, spriteRenderer.sprite.vertices);
        }
        else
        {
            Debug.LogError("PolygonCollider2D component is missing. Please add a PolygonCollider2D component to the GameObject.");
        }
    }
}

