using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralInformationWindowUI : SingletonBehaviour<GeneralInformationWindowUI>
{
    public enum InformationType { GENERAL, CONFIRMATION }
    [SerializeField] private Transform _mainBackgroundPanel;
    [Header("General Information Window")]
    [SerializeField] private Transform _generalInformationBackgroundPanel;
    [SerializeField] private TMP_Text _generalInformationTitleText;
    [Header("Confirmation Window")]
    [SerializeField] private Transform _confirmationBackgroundPanel;
    [SerializeField] private TMP_Text _confirmationTitleText;

    private Action _embeddedAction;
    private Action _lastAction;

    protected override void Initialize()
    {
        dontDestroyOnload = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ClearEmbeddAction();
    }

    public void CallSimpleWindow(string message)
    {
        CallWindow(message);
    }

    public void CallWindow(string message, InformationType type = InformationType.GENERAL, Action action = null)
    {
        _mainBackgroundPanel.gameObject.SetActive(true);
        if (type == InformationType.GENERAL)
        {
            _generalInformationBackgroundPanel.gameObject.SetActive(true);
            _generalInformationTitleText.text = message;
        }
        else if (type == InformationType.CONFIRMATION)
        {
            _confirmationBackgroundPanel.gameObject.SetActive(true);
            _confirmationTitleText.text = message;
        }
        if (action != null)
        {
            _embeddedAction += action;
            _lastAction = action;
        }
    }

    public void OnButtonClick()
    {
        _embeddedAction?.Invoke();
        ClearEmbeddAction();
        DisablePanels();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DisablePanels();
    }

    private void DisablePanels()
    {
        _mainBackgroundPanel.gameObject.SetActive(false);
        _generalInformationBackgroundPanel.gameObject.SetActive(false);
        _confirmationBackgroundPanel.gameObject.SetActive(false);
    }

    private void ClearEmbeddAction()
    {
        if (_embeddedAction != null)
        {
            if (_lastAction != null)
            {
                _embeddedAction -= _lastAction;
            }
        }
        _lastAction = null;
    }
}
