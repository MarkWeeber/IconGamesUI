using TMPro;
using UnityEngine;

public class UserProfileViewUI : SingletonBehaviour<UserProfileViewUI>
{
    [SerializeField] private TMP_Text _userProfileNameText;

    protected override void Initialize()
    {
        dontDestroyOnload = true;
    }

    public void SetProfileName(string profileName)
    {
        _userProfileNameText.text = profileName;
    }

    public void SetEmptyProflieName()
    {
        _userProfileNameText.text = "";
    }
}
