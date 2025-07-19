using UnityEngine;
using UnityEngine.Events;

public class UnityEventContainer : MonoBehaviour
{
    [SerializeField] private UnityEvent unityEvent;

    public void Invoke()
    {
        unityEvent?.Invoke();
    }

}
