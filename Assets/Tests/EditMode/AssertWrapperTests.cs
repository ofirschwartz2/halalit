using NUnit.Framework;
using UnityEngine;

// AssertWrapper's null checks follow Unity's == null (#254). NUnit's own check misses a destroyed object and the
// editor's stand-in ("fake null") for a missing component, so a test could pass while a component is missing.
public class AssertWrapperTests
{
    private const string TEST_OBJECT_NAME = "AssertWrapperTestObject";

    [Test]
    public void DestroyedObjectCountsAsNull()
    {
        var destroyedObject = new GameObject(TEST_OBJECT_NAME);
        UnityEngine.Object.DestroyImmediate(destroyedObject);

        AssertWrapper.IsNull(destroyedObject, failMessage: "IsNull failed on a destroyed object");
        Assert.Throws<AssertionException>(() => AssertWrapper.IsNotNull(destroyedObject), "IsNotNull passed on a destroyed object");
    }

    [Test]
    public void MissingComponentCountsAsNull()
    {
        var holder = new GameObject(TEST_OBJECT_NAME);

        try
        {
            var missingComponent = holder.GetComponent<Health>();

            AssertWrapper.IsNull(missingComponent, failMessage: "IsNull failed on a missing component");
            Assert.Throws<AssertionException>(() => AssertWrapper.IsNotNull(missingComponent), "IsNotNull passed on a missing component");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(holder);
        }
    }

    [Test]
    public void LiveObjectCountsAsNotNull()
    {
        var liveObject = new GameObject(TEST_OBJECT_NAME);

        try
        {
            AssertWrapper.IsNotNull(liveObject, failMessage: "IsNotNull failed on a live object");
            Assert.Throws<AssertionException>(() => AssertWrapper.IsNull(liveObject), "IsNull passed on a live object");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(liveObject);
        }
    }
}
