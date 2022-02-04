using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Common;

public class FireRateItemController : MonoBehaviour
{
    public bool UseConfigFile;
    public float CooldownMultiplier;

    void Start()
    {
        if (UseConfigFile)
            ConfigureFromFile();
    }

    public void LoadItem(GameObject gun)
    {
        gun.SendMessage("FasterCooldownInterval", CooldownMultiplier);
    }

    private void ConfigureFromFile()
    {
        string[] props = {"CooldownMultiplier"};
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

        CooldownMultiplier = propsFromConfig["CooldownMultiplier"];
    }
}
