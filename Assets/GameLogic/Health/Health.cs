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
    #endregion

    #region Change Health
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHarmer(other);
        HandleContributor(other);
    }

    private void HandleHarmer(Collider2D other)
    {
        if (IsHarmer(other.gameObject))
        {
            int healthSubtruction = GetHealthHarm(other.gameObject);
            ChangeHealth(healthSubtruction);
        }
    }

    private int GetHealthHarm(GameObject harmer)
    {
        // TODO (dev): Decide by the harmer power attribute
        return -1;
    }

    private void HandleContributor(Collider2D other)
    {
        if (IsContributor(other.gameObject))
        {
            int healthAddition = GetHealthContribution(other.gameObject);
            ChangeHealth(healthAddition);
        }
    }

    private int GetHealthContribution(GameObject contributor)
    {
        // TODO (dev): Decide by the contributor HpRaise attribute
        return 1;
    }

    private void ChangeHealth(int healthChange)
    {
        _health += healthChange;

        if (_health > _currentMaxHealth)
        {
            _health = _currentMaxHealth;
        }

        _healthBarFill.value = _health;

        if (_health <= 0)
        {
            DeathEvent.Invoke(EventName.HALALIT_DEATH, this, new());
        }
    }
    #endregion

    #region Events actions
    private void UpgradeCurrentMaxHealth(object initiator, ItemEventArguments arguments)
    {
        if (arguments.Name == ItemName.HALALIT_VITALITY)
        {
            _currentMaxHealth++;
            _healthBarBorder.value = _currentMaxHealth;
            Debug.Log("Halalit vitality raised");
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