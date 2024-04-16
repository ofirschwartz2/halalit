using Assets.Enums;
using Assets.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

public class Health : MonoBehaviour
{
    [SerializeField]
    private GameObject _healthBar;
    [SerializeField]
    private Sprite _healthBarIcon;
    [SerializeField]
    private int _currentMaxHealth;
    [SerializeField]
    private int _finalMaxHealth;
    [SerializeField]
    private List<Tag> _harmers;
    [SerializeField]
    private List<Tag> _contributors;

    private int _health;
    private Slider _healthBarBorder;
    private Slider _healthBarFill;
    private List<string> _harmersDescriptions;
    private List<string> _contributorsDescriptions;
    private Canvas _privateCanvas;
    private bool _died;

    #region Init
    private void Awake()
    {
        SetEventListeners();
        _harmersDescriptions = new List<string>();
        _contributorsDescriptions = new List<string>();
        _died = false;
    }

    private void SetEventListeners()
    {
        ItemEvent.PlayerUpgradePickUp += UpgradeCurrentMaxHealth;
    }

    private void Start()
    {
        InitiateDynamicHealthBar();
        SetMembersValues();
        SetBarIcon();
    }

    private void InitiateDynamicHealthBar()
    {
        if (_healthBar == null)
        {
            AddPrivateCanvasComponent();
            CreateDynamicHealthBar();
        }
    }

    private void CreateDynamicHealthBar()
    {
        GameObject dynamicBarPrefab = (GameObject)Resources.Load(Constants.RESOURCE_DYNAMIC_BAR_PREFAB);
        _healthBar = Instantiate(dynamicBarPrefab, _privateCanvas.transform, false);
        SetHealthBarInvisible();
        StartCoroutine(SetHealthBarVisible(1f));
    }

    private void SetHealthBarInvisible()
    {
        _healthBar.SetActive(false);
    }

    private IEnumerator SetHealthBarVisible(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        _healthBar.SetActive(true);

        SetHealthBarScale();
        SetHealthBarPosition();
    }

    private void SetHealthBarScale()
    {
        _healthBar.transform.localScale = new(Constants.HEALH_BAR_SCALE, Constants.HEALH_BAR_SCALE, Constants.HEALH_BAR_SCALE);
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        RectTransform healthBarRect = _healthBar.GetComponent<RectTransform>();
        healthBarRect.sizeDelta = new Vector2(spriteRenderer.sprite.pivot.x, healthBarRect.rect.height);
    }

