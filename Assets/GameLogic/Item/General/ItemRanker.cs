using UnityEngine;
using Assets.Utils;
using System.Collections.Generic;
using Assets.Enums;

public class ItemRanker : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _ballShotRank;
    [SerializeField]
    private float _laserBeamRank;
    [SerializeField]
    private float _fireRateRank;
    [SerializeField]
    private float _nitroFuelRank;

    private Dictionary<ItemName, float> _rankMap;

    public void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        InitRankMap();
    }

    private void InitRankMap()
    {
        _rankMap = new();

        _rankMap[ItemName.BALL_SHOT] = _ballShotRank;
        _rankMap[ItemName.LASER_BEAM] = _laserBeamRank;
        _rankMap[ItemName.FIRE_RATE] = _fireRateRank;
        _rankMap[ItemName.NITRO_FUEL] = _nitroFuelRank;
    }

    public float GetRank(ItemName itemName)
    {
        return _rankMap[itemName];
    }
}
