using Assets.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Daily Twist (#239): shows the run's twist on the HUD for _showSeconds at the start of a daily. With no twist the label is hidden at once.
public class DailyTwistBanner : MonoBehaviour
{
    [SerializeField]
    private Text _twistText;
    [SerializeField]
    private float _showSeconds;

    private void Start()
    {
        // An unwired label is skipped, so the banner does not throw on every Playground load
        if (_twistText == null)
        {
            return;
        }

        if (DailyTwists.Current.IsNone())
        {
            _twistText.gameObject.SetActive(false);
            return;
        }

        _twistText.text = DailyTwists.Current.GetDisplayLine();
        StartCoroutine(HideTwistText(_showSeconds));
    }

    private IEnumerator HideTwistText(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        _twistText.gameObject.SetActive(false);
    }
}
