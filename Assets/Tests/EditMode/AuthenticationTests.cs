using System.IO;
using NUnit.Framework;
using UnityEngine;

public class AuthenticationTests
{
    private const string GAME_CODE_FOLDER = "GameLogic";
    private const string ACCESS_TOKEN_MEMBER = "AccessToken";

    [Test]
    public void NoGameCodeReadsTheAccessToken()
    {
        // Sign-in used to write the player's access token to the device logs (#281).
        // Nothing in the game uses the token, so no script should read it, and then no log can show it
        var checkedScriptCount = 0;

        foreach (var scriptPath in Directory.GetFiles(Path.Combine(Application.dataPath, GAME_CODE_FOLDER), "*.cs", SearchOption.AllDirectories))
        {
            var readsAccessToken = File.ReadAllText(scriptPath).Contains(ACCESS_TOKEN_MEMBER);

            AssertWrapper.IsFalse(readsAccessToken, $"{scriptPath} reads the sign-in {ACCESS_TOKEN_MEMBER}, so it could reach the device logs. If a feature really needs it, ask the architect and update this test");
            checkedScriptCount++;
        }

        AssertWrapper.IsTrue(checkedScriptCount > 0, $"Found no scripts in Assets/{GAME_CODE_FOLDER}, so nothing was checked");
    }
}
