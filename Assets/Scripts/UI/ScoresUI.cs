using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoresUI : SingletonBehaviour<ScoresUI>
{
    [SerializeField] private GameObject _scoreBoardElementPrefab;
    [SerializeField] private Transform _background;
    [SerializeField] private Transform _contentTransform;
    [SerializeField] private Transform _goToNextLevelButton;

    private ScoreBoardElementUI _instantiatedScoreBoardElement;
    private GameLevelsAsset _gameLevelsAsset;
    private LevelSceneInfo _currentLevelSceneInfo;
    private int _currentLevelSceneInfoIndex;
    private int _currentLevelSceneIndex;
    private bool _lastLevel;


    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        _gameLevelsAsset = GameContext.Instance.GameLevelsAsset;
        CheckCurrentSceneLevelIndex();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HidePanel();
    }

    public void ShowPanel()
    {
        _background.gameObject.SetActive(true);
    }

    private void HidePanel()
    {
        _background.gameObject.SetActive(false);
        ClearScores();
    }

    private void CheckCurrentSceneLevelIndex()
    {
        _currentLevelSceneIndex = SceneManager.GetActiveScene().buildIndex;
        _currentLevelSceneInfoIndex = Array.FindIndex(_gameLevelsAsset.LevelSceneInfos, info => info.LevelSceneBuildIndex == _currentLevelSceneIndex);
        _currentLevelSceneInfo = _gameLevelsAsset.LevelSceneInfos[_currentLevelSceneInfoIndex];
        int lastLevelSceneBuildIndex = _gameLevelsAsset.LevelSceneInfos[_gameLevelsAsset.LevelSceneInfos.Length - 1].LevelSceneBuildIndex;
        // check if it's the last level
        if (_currentLevelSceneInfo.LevelSceneBuildIndex == lastLevelSceneBuildIndex)
        {
            // then disable Go To next level button
            _goToNextLevelButton.gameObject.SetActive(false);
            _lastLevel = true;
        }
        else
        {
            _goToNextLevelButton.gameObject.SetActive(true);
        }
    }

    private void ClearScores()
    {
        foreach (Transform _tranform in _contentTransform.gameObject.GetComponentInChildren<Transform>())
        {
            if (_tranform == _contentTransform)
            {
                continue;
            }
            Destroy(_tranform.gameObject);
        }
    }

    public void AddScores(Score[] scores)
    {
        int reversedOrder = scores.Length;
        foreach (var score in scores)
        {
            _instantiatedScoreBoardElement = Instantiate(_scoreBoardElementPrefab, _contentTransform).GetComponent<ScoreBoardElementUI>();
            _instantiatedScoreBoardElement.SetNumber(reversedOrder);
            _instantiatedScoreBoardElement.SetPlayerName(score.PlayerName);
            _instantiatedScoreBoardElement.SetPlayerLastScore(TimerUI.GetTimeString(score.LastTime));
            _instantiatedScoreBoardElement.SetPlayerBestScore(TimerUI.GetTimeString(score.BestTime));
            reversedOrder--;
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(_gameLevelsAsset.MainSceneBuildIndex);
    }

    public void GoToNextLevel()
    {
        if (!_lastLevel)
        {
            SceneManager.LoadScene(_gameLevelsAsset.LevelSceneInfos[_currentLevelSceneInfoIndex+1].LevelSceneBuildIndex);
        }
    }
}
