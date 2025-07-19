using UnityEngine;
using UnityEngine.UI;

public class ProgressMarkUI : MonoBehaviour
{
    [SerializeField] private Image _outlineImage;
    [SerializeField] private Transform _checkMarkTransform;
    [SerializeField] private Color _pendingColor = new Color(1f, 0.56f, 0f);
    [SerializeField] private Color _completeColor = Color.green;
    [SerializeField] private Color _unreachedColord = Color.grey;

    public void SetIncomplete()
    {
        _checkMarkTransform.gameObject.SetActive(false);
        _outlineImage.color = _unreachedColord;
    }

    public void SetComplete()
    {
        _checkMarkTransform.gameObject.SetActive(true);
        _outlineImage.color = _completeColor;
    }

    public void SetPending()
    {
        _checkMarkTransform.gameObject.SetActive(false);
        _outlineImage.color = _pendingColor;
    }

}
