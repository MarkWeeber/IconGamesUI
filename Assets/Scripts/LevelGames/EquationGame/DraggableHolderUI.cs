using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableHolderUI : MonoBehaviour, IDropHandler
{
    private DraggableNumberUI _draggableNumberUI;
    public DraggableNumberUI HoldingDraggable { get => _draggableNumberUI; set => _draggableNumberUI = value; }
    private DraggableNumberUI _incomingHoldingDraggable;

    private static List<DraggableHolderUI> _instances = new List<DraggableHolderUI>();
    public Action OnNumberAccepted;

    private void Start()
    {
        _instances.Add(this);
        CheckIfHolding();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.TryGetComponent<DraggableNumberUI>(out _incomingHoldingDraggable))
        {
            if (_draggableNumberUI == null && _draggableNumberUI != _incomingHoldingDraggable)
            {
                OnDraggableAccepted(_incomingHoldingDraggable);
                _incomingHoldingDraggable.SetHousingHolder(this);
                _draggableNumberUI = _incomingHoldingDraggable;
                OnNumberAccepted?.Invoke();
            }
        }
    }

    public void ClearHoldingNumber()
    {
        if (_draggableNumberUI != null)
        {
            Destroy(_draggableNumberUI.gameObject);
        }
        _draggableNumberUI = null;
    }

    private void CheckIfHolding()
    {
        var childNumber = GetComponentInChildren<DraggableNumberUI>();
        if (childNumber != null)
        {
            _draggableNumberUI = childNumber;
            childNumber.SetHousingHolder(this);
        }
    }

    private static void OnDraggableAccepted(DraggableNumberUI draggableNumber)
    {
        foreach (var instance in _instances)
        {
            if (instance._draggableNumberUI == draggableNumber)
            {
                instance._draggableNumberUI = null;
            }
        }
    }

}
