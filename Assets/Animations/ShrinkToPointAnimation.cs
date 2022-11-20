using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShrinkToPointAnimation : MonoBehaviour
{
    public float Speed = 2000;
    public float MaxOperationTime = 2;
    public bool StartVisible = true;
    public delegate Vector3 PointDelegate();
    public PointDelegate GetShrinkPoint;
    private Vector3 _shrinkPoint;

    private RectTransform _rectTransform;
    private Vector3 _scale;
    private Vector3 _position;
    private ContentSizeFitter _sizeFitter;

    private Coroutine _showCoroutine, _hideCoroutine;
    private float _coroutineStartTime;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _scale = _rectTransform.localScale;
        _position = _rectTransform.anchoredPosition;
        _sizeFitter = GetComponent<ContentSizeFitter>();

        if (!StartVisible)
        {
            if (_sizeFitter != null)
                _sizeFitter.enabled = false;
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }
    }

    public void Show(UnityAction<ShrinkToPointAnimation> onVisible)
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
        _showCoroutine = StartCoroutine(ShowCoroutine(onVisible));
    }

    private IEnumerator ShowCoroutine(UnityAction<ShrinkToPointAnimation> onVisible)
    {
        _coroutineStartTime = Time.time;
        _shrinkPoint = GetShrinkPoint();
        Vector3 scale = _rectTransform.localScale;
        Vector3 position = _rectTransform.position;
        while (_rectTransform.position != _position && _rectTransform.localScale.x < _scale.x && _rectTransform.localScale.y < _scale.y)
        {
            float moveDelta = Mathf.Min(Speed * Time.deltaTime, (_position - position).magnitude);
            position += (_position - position).normalized * moveDelta;
            _rectTransform.position = position;
            float relativeDelta = (_position - position).magnitude / (_position - _shrinkPoint).magnitude;
            scale = (1 - relativeDelta) * _scale;
            _rectTransform.localScale = scale;
            //Debug.Log($"Enabling - {size} / {_rectTransform.sizeDelta.y}");
            yield return new WaitForEndOfFrame();
            if (Time.time - _coroutineStartTime > MaxOperationTime)
            {
                _rectTransform.position = _position;
                _rectTransform.localScale = _scale;
            }
        }

        _showCoroutine = null;
        if (_sizeFitter != null)
            _sizeFitter.enabled = true;
        if (onVisible != null)
            onVisible(this);
    }

    public void Hide(UnityAction<ShrinkToPointAnimation> onHidden)
    {
        if (_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
            _showCoroutine = null;
        }
        _position = _rectTransform.position;
        _hideCoroutine = StartCoroutine(HideCoroutine(onHidden));
    }

    private IEnumerator HideCoroutine(UnityAction<ShrinkToPointAnimation> onHidden)
    {
        _coroutineStartTime = Time.time;
        _shrinkPoint = GetShrinkPoint();
        if (_sizeFitter != null)
            _sizeFitter.enabled = false;
        Vector3 scale = _rectTransform.localScale;
        Vector3 position = _rectTransform.position;
        while (_rectTransform.position != _shrinkPoint && _rectTransform.localScale.x > 0 && _rectTransform.localScale.y > 0)
        {
            float moveDelta = Mathf.Min(Speed * Time.deltaTime, (_shrinkPoint - position).magnitude);
            position += (_shrinkPoint - position).normalized * moveDelta;
            _rectTransform.position = position;
            float relativeDelta = (_shrinkPoint - position).magnitude / (_position - _shrinkPoint).magnitude;
            scale = relativeDelta * _scale;
            _rectTransform.localScale = scale;
            //Debug.Log($"Enabling - {size} / {_rectTransform.sizeDelta.y}");
            yield return new WaitForEndOfFrame();
            if (Time.time - _coroutineStartTime > MaxOperationTime)
            {
                _rectTransform.position = _shrinkPoint;
                _rectTransform.localScale = Vector3.zero;
            }
        }

        _showCoroutine = null;
        if (onHidden != null)
            onHidden(this);
    }
}
