using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PatternFrameUI : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField] private Animator _animator;
    [SerializeField] private TMP_Text _numberText;

    private int _number;
    public int Number { get => _number; }
    private bool _clicked;

    public Action<PatternFrameUI> OnClicked;

    public void SetNumber(int number)
    {
        _number = number;
        _numberText.text = number.ToString();
    }

    public void ClearClick()
    {
        _clicked = false;
        _animator.SetTrigger("Reset");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _clicked = true;
        _animator.SetTrigger("Selected");
        OnClicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_clicked)
        {
            _animator.SetTrigger("OnHover");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_clicked)
        {
            _animator.SetTrigger("Reset");
        }
    }
}
