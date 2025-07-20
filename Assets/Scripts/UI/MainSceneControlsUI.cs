using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneControlsUI : MonoBehaviour
{
    [SerializeField] private InputField _playerNameInputField;
    [SerializeField] private Transform _mainPanel;
    [SerializeField] private Transform _profilePanel;
    [SerializeField] private Transform _levelSelectPanel;

    // web-gl update
    private LocalSaveManager _localSaveManager;
    private UserProfileViewUI _userProfileViewUI;
    private string _enteredPlayerName;

    private void Start()
    {
        _localSaveManager = LocalSaveManager.Instance;
        _userProfileViewUI = UserProfileViewUI.Instance;
        CheckProfileLoadedOnStart();
    }

    public async void SubmitPlayerName()
    {
        _enteredPlayerName = _playerNameInputField.text;
        if (_enteredPlayerName == "")
        {
            LogUI.Instance.SendLogInformation("Please Enter Your Name", LogUI.MessageType.WARNING);
            return;
        }
        await _localSaveManager.SubmitProfileName(_enteredPlayerName);
        EnterLevelSelectPanel();
    }

    public void LoadSceneIndex(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void EnterMainPanel()
    {
        _mainPanel.gameObject.SetActive(true);
        _profilePanel.gameObject.SetActive(false);
        _levelSelectPanel.gameObject.SetActive(false);
    }

    public void EnterProfilePanel()
    {
        _mainPanel.gameObject.SetActive(false);
        _profilePanel.gameObject.SetActive(true);
        _levelSelectPanel.gameObject.SetActive(false);
        _userProfileViewUI.SetEmptyProflieName();
    }

    public void ChangeProfile()
    {
        _localSaveManager.UnloadProfile();
        EnterProfilePanel();
    }

    private void EnterLevelSelectPanel()
    {
        if (CheckIfProfileAlreadyLoaded())
        {
            _userProfileViewUI.SetProfileName(_localSaveManager.ProgressData.ProfileName);
            _mainPanel.gameObject.SetActive(false);
            _profilePanel.gameObject.SetActive(false);
            _levelSelectPanel.gameObject.SetActive(true);
        }
        else
        {
            LogUI.Instance.SendLogInformation("Profile not selected", LogUI.MessageType.WARNING);
        }
    }

    private void CheckProfileLoadedOnStart()
    {
        if (CheckIfProfileAlreadyLoaded())
        {
            _mainPanel.gameObject.SetActive(false);
            _profilePanel.gameObject.SetActive(false);
            _levelSelectPanel.gameObject.SetActive(true);
        }
        else
        {
            _mainPanel.gameObject.SetActive(true);
            _profilePanel.gameObject.SetActive(false);
            _levelSelectPanel.gameObject.SetActive(false);
            _userProfileViewUI.SetEmptyProflieName();
        }
    }

    private bool CheckIfProfileAlreadyLoaded()
    {
        if (_localSaveManager.ProgressData != null && _localSaveManager.ProgressData.ProfileName != "")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
