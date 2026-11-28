using TMPro;
using UnityEngine;

// Daily replay (#244): one ship marker on the replay arena, with the player's name above it and the live score below it.
// DailyReplay creates it and moves it; it looks nothing up itself
public class DailyReplayShip : MonoBehaviour
{
    private const float OUT_ALPHA = 0.35f;

    [SerializeField]
    private TMP_Text _nameText;
    [SerializeField]
    private TMP_Text _scoreText;
    [SerializeField]
    private CanvasGroup _canvasGroup;
    [SerializeField]
    private GameObject _crown;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Init(string playerName, bool isPlayer, Color playerColor)
    {
        _nameText.text = playerName;
        SetScore(0);
        _canvasGroup.alpha = 1f;
        _crown.SetActive(false);

        if (isPlayer)
        {
            _nameText.color = playerColor;
            _scoreText.color = playerColor;
        }
    }

    // A point 0 to 1 across the arena on each axis. Both anchors on the point and no offset put the ship's pivot on it, so no screen maths is needed
    public void MoveTo(Vector2 normalised)
    {
        _rectTransform.anchorMin = normalised;
        _rectTransform.anchorMax = normalised;
        _rectTransform.anchoredPosition = Vector2.zero;
    }

    public void SetScore(int score)
    {
        _scoreText.text = score.ToString();
    }

    // The ship stays, faded, where its player died
    public void SetOut(int finalScore, float survivalSeconds)
    {
        _canvasGroup.alpha = OUT_ALPHA;
        _scoreText.text = DailyReplayRules.GetOutText(finalScore, survivalSeconds);
    }

    // The crown shows at full strength, even on a ship that is out
    public void SetWinner()
    {
        _canvasGroup.alpha = 1f;
        _crown.SetActive(true);
    }
}
