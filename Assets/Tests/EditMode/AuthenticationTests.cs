using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

public class AuthenticationTests
{
    private const string GAME_CODE_FOLDER = "GameLogic";
    private const string SIGN_IN_SCRIPT = nameof(Authentication) + ".cs";
    private const string ACCESS_TOKEN_MEMBER = "AccessToken";

    // Comments, and the literals that can hold "//" or "/*" without starting a comment.
    // Literals are matched only to be kept: a log line can read the token inside an interpolated string
    private const string LINE_COMMENT = @"//[^\n]*";
    private const string BLOCK_COMMENT = @"/\*.*?\*/";
    private const string VERBATIM_STRING_LITERAL = @"(?:@\$|\$@|@)""(?:[^""]|"""")*""";
    private const string STRING_LITERAL = @"""(?:[^""\\\n]|\\.)*""";
    private const string CHAR_LITERAL = @"'(?:[^'\\\n]|\\.)*'";
    // Also used by TestServicesRuleTests (#307)
    internal static readonly Regex COMMENT_OR_LITERAL = new Regex(string.Join("|", LINE_COMMENT, BLOCK_COMMENT, VERBATIM_STRING_LITERAL, STRING_LITERAL, CHAR_LITERAL), RegexOptions.Singleline);

    [Test]
    public void NoGameCodeReadsTheAccessToken()
    {
        // Sign-in used to write the player's access token to the device logs (#281).
        // Nothing in the game uses the token, so no script should read it, and then no log can show it
        var gameCodeFolder = Path.Combine(Application.dataPath, GAME_CODE_FOLDER);
        AssertWrapper.IsTrue(Directory.Exists(gameCodeFolder), $"{gameCodeFolder} does not exist, so nothing was checked");

        var scriptsThatReadTheToken = new List<string>();
        var didCheckSignInScript = false;

        foreach (var scriptPath in Directory.GetFiles(gameCodeFolder, "*.cs", SearchOption.AllDirectories))
        {
            if (IsAccessTokenInCode(File.ReadAllText(scriptPath)))
            {
                scriptsThatReadTheToken.Add(scriptPath);
            }

            didCheckSignInScript = didCheckSignInScript || Path.GetFileName(scriptPath) == SIGN_IN_SCRIPT;
        }

        // The token was logged in the sign-in script, so a scan that never reached it looked in the wrong place
        AssertWrapper.IsTrue(didCheckSignInScript, $"Found no {SIGN_IN_SCRIPT} in {gameCodeFolder}, so the check looked in the wrong place. If the script moved, update this test");
        AssertWrapper.IsTrue(scriptsThatReadTheToken.Count == 0, $"These scripts read the sign-in {ACCESS_TOKEN_MEMBER}, so it could reach the device logs: {string.Join(", ", scriptsThatReadTheToken)}. If a feature really needs it, ask the architect and update this test");
    }

    [Test]
    public void TokenCheckFindsReadsInCode()
    {
        AssertWrapper.IsTrue(IsAccessTokenInCode("Debug.Log($\"Access Token: {AuthenticationService.Instance.AccessToken}\");"), "The token check missed the log line that #281 removed");

        // A "//" inside a literal is not a comment, so the code after it is still checked
        AssertWrapper.IsTrue(IsAccessTokenInCode("var url = \"https://example.com\"; var token = AuthenticationService.Instance.AccessToken;"), "The token check missed a read after a web address");
        AssertWrapper.IsTrue(IsAccessTokenInCode("var quote = '\"'; var url = \"https://example.com\"; var token = AuthenticationService.Instance.AccessToken;"), "The token check missed a read after a quote character");
        AssertWrapper.IsTrue(IsAccessTokenInCode("var folder = @\"C:\\Logs\\\"; var url = \"https://example.com\"; var token = AuthenticationService.Instance.AccessToken;"), "The token check missed a read after a verbatim string");
        AssertWrapper.IsTrue(IsAccessTokenInCode("/* old code */ var token = AuthenticationService.Instance.AccessToken; /* more old code */"), "The token check missed a read between two block comments");
    }

    [Test]
    public void TokenCheckIgnoresComments()
    {
        // A comment cannot leak the token, so a note about it, or old code left in a comment, does not fail the check
        AssertWrapper.IsFalse(IsAccessTokenInCode("// Never log the AccessToken (#281)"), "The token check flagged a line comment");
        AssertWrapper.IsFalse(IsAccessTokenInCode("var isSignedIn = true; // not the AccessToken"), "The token check flagged a comment at the end of a line");
        AssertWrapper.IsFalse(IsAccessTokenInCode("/// <summary>Never returns the AccessToken</summary>"), "The token check flagged a doc comment");
        AssertWrapper.IsFalse(IsAccessTokenInCode("/*\nDebug.Log(AuthenticationService.Instance.AccessToken);\n*/"), "The token check flagged old code in a block comment");
    }

    // True if the code reads the token outside its comments. Literals stay in, so a read inside a log string still counts
    private static bool IsAccessTokenInCode(string code)
    {
        var codeWithoutComments = COMMENT_OR_LITERAL.Replace(code, match => match.Value[0] == '/' ? " " : match.Value);

        return codeWithoutComments.Contains(ACCESS_TOKEN_MEMBER);
    }
}
