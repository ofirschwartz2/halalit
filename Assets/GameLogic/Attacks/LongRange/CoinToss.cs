using Assets.Enums;
using Assets.Models;
using Assets.Utils;
using UnityEngine;

public class CoinToss : BallShot
{
    [SerializeField]
    private AttackBehaviour _attackBehaviour;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private Sprite _pebbleSprite;
    [SerializeField]
    private int _pebblePower = 1;

    private IScoreWallet _wallet;
    private bool _isCoin;
    private bool _didLand;

    // Awake, not Start: the shot can hit in its first physics step, before its Start.
    private void Awake()
    {
        _wallet = GameObject.FindGameObjectWithTag(Tag.SCORE.GetDescription()).GetComponent<IScoreWallet>();
        _isCoin = _wallet.TrySpend(ValuableName.SILVER);

        if (!_isCoin)
        {
            BecomePebble();
        }
    }

    private void BecomePebble()
    {
        var currentStats = _attackBehaviour.AttackStats;
        _attackBehaviour.AttackStats = new AttackStats(ItemRank.COMMON, _pebblePower, 1f, 0, currentStats.Rate, currentStats.Weight);
        _spriteRenderer.sprite = _pebbleSprite;
    }

    protected override void OnLanded()
    {
        if (_didLand || !_isCoin)
        {
            return;
        }

        _didLand = true;
        ValuableDropEvent.Invoke(EventName.NEW_VALUABLE_DROP, this, new(Dropper.DEFAULT, transform.position, Vector2.zero, ValuableName.SILVER));
    }

#if UNITY_EDITOR
    internal int GetPebblePower()
    {
        return _pebblePower;
    }
#endif
}
