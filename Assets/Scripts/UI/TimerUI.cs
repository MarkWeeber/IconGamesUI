using System;
using TMPro;
using UnityEngine;

public class TimerUI : SingletonBehaviour<TimerUI>
{
    [SerializeField] private TMP_Text _timerText;

    private float _timer;
    public float Timer { get => _timer; }

    private bool _started;
    private TimeSpan _timeSpan;

    private void Update()
    {
        if (_started)
        {
            _timer += Time.deltaTime;
        }
        _timerText.text = GetTimeString();
    }

    public void StartTimer()
    {
        _started = true;
    }

    public void ResetTimer()
    {
        _timer = 0f;
    }

    public void StopTimer()
    {
        _started = false;
    }

    public string GetTimeString()
    {
        return TimeSpan.FromSeconds(_timer).ToString("mm':'ss");
    }

    public static string GetTimeString(float time)
    {
        return TimeSpan.FromSeconds(time).ToString("mm':'ss");
    }
}
