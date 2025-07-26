using Assets.Enums;
using Assets.Models;
using Items.Utility;
using UnityEngine;

namespace Items.Factory
{
    public class UtilityFactory
    {
        private const string CONFIG_PATH = "UtilityConfigs/";

        public IUtility CreateUtility(ItemName utilityType)
        {
            string configName = GetConfigName(utilityType);
            var config = Resources.Load<BaseUtilityConfiguration>(CONFIG_PATH + configName);
            
            if (config == null)
            {
                Debug.LogError($"UtilityFactory: Could not find configuration at path: {CONFIG_PATH + configName}");
                throw new System.ArgumentException($"Could not find configuration for utility type: {utilityType}");
            }

            return utilityType switch
            {
                ItemName.NITRO_FUEL => new NitroUtility((NitroConfiguration)config),
                ItemName.PUSHING_MAGNET => new PushingMagnetUtility((PushingMagnetConfiguration)config),
                ItemName.PULLING_MAGNET => new PullingMagnetUtility((PullingMagnetConfiguration)config),
                _ => throw new System.ArgumentException($"Unknown utility type: {utilityType}")
            };
        }

        private string GetConfigName(ItemName utilityType)
        {
            return utilityType switch
            {
                ItemName.NITRO_FUEL => "NitroFuelConfig",
                ItemName.PUSHING_MAGNET => "PushingMagnetConfig",
                ItemName.PULLING_MAGNET => "PullingMagnetConfig",
                _ => throw new System.ArgumentException($"No configuration name mapping for utility type: {utilityType}")
            };
        }
    }
} 