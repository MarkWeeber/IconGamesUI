using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EquationGame : MonoBehaviour
{
    [SerializeField] private ProgressBarSliderUI _progressBarSliderUI;
    [SerializeField] private DraggableHolderUI[] _equationMembers;
    [SerializeField] private DraggableHolderUI[] _variantMemebers;
    [SerializeField] private GameObject _draggableNumberPrefab;
    [SerializeField] private TMP_Text _firstOperator;
    [SerializeField] private TMP_Text _secondOperator;

    private LocalSaveManager _localSaveManager;
    private TimerUI _timerUI;
    private ScoresUI _scoresUI;
    private GameLevelsAsset _gameLevelsAsset;
    private LevelSceneInfo _levelSceneInfo;
    private DraggableNumberUI _instantiatedDraggableNumber;
    private EquationRow _pickedEquationRow;
    private int _currentStage = 0;

    private void Start()
    {
        _localSaveManager = LocalSaveManager.Instance;
        _timerUI = TimerUI.Instance;
        _scoresUI = ScoresUI.Instance;
        GetLevelData();
        AnnounceOnStart();
        InitializeProgressBar();
        AssignEventCallbacks();
        SetUpEquation();
    }

    private void OnDestroy()
    {
        ClearCallbacks();
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

    private void InitializeProgressBar()
    {
        _progressBarSliderUI.StageCount = _levelSceneInfo.LevelStageCount;
        _progressBarSliderUI.Initialize();
    }

    #region equation set-up
    private void SetUpEquation()
    {
        // pick equation
        _pickedEquationRow = equations[Random.Range(0, equations.Length)];
        Debug.Log(_pickedEquationRow.ToString());
        // random numbers setup for members
        int firstMissingMember = Random.Range(0, 4);
        int secondMissingMember = (firstMissingMember + Random.Range(1, 4)) % 4;
        // fill in equation members
        for (int i = 0; i < 4; i++)
        {
            // clear existing number
            _equationMembers[i].ClearHoldingNumber();
            // the missing ones should have no draggableUI inside of them
            if (i == firstMissingMember || i == secondMissingMember)
            {
                continue;
            }
            PlaceDraggableUI(_pickedEquationRow.GetMemberValue(i), _equationMembers[i].transform, _equationMembers[i], false);
        }
        // random numbers setup for variants
        int firstCorrectVariant = Random.Range(0, 5);
        int secondCorrectVariant = (firstCorrectVariant + Random.Range(1, 5)) % 5;
        // fill in variants
        for (int i = 0; i < 5; i++)
        {
            // clear existing number
            _variantMemebers[i].ClearHoldingNumber();
            // place correct answer at randomly picked variant
            if (i == firstCorrectVariant)
            {
                PlaceDraggableUI(_pickedEquationRow.GetMemberValue(firstMissingMember), _variantMemebers[i].transform, _variantMemebers[i]);
                continue;
            }
            // place correct answer at randomly picked variant
            if (i == secondCorrectVariant)
            {
                PlaceDraggableUI(_pickedEquationRow.GetMemberValue(secondMissingMember), _variantMemebers[i].transform, _variantMemebers[i]);
                continue;
            }
            // place wrong asnwer on other variants
            PlaceDraggableUI(Random.Range(_pickedEquationRow.MinNumber() - 5, _pickedEquationRow.MaxNumber() + 5), _variantMemebers[i].transform, _variantMemebers[i]);
        }
        // fill operators
        _firstOperator.text = _pickedEquationRow.GetFirstOperator();
        _secondOperator.text = _pickedEquationRow.GetSecondOperator();
    }

    private void PlaceDraggableUI(int number, Transform parentTransform, DraggableHolderUI holder, bool draggingEnabled = true)
    {
        _instantiatedDraggableNumber = Instantiate(_draggableNumberPrefab, parentTransform).GetComponent<DraggableNumberUI>();
        _instantiatedDraggableNumber.Number = number;
        _instantiatedDraggableNumber.DraggingEnabled = draggingEnabled;
        holder.HoldingDraggable = _instantiatedDraggableNumber;
    }
    #endregion

    #region equiation checking
    private void AssignEventCallbacks()
    {
        foreach (var member in _equationMembers)
        {
            member.OnNumberAccepted += CheckEquation;
        }
    }

    private void ClearCallbacks()
    {
        foreach (var member in _equationMembers)
        {
            member.OnNumberAccepted -= CheckEquation;
        }
    }

    private async void CheckEquation()
    {
        bool matched = true;
        for (int i = 0; i < _equationMembers.Length; i++)
        {
            if (_equationMembers[i].HoldingDraggable == null)
            {
                matched = false;
                break;
            }
            else if (_equationMembers[i].HoldingDraggable.Number != _pickedEquationRow.GetMemberValue(i))
            {
                matched = false;
                break;
            }
        }
        // stage win
        if (matched)
        {
            _currentStage++;
            _progressBarSliderUI.PushProgress();
            if (_currentStage >= _levelSceneInfo.LevelStageCount) // last stage win
            {
                await OnLevelWin();
            }
            else
            {
                SetUpEquation();
            }
        }
    }


    private async Task OnLevelWin()
    {
        _localSaveManager.SubmitScore(_levelSceneInfo.LevelId, _timerUI.Timer);
        // TO-DO call scores board
        _scoresUI.ShowPanel();
        // get all scores on this level
        var allScoresOnThisLevel = await _localSaveManager.GetScores(_levelSceneInfo.LevelId);
        // feed the scores to score board
        _scoresUI.AddScores(allScoresOnThisLevel);
    }
    #endregion

    #region fixed equation examples

    private enum Operator
    {
        Add, Subtract, Multiply, Divide
    }

    private struct EquationRow
    {
        public int a;
        public Operator firstOperator;
        public int b;
        public Operator secondOperator;
        public int c;
        public int answer;
        public EquationRow(int a, Operator firstOperator, int b, Operator secondOperator, int c, int answer)
        {
            this.a = a;
            this.firstOperator = firstOperator;
            this.b = b;
            this.secondOperator = secondOperator;
            this.c = c;
            this.answer = answer;
        }
        public int GetMemberValue(int index)
        {
            int result = 0;
            if (index == 0) result = a;
            if (index == 1) result = b;
            if (index == 2) result = c;
            if (index == 3) result = answer;
            return result;
        }

        public int MaxNumber()
        {
            return Mathf.Max(a, b, c, answer);
        }

        public int MinNumber()
        {
            return Mathf.Min(a, b, c, answer);
        }

        public string GetFirstOperator()
        {
            string resultOperator = "";
            switch (firstOperator)
            {
                case Operator.Add:
                    resultOperator = "+";
                    break;
                case Operator.Subtract:
                    resultOperator = "-";
                    break;
                case Operator.Multiply:
                    resultOperator = "x";
                    break;
                case Operator.Divide:
                    resultOperator = "÷";
                    break;
                default:
                    resultOperator = "";
                    break;
            }
            return resultOperator;
        }

        public string GetSecondOperator()
        {
            string resultOperator = "";
            switch (secondOperator)
            {
                case Operator.Add:
                    resultOperator = "+";
                    break;
                case Operator.Subtract:
                    resultOperator = "-";
                    break;
                case Operator.Multiply:
                    resultOperator = "x";
                    break;
                case Operator.Divide:
                    resultOperator = "÷";
                    break;
                default:
                    resultOperator = "";
                    break;
            }
            return resultOperator;
        }

        public override string ToString()
        {
            return 
                a.ToString() + " " 
                + GetFirstOperator() + " " 
                + b.ToString() + " " 
                + GetSecondOperator() + " " 
                + c.ToString() + " = " 
                + answer.ToString();
        }
    }

    private EquationRow[] equations = new EquationRow[]
    {
        // Multiplication first
        new EquationRow(2, Operator.Add, 3, Operator.Multiply, 4, 14),    // 2 + 3×4 = 2 + 12 = 14
        new EquationRow(5, Operator.Subtract, 2, Operator.Multiply, 6, -7), // 5 - 2×6 = 5 - 12 = -7
        new EquationRow(4, Operator.Add, 5, Operator.Divide, 5, 5),         // 4 + 5/5 = 4 + 1 = 5
        new EquationRow(10, Operator.Subtract, 6, Operator.Divide, 2, 7),  // 10 - 6/2 = 10 - 3 = 7
        
        // Division first
        new EquationRow(12, Operator.Add, 6, Operator.Divide, 3, 14),      // 12 + 6/3 = 12 + 2 = 14
        new EquationRow(20, Operator.Subtract, 15, Operator.Divide, 5, 17),// 20 - 15/5 = 20 - 3 = 17
        
        // Mixed operators
        new EquationRow(3, Operator.Multiply, 4, Operator.Add, 5, 17),     // 3×4 + 5 = 12 + 5 = 17
        new EquationRow(6, Operator.Multiply, 2, Operator.Subtract, 3, 9), // 6×2 - 3 = 12 - 3 = 9
        new EquationRow(8, Operator.Divide, 2, Operator.Add, 6, 10),       // 8/2 + 6 = 4 + 6 = 10
        new EquationRow(9, Operator.Divide, 3, Operator.Subtract, 1, 2),   // 9/3 - 1 = 3 - 1 = 2
        
        // Negative numbers
        new EquationRow(-5, Operator.Add, 3, Operator.Multiply, 4, 7),     // -5 + 3×4 = -5 + 12 = 7
        new EquationRow(4, Operator.Subtract, 6, Operator.Multiply, 2, -8),// 4 - 6×2 = 4 - 12 = -8
        new EquationRow(-8, Operator.Divide, 4, Operator.Add, 5, 3),       // -8/4 + 5 = -2 + 5 = 3
        new EquationRow(10, Operator.Add, -4, Operator.Divide, 2, 8),      // 10 + (-4)/2 = 10 - 2 = 8
        
        // Larger numbers (still within range)
        new EquationRow(15, Operator.Add, 5, Operator.Multiply, 10, 65),    // 15 + 5×10 = 15 + 50 = 65
        new EquationRow(20, Operator.Subtract, 4, Operator.Multiply, 5, 0), // 20 - 4×5 = 20 - 20 = 0
        new EquationRow(25, Operator.Divide, 5, Operator.Add, 10, 15),      // 25/5 + 10 = 5 + 10 = 15
        new EquationRow(30, Operator.Subtract, 10, Operator.Divide, 5, 28), // 30 - 10/5 = 30 - 2 = 28
        
        // Edge cases
        new EquationRow(0, Operator.Add, 5, Operator.Multiply, 10, 50),     // 0 + 5×10 = 0 + 50 = 50
        new EquationRow(10, Operator.Multiply, 0, Operator.Add, 5, 5),      // 10×0 + 5 = 0 + 5 = 5
        new EquationRow(-10, Operator.Multiply, 2, Operator.Add, 20, 0),    // -10×2 + 20 = -20 + 20 = 0
        
        // More complex examples
        new EquationRow(7, Operator.Multiply, 3, Operator.Subtract, 5, 16), // 7×3 - 5 = 21 - 5 = 16
        new EquationRow(12, Operator.Divide, 4, Operator.Multiply, 3, 9),   // 12/4 × 3 = 3 × 3 = 9
        new EquationRow(6, Operator.Add, 4, Operator.Multiply, 5, 26),      // 6 + 4×5 = 6 + 20 = 26
        new EquationRow(8, Operator.Subtract, 4, Operator.Divide, 2, 6)     // 8 - 4/2 = 8 - 2 = 6
    };
    #endregion
}