    private void SetHealthBarPosition()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        float yOffset = (spriteRenderer.bounds.size.y / 2f) + Constants.HEALTH_BAR_Y_OFFSET_ADITION;
        _healthBar.transform.position = new(_healthBar.transform.position.x, _healthBar.transform.position.y + yOffset);
    }

    private void AddPrivateCanvasComponent()
    {
        _privateCanvas = gameObject.AddComponent<Canvas>();
        _privateCanvas.renderMode = RenderMode.WorldSpace;
    }

    private void SetMembersValues()
    {
        _health = _currentMaxHealth;

        Slider[] sliderComponents = _healthBar.GetComponentsInChildren<Slider>();

        _healthBarFill = sliderComponents.Where(sliderComponent => sliderComponent.gameObject.CompareTag(Tag.BAR_FILL.GetDescription())).ToList()[0];
        _healthBarFill.maxValue = _finalMaxHealth;
        _healthBarFill.value = _currentMaxHealth;

        _healthBarBorder = sliderComponents.Where(sliderComponent => sliderComponent.gameObject.CompareTag(Tag.BAR_BORDEDR.GetDescription())).ToList()[0];
        _healthBarBorder.maxValue = _finalMaxHealth;
        _healthBarBorder.value = _currentMaxHealth;

        _harmersDescriptions = _harmers.Select(tag => Utils.GetDescription(tag)).ToList();

        _contributorsDescriptions = _contributors.Select(tag => Utils.GetDescription(tag)).ToList();
    }

    private void SetBarIcon()
    {
        if (_healthBarIcon != null)
        {
            _healthBar.GetComponentsInChildren<Image>().Where(image => image.gameObject.CompareTag(Tag.BAR_ICON.GetDescription())).ToList()[0].sprite = _healthBarIcon;
        }
    }
    #endregion

    #region Destroy

    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    public void DestroyEventListeners()
    {
        ItemEvent.PlayerUpgradePickUp -= UpgradeCurrentMaxHealth;
    }
    #endregion

    #region Change health
    private void HandleHealth(GameObject other, bool physical)
    {
        if (physical)
        {
            CollisionHarmer collisionHarmer = other.GetComponent<CollisionHarmer>() != null ?
                other.GetComponent<CollisionHarmer>() :
                other.GetComponentInParent<CollisionHarmer>();

            HandleCollisionHarmer(collisionHarmer);
        }
        else
        {
            TriggerHarmer triggerHarmer = other.GetComponent<TriggerHarmer>() != null ?
                other.GetComponent<TriggerHarmer>() :
                other.GetComponentInParent<TriggerHarmer>();

            HandleTriggerHarmer(other, triggerHarmer);
        }
    }

    private void HandleTriggerHarmer(GameObject target, TriggerHarmer triggerHarmer)
    {
        int harm = triggerHarmer.GetTriggerHarm(target);
        TryChangeHealth(-harm);
    }

    private void HandleCollisionHarmer(CollisionHarmer collisionHarmer)
    {
        int harm = collisionHarmer.GetCollisionHarm();
        TryChangeHealth(-harm);
    }

    public void TryChangeHealth(int healthChange)
    {
        if (!_died) 
        {
            _health += healthChange;

            if (_health > _currentMaxHealth)
            {
                _health = _currentMaxHealth;
            }

            _healthBarFill.value = _health;

            if (_health <= 0)
            {
                InvokeDeathEvent();
                _died = true;
            }
        }
    }
    #endregion

    #region Events
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (IsHarmer(other.gameObject))
        {
            HandleHealth(other.gameObject, true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsHarmer(other.gameObject) && other.gameObject.GetComponent<AttackBehaviour>().ShotType == AttackShotType.DESCRETE)
        {
            HandleHealth(other.gameObject, false);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsHarmer(other.gameObject) && other.gameObject.GetComponent<AttackBehaviour>().ShotType == AttackShotType.CONSECUTIVE)
        {
            HandleHealth(other.gameObject, false);
        }
    }

    private void UpgradeCurrentMaxHealth(object initiator, ItemEventArguments arguments)
    {
        if (arguments.Name == ItemName.HALALIT_VITALITY)
        {
            _currentMaxHealth++;
            _healthBarBorder.value = _currentMaxHealth;
            Debug.Log("Halalit vitality raised");
        }
    }

    private void InvokeDeathEvent()
    {
        if (gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            HalalitDeathEvent.InvokeHalalitDeath(this, new());
        }
        else if (gameObject.CompareTag(Tag.ENEMY.GetDescription()))
        {
            DeathEvent.InvokeTargetDeath(EventName.ENEMY_DEATH, this, new(transform.localScale.x));
        }
        else if (gameObject.CompareTag(Tag.ASTEROID.GetDescription()))
        {
            DeathEvent.InvokeTargetDeath(EventName.ASTEROID_DEATH, this, new(transform.localScale.x));
        }
    }
    #endregion

    #region Predicates 
    private bool IsHarmer(GameObject other)
    {
        return _harmersDescriptions.Contains(other.tag);
    }

    private bool IsContributor(GameObject other)
    {
        return _contributorsDescriptions.Contains(other.tag);
    }
    #endregion

#if UNITY_EDITOR
    internal int GetHealth()
    {
        return _health;
    }

    internal void SetHealth(int health)
    {
        _health = health;
    }
#endif
}