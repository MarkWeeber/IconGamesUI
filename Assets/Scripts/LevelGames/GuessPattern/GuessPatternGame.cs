using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GuessPatternGame : MonoBehaviour
{
    [SerializeField] private Transform _guessPatterPanel;
    [SerializeField] private int _minRandomNumber = 1;
    [SerializeField] private int _maxRandomNumber = 99;
    [SerializeField] private ProgressBarSliderUI _progressBarSliderUI;

    private LocalSaveManager _localSaveManager;
    private PatternFrameUI[] _patternFrameUIs;
    private PatternFrameUI _patterFrameUI;
    private GameLevelsAsset _gameLevelsAsset;
    private TimerUI _timerUI;
    private ScoresUI _scoresUI;
    private LevelSceneInfo _levelSceneInfo;
    private int _newRandomNumber;
    private int[] _selectedPattern;
    private int _clickedCount = 0;
    private int _currentStage = 0;

    private void Start()
    {
        _localSaveManager = LocalSaveManager.Instance;
        _timerUI = TimerUI.Instance;
        _scoresUI = ScoresUI.Instance;
        GetLevelData();
        InitializeProgressBar();
        InitializePatternFrames();
        RandomizeNumbers(registerCallbacks: true);
        AnnounceOnStart();
    }

    private void GetLevelData()
    {
        _gameLevelsAsset = GameContext.Instance.GameLevelsAsset;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        _levelSceneInfo = _gameLevelsAsset.LevelSceneInfos.Where(n => n.LevelSceneBuildIndex == currentSceneIndex).First();
    }

    private void AnnounceOnStart()
    {
        GeneralInformationWindowUI.Instance.CallWindow(message: _levelSceneInfo.InfoMessage, action: () => { _timerUI.StartTimer(); });
    }

    private void InitializePatternFrames()
    {
        _patternFrameUIs = _guessPatterPanel.GetComponentsInChildren<PatternFrameUI>();
        _selectedPattern = new int[_patternFrameUIs.Length];
    }

    private void InitializeProgressBar()
    {
        _progressBarSliderUI.StageCount = _levelSceneInfo.LevelStageCount;
        _progressBarSliderUI.Initialize();
    }

    private void RandomizeNumbers(bool registerCallbacks = false, bool reset = false)
    {
        for (int i = 0; i < _patternFrameUIs.Length; i++)
        {
            _patterFrameUI = _patternFrameUIs[i];
            _newRandomNumber = Random.Range(_minRandomNumber, _maxRandomNumber + 1);
            _patterFrameUI.SetNumber(_newRandomNumber);
            if (reset)
            {
                _patterFrameUI.ClearClick();
            }
            if (registerCallbacks)
            {
                _patterFrameUI.OnClicked += OnPatternFrameClicked;
            }
        }
    }

    private async void OnPatternFrameClicked(PatternFrameUI patterFrameUI)
    {
        if (_clickedCount == _patternFrameUIs.Length - 1) // last member
        {
            _selectedPattern[_clickedCount] = patterFrameUI.Number;
            _clickedCount = 0;
            if (IsArraySortedAscending(_selectedPattern))
            {
                // win condition
                await ResetPattern(true);
            }
            else
            {
                await ResetPattern();
            }
            ClearArray(_selectedPattern);
        }
        else
        {
            _selectedPattern[_clickedCount] = patterFrameUI.Number;
            _clickedCount++;
        }

    }

    private async Task ResetPattern(bool won = false)
    {
        if (won)
        {
            _currentStage++;
            _progressBarSliderUI.PushProgress();
        }
        if (_currentStage >= _levelSceneInfo.LevelStageCount) // all stages complete
        {
            // level win
            await OnLevelWin();
        }
        else
        {
            RandomizeNumbers(reset: true);
        }
    }

    private async Task OnLevelWin()
    {
        _localSaveManager.SubmitScore(_levelSceneInfo.LevelId, _timerUI.Timer);
        // TO-DO call scores board
        _scoresUI.ShowPanel();
        // get all scores on this level
        var allScoresOnThisLevel = await _localSaveManager.GetScores(_levelSceneInfo.LevelId);
        LogUI.Instance.SendLogInformation("TEST1: " + allScoresOnThisLevel.Length.ToString() ,LogUI.MessageType.SUCCESS);
        // feed the scores to score board
        _scoresUI.AddScores(allScoresOnThisLevel);
    }

    private bool IsArraySortedAscending(int[] array)
    {
        return array.SequenceEqual(array.OrderBy(n => n));
    }

    private void ClearArray(int[] array, int number = 0)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = number;
        }
    }
}
