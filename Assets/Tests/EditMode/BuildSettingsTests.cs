using System;
using System.Linq;
using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using UnityEditor;

public class BuildSettingsTests
{

    [Test]
    public void EverySceneNameIsInBuild()
    {
        foreach (SceneName sceneName in Enum.GetValues(typeof(SceneName)))
        {
            var fileEnding = "/" + sceneName.GetDescription() + ".unity";
            var isInBuild = EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path.EndsWith(fileEnding, StringComparison.Ordinal));

            AssertWrapper.IsTrue(isInBuild, $"Scene {sceneName.GetDescription()} is not an enabled scene in Build Settings, so loading it by name fails");
        }
    }

    [Test]
    public void EveryBuildSceneExists()
    {
        foreach (var scene in EditorBuildSettings.scenes)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);

            AssertWrapper.IsTrue(sceneAsset != null, $"Build Settings lists {scene.path}, but no scene exists there");
        }
    }
}
