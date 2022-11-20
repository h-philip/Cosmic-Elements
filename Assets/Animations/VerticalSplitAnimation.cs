using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VerticalSplitAnimation : MonoBehaviour
{
    public float Speed = 2000;
    public bool StartVisible = true;

    private RectTransform _rectTransform;
    private Vector2 _size;

    private Coroutine _showCoroutine, _hideCoroutine;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _size = _rectTransform.rect.size;

        if (!StartVisible)
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
    }

    public void Show(UnityAction<VerticalSplitAnimation> onVisible)
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
        _showCoroutine = StartCoroutine(ShowCoroutine(onVisible));
    }

    private IEnumerator ShowCoroutine(UnityAction<VerticalSplitAnimation> onVisible)
    {
        float size = _rectTransform.rect.height;
        while (_rectTransform.rect.height < _size.y)
        {
            size += Mathf.Min(Speed * Time.deltaTime, _size.y - size);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            //Debug.Log($"Enabling - {size} / {_rectTransform.sizeDelta.y}");
            yield return new WaitForEndOfFrame();
        }

        _showCoroutine = null;
        if (onVisible != null)
            onVisible(this);
    }

    public void Hide(UnityAction<VerticalSplitAnimation> onHidden)
    {
        if (_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
            _showCoroutine = null;
        }
        _hideCoroutine = StartCoroutine(HideCoroutine(onHidden));
    }

    private IEnumerator HideCoroutine(UnityAction<VerticalSplitAnimation> onHidden)
    {
        float size = _rectTransform.rect.height;
        while (_rectTransform.rect.height > 0)
        {
            size -= Mathf.Min(Speed * Time.deltaTime, size);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            //Debug.Log($"Disabling - {size} / {_rectTransform.sizeDelta.y}");
            yield return new WaitForEndOfFrame();
        }

        _hideCoroutine = null;
        if (onHidden != null)
            onHidden(this);
    }
}
