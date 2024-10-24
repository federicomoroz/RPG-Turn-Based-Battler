using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WindowBase : MonoBehaviour
{
    protected GameObject _textContainer;
    protected TextMeshProUGUI _text;
    private RectTransform _backgroundRectTransform;

    private void Awake()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        _backgroundRectTransform = (RectTransform)gameObject.transform.GetChild(0);
        _textContainer = gameObject.transform.GetChild(1).gameObject;
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public virtual void Open() 
    {
        _backgroundRectTransform.gameObject.SetActive(true);
        _textContainer.SetActive(true);
    }

    public virtual void Close()
    {
        _backgroundRectTransform.gameObject.SetActive(false);
        _textContainer.SetActive(false);
    }
}
