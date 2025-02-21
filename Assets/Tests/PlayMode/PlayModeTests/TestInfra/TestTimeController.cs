using UnityEngine;

namespace Assets.Tests.PlayMode.PlayModeTests.TestInfra
{
    public static class TestTimeController
    {
        private const float DEFAULT_TIME_SCALE = 5.0f;
        private static float _originalTimeScale;

        public static void SetTestTimeScale()
        {
            _originalTimeScale = Time.timeScale;
            Time.timeScale = DEFAULT_TIME_SCALE;
        }

        public static void ResetTimeScale()
        {
            Time.timeScale = _originalTimeScale;
        }

        public static void SetCustomTimeScale(float scale)
        {
            Time.timeScale = scale;
        }
    }
} 