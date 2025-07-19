using TMPro;
using UnityEngine;

public class ScoreBoardElementUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _numberText;
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _playerLastScore;
    [SerializeField] private TMP_Text _playerBestScore;

    public void SetNumber(int number)
    {
        _numberText.text = number.ToString();
    }

    public void SetPlayerName(string name)
    {
        _playerNameText.text = name;
    }

    public void SetPlayerLastScore(string score)
    {
        _playerLastScore.text = score;
    }

    public void SetPlayerBestScore(string score)
    {
        _playerBestScore.text = score;
    }
}
