using NUnit.Framework;

public class PlayerPlaceTests
{

    [Test]
    public void Ordinals()
    {
        AssertWrapper.AreEqual("1st", PlayerPlace.GetOrdinal(1), "Wrong ordinal for 1");
        AssertWrapper.AreEqual("2nd", PlayerPlace.GetOrdinal(2), "Wrong ordinal for 2");
        AssertWrapper.AreEqual("3rd", PlayerPlace.GetOrdinal(3), "Wrong ordinal for 3");
        AssertWrapper.AreEqual("4th", PlayerPlace.GetOrdinal(4), "Wrong ordinal for 4");
        AssertWrapper.AreEqual("21st", PlayerPlace.GetOrdinal(21), "Wrong ordinal for 21");
        AssertWrapper.AreEqual("22nd", PlayerPlace.GetOrdinal(22), "Wrong ordinal for 22");
        AssertWrapper.AreEqual("23rd", PlayerPlace.GetOrdinal(23), "Wrong ordinal for 23");
        AssertWrapper.AreEqual("101st", PlayerPlace.GetOrdinal(101), "Wrong ordinal for 101");
    }

    [Test]
    public void ElevenToThirteenEndInTh()
    {
        AssertWrapper.AreEqual("11th", PlayerPlace.GetOrdinal(11), "Wrong ordinal for 11");
        AssertWrapper.AreEqual("12th", PlayerPlace.GetOrdinal(12), "Wrong ordinal for 12");
        AssertWrapper.AreEqual("13th", PlayerPlace.GetOrdinal(13), "Wrong ordinal for 13");
        AssertWrapper.AreEqual("111th", PlayerPlace.GetOrdinal(111), "Wrong ordinal for 111");
        AssertWrapper.AreEqual("112th", PlayerPlace.GetOrdinal(112), "Wrong ordinal for 112");
        AssertWrapper.AreEqual("113th", PlayerPlace.GetOrdinal(113), "Wrong ordinal for 113");
    }

    [Test]
    public void RoundNumbersEndInTh()
    {
        AssertWrapper.AreEqual("10th", PlayerPlace.GetOrdinal(10), "Wrong ordinal for 10");
        AssertWrapper.AreEqual("20th", PlayerPlace.GetOrdinal(20), "Wrong ordinal for 20");
        AssertWrapper.AreEqual("100th", PlayerPlace.GetOrdinal(100), "Wrong ordinal for 100");
    }

    [Test]
    public void PlaceTextStartsFromRankZero()
    {
        AssertWrapper.AreEqual("12th of 40 so far", PlayerPlace.GetPlaceText(11, 40, true), "Wrong place text for a list that is still open");
        AssertWrapper.AreEqual("3rd of 9", PlayerPlace.GetPlaceText(2, 9, false), "Wrong place text for a closed list");
    }

    [Test]
    public void OnlyPlayerIsFirstOfOne()
    {
        AssertWrapper.AreEqual("1st of 1 so far", PlayerPlace.GetPlaceText(0, 1, true), "Wrong place text for the only player on a list that is still open");
        AssertWrapper.AreEqual("1st of 1", PlayerPlace.GetPlaceText(0, 1, false), "Wrong place text for the only player on a closed list");
    }

    [Test]
    public void PlayerCountIsNeverBelowThePlace()
    {
        AssertWrapper.AreEqual("12th of 12", PlayerPlace.GetPlaceText(11, 5, false), "The player count was lower than the place");
        AssertWrapper.AreEqual("1st of 1 so far", PlayerPlace.GetPlaceText(0, 0, true), "The player count stayed at 0 although you are on the list");
    }
}
