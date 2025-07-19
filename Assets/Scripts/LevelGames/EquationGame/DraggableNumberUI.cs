using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableNumberUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    
    [SerializeField] private TMP_Text _numberText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _bordersImage;
    [SerializeField] private Color _dragabbleColor;
    [SerializeField] private Color _disabledDraggableColor;

    private bool _draggingEnabled = true;
    public bool DraggingEnabled { get => _draggingEnabled; set => SetDraggingEnabled(value); }

    private int _number;
    public int Number { get => _number; set => SetNumber(value); }

    private static Canvas _topViewCanvas;
    private static Canvas _initialCanvas;
    private static int _initialSortingOrder;
    private Transform _homeTransform;
    private Color _backgroundTransitionColorOnDrag;
    private Color _bordersTransitionColorOnDrag;

    private void Start()
    {
        _homeTransform = transform.parent;
        GetCavnases();
    }

    private void GetCavnases()
    {
        if (_topViewCanvas == null)
        {
            _topViewCanvas = transform.root.gameObject.GetComponentInChildren<TopViewCanvasUI>().Canvas;
            if (_topViewCanvas == null)
            {
                Debug.LogError("No canvas found for draggables");
            }
            else
            {
                _initialSortingOrder = _topViewCanvas.sortingOrder;
            }
        }
        if (_initialCanvas == null)
        {
            _initialCanvas = transform.root.GetComponentsInChildren<Canvas>().Where(n => n.GetComponent<TopViewCanvasUI>() == null).FirstOrDefault();
        }
    }

    private void SetDraggingEnabled(bool enabled)
    {
        _draggingEnabled = enabled;
        if (enabled)
        {
            _bordersImage.color = _dragabbleColor;
        }
        else
        {
            _bordersImage.color = _disabledDraggableColor;
        }
    }

    private void SetNumber(int number)
    {
        _numberText.text = number.ToString();
        _number = number;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggingEnabled)
        {
            transform.position = eventData.position;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_draggingEnabled)
        {
            _bordersImage.raycastTarget = false;
            transform.parent = _topViewCanvas.transform;
            _topViewCanvas.sortingOrder = 99;
            _backgroundTransitionColorOnDrag = _backgroundImage.color;
            _backgroundTransitionColorOnDrag.a = 0.5f;
            _backgroundImage.color = _backgroundTransitionColorOnDrag;
            _bordersTransitionColorOnDrag = _bordersImage.color;
            _bordersTransitionColorOnDrag.a = 0.5f;
            _bordersImage.color = _bordersTransitionColorOnDrag;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_draggingEnabled)
        {
            transform.parent = _homeTransform;
            transform.position = _homeTransform.position;
            _bordersImage.raycastTarget = true;
            _topViewCanvas.sortingOrder = _initialSortingOrder;
            _backgroundTransitionColorOnDrag = _backgroundImage.color;
            _backgroundTransitionColorOnDrag.a = 1f;
            _backgroundImage.color = _backgroundTransitionColorOnDrag;
            _bordersTransitionColorOnDrag = _bordersImage.color;
            _bordersTransitionColorOnDrag.a = 1f;
            _bordersImage.color = _bordersTransitionColorOnDrag;
        }
    }

    public void SetHousingHolder(DraggableHolderUI holder)
    {
        _homeTransform = holder.transform;
    }
}
