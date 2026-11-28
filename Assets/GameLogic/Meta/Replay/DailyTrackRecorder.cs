using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

// Daily replay (#244): sits on the Halalit and records its path and score 4 times a second of game time.
// It records every run, which is cheap, and hands the track to PlayerStats only after a daily, as Score does with the score.
// The results card saves one frame after the death (#249), so the save always finds the track
public class DailyTrackRecorder : MonoBehaviour
{
    private Bounds _worldBounds;
    private Score _score;
    private List<Vector2> _positions;
    private List<int> _scoreSampleIndexes;
    private List<int> _scores;
    private int _lastScore;
    private float _elapsedSeconds;
    private bool _isRecording;

    #region Init
    private void Awake()
    {
        _positions = new List<Vector2>();
        _scoreSampleIndexes = new List<int>();
        _scores = new List<int>();
        _lastScore = 0;
        _elapsedSeconds = 0f;
        _isRecording = false;

        SetEventListeners();
    }

    private void SetEventListeners()
    {
        GameOverEvent.GameOver += OnGameOver;
    }

    private void Start()
    {
        _worldBounds = GameObject.FindGameObjectWithTag(Tag.INTERNAL_WORLD.GetDescription()).GetComponent<Collider2D>().bounds;
        _score = GameObject.FindGameObjectWithTag(Tag.SCORE.GetDescription()).GetComponent<Score>();

        _isRecording = true;
        TakeSample();
    }
    #endregion

    #region Destroy
    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    public void DestroyEventListeners()
    {
        GameOverEvent.GameOver -= OnGameOver;
    }
    #endregion

    private void FixedUpdate()
    {
        if (!_isRecording)
        {
            return;
        }

        _elapsedSeconds += Time.fixedDeltaTime;

        if (DailyTrackCodec.IsSampleDue(_positions.Count, _elapsedSeconds))
        {
            TakeSample();
        }
    }

    // One last sample at the death position. Past the 30-minute cap only the final score is kept, so the replay ends on the listed score.
    // A second game over is ignored
    private void OnGameOver(object initiator, GameOverEventArguments arguments)
    {
        if (!_isRecording)
        {
            return;
        }

        if (DailyTrackCodec.CanAddSample(_positions.Count))
        {
            TakeSample();
        }
        else
        {
            TryAddScoreChange(DailyTrackCodec.GetSampleIndex(_elapsedSeconds));
        }

        _isRecording = false;

        if (PlayerStats._isDailyRun)
        {
            PlayerStats._dailyTrack = DailyTrackCodec.ToJson(_positions, _scoreSampleIndexes, _scores, _elapsedSeconds);
        }
    }

    // Callers check the 30-minute cap first. The survival time keeps counting after it
    private void TakeSample()
    {
        _positions.Add(GetNormalisedPosition());
        TryAddScoreChange(_positions.Count - 1);
    }

    // 0 to 1 across the InternalWorld box on each axis
    private Vector2 GetNormalisedPosition()
    {
        Vector3 position = transform.position;
        return new Vector2(
            Mathf.InverseLerp(_worldBounds.min.x, _worldBounds.max.x, position.x),
            Mathf.InverseLerp(_worldBounds.min.y, _worldBounds.max.y, position.y));
    }

    private void TryAddScoreChange(int sampleIndex)
    {
        int score = _score.GetScore();

        if (score == _lastScore)
        {
            return;
        }

        _scoreSampleIndexes.Add(sampleIndex);
        _scores.Add(score);
        _lastScore = score;
    }
}
