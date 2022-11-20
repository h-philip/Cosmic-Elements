using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class UiIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public bool DrawIcon = true;
    public bool DrawText = false;
    public bool DrawTextOnHover = true;

    public RectTransform TextPrefab;
    public RectTransform IconPrefab;
    public RectTransform Window;

    public UnityEvent<PointerEventData> OnClick = new UnityEvent<PointerEventData>();

    private Vector3 _windowBottomLeft, _windowTopRight;
    private GameObject _uiElement;
    private RectTransform _icon, _text;
    private bool _mouseHover = false;

    private void OnEnable()
    {
        if (_uiElement != null)
            _uiElement.gameObject.SetActive(true);
        if (Window != null)
        {
            Vector3[] corners = new Vector3[4];
            Window.GetWorldCorners(corners);
            _windowBottomLeft = corners[0];
            _windowTopRight = corners[2];
        }
    }

    private void Start()
    {
        CreateUiElement();
        Vector3[] corners = new Vector3[4];
        Window.GetWorldCorners(corners);
        _windowBottomLeft = corners[0];
        _windowTopRight = corners[2];
    }

    private void OnDisable()
    {
        if (_uiElement != null)
            _uiElement.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        // TODO: Currently not drawn if too close, or too far
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        bool isInWindow = IsInWindow(screenPos);
        _icon.gameObject.SetActive(isInWindow && DrawIcon);
        _text.gameObject.SetActive(isInWindow && (DrawText || (DrawTextOnHover && _mouseHover)));
        // If nothing is active -> return
        if (!_icon.gameObject.activeSelf && !_text.gameObject.activeSelf)
            return;

        _uiElement.GetComponent<RectTransform>().position = screenPos;
    }

    private void CreateUiElement()
    {
        Type[] components = { typeof(RectTransform) };
        _uiElement = new GameObject(transform.name, components);
        _uiElement.transform.SetParent(Window, false);
        _icon = Instantiate(IconPrefab, _uiElement.transform);
        IconExtension extension = _icon.gameObject.AddComponent<IconExtension>();
        extension.PointerClick = OnPointerClick;
        extension.PointerEnter = OnPointerEnter;
        extension.PointerExit = OnPointerExit;
        _icon.name = "Icon";
        _text = Instantiate(TextPrefab, _uiElement.transform);
        _text.name = "Text";
        _text.GetComponent<TextMeshProUGUI>().text = transform.name;
    }

    public bool IsInWindow(Vector3 position)
    {
        return position.x >= _windowBottomLeft.x
            && position.x <= _windowTopRight.x
            && position.y >= _windowBottomLeft.y
            && position.y <= _windowTopRight.y;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _mouseHover = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _mouseHover = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick.Invoke(eventData);
    }

    private class IconExtension : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public delegate void EventHandler(PointerEventData eventData);
        public EventHandler PointerEnter, PointerExit, PointerClick;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (PointerEnter != null)
                PointerEnter.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (PointerExit != null)
                PointerExit.Invoke(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (PointerClick != null)
                PointerClick.Invoke(eventData);
        }
    }
}
