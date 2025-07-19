using UnityEngine;
using UnityEngine.UI;

public class ProgressBarSliderUI : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private GameObject _checkMarkPrefab;
    public int StageCount = 2;

    private CheckMarkPosition[] checkMarkPositions;
    private RectTransform _rectTransform;
    private GameObject _instantiatedObject;
    private Vector2 _setAnchor = new Vector2(0.5f, 0.5f);
    private ProgressMarkUI _progressMarkUI;
    private int _currentStage = 0;
    private float _distanceBetweenMarks;
    private AudioManager _audioManager;

    public void Initialize()
    {
        checkMarkPositions = new CheckMarkPosition[StageCount];
        _distanceBetweenMarks = 1 / (float)(Mathf.Max(StageCount, 1) - 1); // divisor should not be equal to 0
        PlaceCheckMarksAtStart();
        ManageSliderChange(0);
        //_slider.onValueChanged.AddListener(SliderValueChanged); // for testing purposes from UI inspector, need to comment this line before build
    }

    private void Start()
    {
        _audioManager = AudioManager.Instance;
    }

    public void PushProgress()
    {
        _currentStage++;
        float newStage = _currentStage * _distanceBetweenMarks;
        ManageSliderChange(newStage);
        _audioManager.PlaySuccessSound();
    }

    private void PlaceCheckMarksAtStart()
    {
        if (StageCount > 1)
        {
            for (int i = 0; i < StageCount; i++)
            {
                _setAnchor.x = i * _distanceBetweenMarks;
                CreateAndPlaceCheckMark(_setAnchor, i);
            }
        }
        else if (StageCount == 1)
        {
            _setAnchor.x = 0.5f;
            CreateAndPlaceCheckMark(_setAnchor, 0);
        }
    }

    private void CreateAndPlaceCheckMark(Vector2 setAnchor, int index)
    {
        _instantiatedObject = GameObject.Instantiate(_checkMarkPrefab, _slider.transform);
        _rectTransform = _instantiatedObject.GetComponent<RectTransform>();
        _rectTransform.anchorMin = setAnchor;
        _rectTransform.anchorMax = setAnchor;
        _rectTransform.anchoredPosition = Vector2.zero;
        _progressMarkUI = _instantiatedObject.GetComponent<ProgressMarkUI>();
        _progressMarkUI.SetIncomplete();
        checkMarkPositions[index].setAnchor = setAnchor;
        checkMarkPositions[index].progressMarkUI = _progressMarkUI;
    }

    private void ManageSliderChange(float newValue)
    {
        // progress marks
        for (int i = 0; i < StageCount; i++)
        {
            if (newValue == 0f) // fresh start
            {
                checkMarkPositions[i].progressMarkUI.SetPending();
                break;
            }
            if (newValue > 1f) // all complete
            {
                checkMarkPositions[i].progressMarkUI.SetComplete();
                continue;
            }
            float currentProgressMarksXPosition = checkMarkPositions[i].setAnchor.x;
            if (newValue > currentProgressMarksXPosition)
            {
                checkMarkPositions[i].progressMarkUI.SetComplete();
            }
            else
            {
                checkMarkPositions[i].progressMarkUI.SetPending();
                break;
            }
        }
        // slider value
        _slider.value = newValue;
    }

    private struct CheckMarkPosition
    {
        public Vector2 setAnchor;
        public ProgressMarkUI progressMarkUI;
    }
}
