using UnityEngine;

public class TopViewCanvasUI : MonoBehaviour
{
    public Canvas Canvas;

    private void Awake()
    {
        Canvas = GetComponent<Canvas>();
    }
}
