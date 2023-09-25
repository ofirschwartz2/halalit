using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

class Health : MonoBehaviour
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

    #region Init
    private void Awake()
    {
        SetEventListeners();
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

    #region Change health
    private void HandleHealth(GameObject other, bool physical)
    {
        if (IsHarmer(other))
        {
            if (physical)
            {
                HandlePhysicalHarmer(other.GetComponent<CollisionHarmer>());
            }
            else
            {
                HandleNonPhysicalHarmer(other.GetComponent<TriggerHarmer>());
            }
        }
    }

    private void HandleNonPhysicalHarmer(TriggerHarmer nonPhysicalHarmer)
    {
        int harm = nonPhysicalHarmer.GetTriggerHarm();
        ChangeHealth(-harm);
    }

    private void HandlePhysicalHarmer(CollisionHarmer physicalHarmer)
    {
        int harm = physicalHarmer.GetCollisionHarm();
        ChangeHealth(-harm);
    }


    public void ChangeHealth(int healthChange)
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
        }
    }
    #endregion

    #region Events
    private void OnCollisionEnter2D(Collision2D other)
    {
        HandleHealth(other.gameObject, true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHealth(other.gameObject, false);
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
            DeathEvent.Invoke(EventName.HALALIT_DEATH, this, new(transform.localScale.x));
        }
        else if (gameObject.CompareTag(Tag.ENEMY.GetDescription()))
        {
            DeathEvent.Invoke(EventName.ENEMY_DEATH, this, new(transform.localScale.x));
        }
        else if (gameObject.CompareTag(Tag.ASTEROID.GetDescription()))
        {
            DeathEvent.Invoke(EventName.ASTEROID_DEATH, this, new(transform.localScale.x));
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
}