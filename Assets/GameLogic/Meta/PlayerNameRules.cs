// Player names (#179): what a typed name becomes, and whether Unity may store it
public static class PlayerNameRules
{
    public static string Clean(string typed)
    {
        if (typed == null)
        {
            return "";
        }

        return typed.Trim();
    }

    // Unity refuses spaces, and the menu fonts may not draw other scripts, so a name is ASCII letters and digits only
    public static bool IsValid(string name)
    {
        if (name == null || name.Length < Constants.PLAYER_NAME_MIN_LENGTH || name.Length > Constants.PLAYER_NAME_MAX_LENGTH)
        {
            return false;
        }

        foreach (char character in name)
        {
            if (!IsAsciiLetterOrDigit(character))
            {
                return false;
            }
        }

        return true;
    }

    // Character ranges, not char.IsLetterOrDigit, which also lets in Hebrew and other scripts
    private static bool IsAsciiLetterOrDigit(char character)
    {
        return (character >= 'a' && character <= 'z')
            || (character >= 'A' && character <= 'Z')
            || (character >= '0' && character <= '9');
    }
}
