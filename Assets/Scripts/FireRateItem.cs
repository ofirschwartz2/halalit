using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Common;

public class FireRateItem : MonoBehaviour, ILoadableItem
{
    public bool UseConfigFile;
    public float CooldownMultiplier;

    private GameObject _gun;

    void Start()
    {
        _gun = GameObject.Find("Gun");
        if(_gun == null)
            throw new System.Exception("FireRateItem - Start: can't find the gun");

        if (UseConfigFile)
            ConfigureFromFile();
    }

    public void LoadItem()
    {
        _gun.SendMessage("FasterCooldownInterval", CooldownMultiplier);
    }

    private void ConfigureFromFile()
    {
        string[] props = {"CooldownMultiplier"};
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

        CooldownMultiplier = propsFromConfig["CooldownMultiplier"];
    }
}
