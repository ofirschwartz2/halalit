using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

// Daily replay (#244): Watch on a finished day opens this full-screen panel in the Leaderboard scene.
// Every ship of the day flies again over the empty arena, faster than real time, with names, live scores and a clock.
// A ship fades where its player died, and at the end the winner is crowned. A player without a track is listed in the results only
public class DailyReplay : MonoBehaviour
{
    private const string LOADING_TEXT = "Loading...";
    private const string NO_REPLAYS_TEXT = "No replays for this day yet";
    private const string COULD_NOT_LOAD_TEXT = "Could not load";

    [SerializeField]
    private GameObject _panel;
    [SerializeField]
    private RectTransform _arena;
    [SerializeField]
    private DailyReplayShip _shipPrefab;
    [SerializeField]
    private TMP_Text _clockText;
    [SerializeField]
    private TMP_Text _statusText;
    [SerializeField]
    private TMP_Text _resultsText;
    // Replay seconds per real second. 0 would never move
    [SerializeField]
    private float _speed = 4f;
    [SerializeField]
    private Color _playerColor = Color.yellow;

    private List<DailyReplayEntry> _entries;
    // One per entry, in the same order. A ship and a path are null for a player without a track
    private List<DailyReplayShip> _ships;
    private List<Vector2[]> _paths;
    private List<bool> _isOut;
    private float _time;
    private float _endSeconds;
    private bool _isPlaying;
    // Set in Awake. Off when a field is not wired, or if Awake has not run, so Open and Close do nothing
    private bool _isReady;
    // A load that finishes after Close, or after a newer Open, is dropped
    private int _loadNumber;

    // The day's online load, swapped for a fake by the tests (#312). Static, so a test can set it before it loads the scene
    private static Func<string, DateTime, Task<List<DailyReplayEntry>>> _loadDay;

    // Unity may run this off the main thread while it loads the scene, so it only sets the call: no Unity API and no log
    static DailyReplay()
    {
        UseRealServices();
    }

    #region Init
    private void Awake()
    {
        _entries = new List<DailyReplayEntry>();
        _ships = new List<DailyReplayShip>();
        _paths = new List<Vector2[]>();
        _isOut = new List<bool>();

        if (!IsWired())
        {
            Debug.LogWarning("DailyReplay: Panel, Arena, Ship Prefab, Clock Text, Status Text or Results Text is not wired in the Leaderboard scene, so Watch does nothing.");
            enabled = false;
            return;
        }

        _panel.SetActive(false);
        _isReady = true;
    }
    #endregion

    #region Buttons
    // Called by LeaderBoard.Watch with the finished day's list version and its Israel date
    public void Open(string versionId, DateTime israelDay)
    {
        if (!_isReady)
        {
            return;
        }

        _loadNumber++;
        ClearShips();
        _panel.SetActive(true);
        _statusText.text = LOADING_TEXT;
        _clockText.text = DailyReplayRules.FormatClock(0f);
        _resultsText.text = string.Empty;
        TryLoad(_loadNumber, versionId, israelDay);
    }

    public void Close()
    {
        if (!_isReady)
        {
            return;
        }

        _loadNumber++;
        ClearShips();
        _panel.SetActive(false);
    }
    #endregion

    private async void TryLoad(int loadNumber, string versionId, DateTime israelDay)
    {
        try
        {
            List<DailyReplayEntry> entries = await _loadDay(versionId, israelDay);

            if (this == null)
            {
                return;
            }

            if (loadNumber != _loadNumber)
            {
                return;
            }

            Play(entries);
        }
        catch (Exception exception)
        {
            Debug.LogError($"DailyReplay: could not load the replay. {exception.Message}");

            if (this == null)
            {
                return;
            }

            if (loadNumber != _loadNumber)
            {
                return;
            }

            _statusText.text = COULD_NOT_LOAD_TEXT;
        }
    }

    private void Play(List<DailyReplayEntry> entries)
    {
        _entries = entries;
        int shipCount = 0;

        foreach (DailyReplayEntry entry in _entries)
        {
            Vector2[] path = DailyTrackCodec.GetPathOrNull(entry.Track);
            DailyReplayShip ship = null;

            if (path == null)
            {
                // A track that cannot be shown counts as no track, so the end time and the results agree with the arena
                entry.Track = null;
            }
            else
            {
                ship = Instantiate(_shipPrefab, _arena);
                ship.Init(entry.PlayerName, entry.IsPlayer, _playerColor);
                shipCount++;
            }

            _paths.Add(path);
            _ships.Add(ship);
            _isOut.Add(false);
        }

        if (shipCount == 0)
        {
            _statusText.text = NO_REPLAYS_TEXT;
            _resultsText.text = DailyReplayRules.GetResultsText(_entries);
            return;
        }

        _statusText.text = string.Empty;
        _time = 0f;
        _endSeconds = DailyReplayRules.GetEndSeconds(_entries);
        _isPlaying = true;
        ShowFrame();
    }

    // A viewer, not gameplay: it runs on real time, so the time scale left by the game does not change it
    private void Update()
    {
        if (!_isPlaying)
        {
            return;
        }

        _time = Mathf.Min(_time + Time.unscaledDeltaTime * _speed, _endSeconds);
        ShowFrame();

        if (_time >= _endSeconds)
        {
            Finish();
        }
    }

    private void ShowFrame()
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            DailyReplayShip ship = _ships[i];

            if (ship == null || _isOut[i])
            {
                continue;
            }

            DailyReplayEntry entry = _entries[i];
            Vector2[] path = _paths[i];

            if (DailyReplayRules.IsAlive(entry, _time))
            {
                ship.MoveTo(DailyTrackCodec.GetPosition(path, entry.Track.SampleSeconds, _time));
                ship.SetScore(DailyTrackCodec.GetScore(entry.Track, _time));
            }
            else
            {
                // The last sample is where the player died
                ship.MoveTo(path[path.Length - 1]);
                ship.SetOut(DailyTrackCodec.GetScore(entry.Track, _time), entry.Track.SurvivalSeconds);
                _isOut[i] = true;
            }
        }

        _clockText.text = DailyReplayRules.FormatClock(_time);
    }

    // Once, when the last ship is out
    private void Finish()
    {
        _isPlaying = false;
        int winnerIndex = DailyReplayRules.GetWinnerIndex(_entries);

        if (winnerIndex != -1 && _ships[winnerIndex] != null)
        {
            _ships[winnerIndex].SetWinner();
        }

        _statusText.text = DailyReplayRules.GetWinnerLine(_entries);
        _resultsText.text = DailyReplayRules.GetResultsText(_entries);
    }

    private void ClearShips()
    {
        _isPlaying = false;

        foreach (DailyReplayShip ship in _ships)
        {
            if (ship != null)
            {
                Destroy(ship.gameObject);
            }
        }

        _entries = new List<DailyReplayEntry>();
        _ships.Clear();
        _paths.Clear();
        _isOut.Clear();
    }

    #region Predicates
    private bool IsWired()
    {
        return _panel != null && _arena != null && _shipPrefab != null && _clockText != null && _statusText != null && _resultsText != null;
    }
    #endregion

    #region Services
    private static void UseRealServices()
    {
        _loadDay = DailyTrackStore.LoadDayAsync;
    }

#if UNITY_EDITOR
    internal static void SetServices(Func<string, DateTime, Task<List<DailyReplayEntry>>> loadDay)
    {
        _loadDay = loadDay;
    }

    internal static void ResetServices()
    {
        UseRealServices();
    }
#endif
    #endregion
}
