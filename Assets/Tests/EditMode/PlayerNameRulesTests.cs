using NUnit.Framework;

public class PlayerNameRulesTests
{

    [Test]
    public void LettersAndDigitsFrom3To12AreValid()
    {
        AssertWrapper.IsTrue(PlayerNameRules.IsValid("Ofi"), "A 3-letter name was refused");
        AssertWrapper.IsTrue(PlayerNameRules.IsValid("Ofir2026"), "A name with letters and digits was refused");
        AssertWrapper.IsTrue(PlayerNameRules.IsValid("HalalitPilot"), "A 12-letter name was refused");
    }

    [Test]
    public void TooShortOrTooLongIsRefused()
    {
        AssertWrapper.IsFalse(PlayerNameRules.IsValid(null), "A missing name was accepted");
        AssertWrapper.IsFalse(PlayerNameRules.IsValid(""), "An empty name was accepted");
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("ab"), "A 2-letter name was accepted");
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("HalalitPilots"), "A 13-letter name was accepted");
    }

    [Test]
    public void SpacesAndSymbolsAreRefused()
    {
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("Of ir"), "A name with a space was accepted");
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("Ofir!"), "A name with ! was accepted");
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("Ofir_1"), "A name with _ was accepted");
    }

    [Test]
    public void HebrewLettersAreRefused()
    {
        // "Ofir" in Hebrew letters, escaped so this file stays ASCII
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("\u05D0\u05D5\u05E4\u05D9\u05E8"), "A name in Hebrew letters was accepted, but the menu fonts may not draw it");
    }

    [Test]
    public void CharacterRangesIncludeTheirEdgesOnly()
    {
        AssertWrapper.IsTrue(PlayerNameRules.IsValid("AZaz09"), "A, Z, a, z, 0 or 9 was refused");

        // The characters just before and after A-Z, a-z and 0-9
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("Ofi@"), "A name with @ was accepted");
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("Ofi["), "A name with [ was accepted");
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("Ofi`"), "A name with ` was accepted");
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("Ofi{"), "A name with { was accepted");
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("Ofi/"), "A name with / was accepted");
        AssertWrapper.IsFalse(PlayerNameRules.IsValid("Ofi:"), "A name with : was accepted");
    }

    [Test]
    public void CleanTrimsSpaces()
    {
        AssertWrapper.AreEqual("Ofir", PlayerNameRules.Clean("  Ofir "), "Spaces around a name were kept");
    }

    [Test]
    public void CleanTurnsNullIntoEmpty()
    {
        AssertWrapper.AreEqual("", PlayerNameRules.Clean(null), "A missing name was not turned into an empty one");
    }
}
