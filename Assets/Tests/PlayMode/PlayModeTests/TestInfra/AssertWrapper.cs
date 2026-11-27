using NUnit.Framework;
using UnityEngine;

internal static class AssertWrapper
{
    internal static void AreSame(object expected, object actual, string failMessage, int? seed = null)
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.AreSame(expected, actual, failMessage);
    }

    internal static void AreEqual(Vector2 expected, Vector2 actual, string failMessage, int? seed = null, float acceptedDelta = 0)
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.AreEqual(expected.x, actual.x, acceptedDelta, failMessage);
        Assert.AreEqual(expected.y, actual.y, acceptedDelta, failMessage);
    }

    internal static void AreEqual(string expected, string actual, string failMessage, int? seed = null)
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.AreEqual(expected, actual, failMessage);
    }

    internal static void AreEqual(float expected, float actual, string failMessage, int? seed = null, float acceptedDelta = 0)
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.AreEqual(expected, actual, acceptedDelta, failMessage);
    }

    internal static void AreNotEqual(float expected, float actual, string failMessage, int? seed = null)
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.AreNotEqual(expected, actual, failMessage);
    }

    internal static void IsNull(UnityEngine.Object obj, int? seed = null, string failMessage = "Not a Null Object")
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.IsNull(NullIfUnityNull(obj), failMessage);
    }

    internal static void IsNotNull(UnityEngine.Object obj, int? seed = null, string failMessage = "Null Object")
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.IsNotNull(NullIfUnityNull(obj), failMessage);
    }
    
    internal static void GreaterOrEqual(float greater, float smaller, string failMessage, int? seed = null, float acceptedDelta = 0)
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.GreaterOrEqual(greater + acceptedDelta, smaller, failMessage);
    }

    internal static void Greater(float greater, float smaller, string failMessage, int? seed = null)
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.Greater(greater, smaller, failMessage);
    }

    internal static void IsTrue(bool condition, string failMessage, int? seed = null)
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.IsTrue(condition, failMessage);
    }

    internal static void IsFalse(bool condition, string failMessage, int? seed = null)
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.IsFalse(condition, failMessage);
    }

    internal static void Fail(string failMessage, int? seed = null)
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.Fail(failMessage);
    }

    internal static void Less(float smaller, float greater, string failMessage, int? seed = null)
    {
        failMessage = CombineFailMessageWithSeed(failMessage, seed);
        Assert.Less(smaller, greater, failMessage);
    }

    // Unity's == null is also true for a destroyed object, and in the editor for the stand-in ("fake null")
    // that GetComponent returns for a missing component or that an unassigned serialized field holds.
    // NUnit checks only the C# reference, so it gets a real null in those cases (#254).
    private static UnityEngine.Object NullIfUnityNull(UnityEngine.Object obj)
    {
        return obj == null ? null : obj;
    }

    private static string CombineFailMessageWithSeed(string failMessage, int? seed)
    {
        return seed != null ?
            failMessage + $"\n Seed: {seed}"
            :
            failMessage;
    }    
}