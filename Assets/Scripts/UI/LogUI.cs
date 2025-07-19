using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LogUI : SingletonBehaviour<LogUI>
{
    public enum MessageType
    {
        NOTE = 1,
        WARNING = 2,
        ERROR = 3,
        SUCCESS = 4,
    }

    [Header("References")]
    [SerializeField] private Transform _informationContainer;
    [Header("Settings")]
    [SerializeField] private bool _showConsoleMessages = false;
    [SerializeField] private float _fadeOutTime = 3f;
    [SerializeField] private Color _noteColor = new Color(0.85f, 0.85f, 0.85f);
    [SerializeField] private Color _warningColor = new Color(1f, 0.75f, 0f);
    [SerializeField] private Color _errorColor = Color.red;
    [SerializeField] private Color _successColor = Color.green;

    private List<TMP_Text> _textList; // this example uses Text mesh pro instead of simple texts
    private int _index = -1;
    private int _count = 0;


    protected override void Initialize ()
    {
        dontDestroyOnload = true; // this is needed if you want to keep this LOG UI after scene is changed
        _textList = _informationContainer.GetComponentsInChildren<TMP_Text>().ToList();
        _count = _textList.Count;
        if (_showConsoleMessages)
        {
            Application.logMessageReceived += HandleConsoleMessages;
        }
    }

    private void OnDestroy()
    {
        if (_showConsoleMessages)
        {
            Application.logMessageReceived -= HandleConsoleMessages;
        }
    }

    private void Update()
    {
        HandleInformationMessageShow();
    }

    private void HandleInformationMessageShow()
    {
        TMP_Text[] sortedArray = _textList.OrderByDescending(t => t.color.a).ToArray(); // sorting
        for (int i = 0; i < sortedArray.Length; i++)
        {
            sortedArray[i].transform.SetSiblingIndex(i);
        }
        foreach (TMP_Text _text in _textList)
        {
            float alpha = _text.color.a;
            float timer = alpha * _fadeOutTime;
            if (alpha > 0.005f)
            {
                timer -= Time.deltaTime;
                _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, Mathf.Lerp(0f, 1f, Mathf.Abs(timer / _fadeOutTime)));
            }
        }
    }

    public void SendLogInformation(string message, MessageType infoMessageType = MessageType.NOTE)
    {
        _index++;
        if (_index >= _count)
        {
            _index = 0;
        }
        _textList[_index].text = message;
        switch (infoMessageType)
        {
            case MessageType.NOTE:
                _textList[_index].color = _noteColor;
                break;
            case MessageType.WARNING:
                _textList[_index].color = _warningColor;
                break;
            case MessageType.ERROR:
                _textList[_index].color = _errorColor;
                break;
            case MessageType.SUCCESS:
                _textList[_index].color = _successColor;
                break;
            default:
                break;
        }
    }

    private void HandleConsoleMessages(string condition, string stackTrace, LogType type)
    {
        SendLogInformation(condition + " + " + stackTrace, MessageType.NOTE);
    }
}