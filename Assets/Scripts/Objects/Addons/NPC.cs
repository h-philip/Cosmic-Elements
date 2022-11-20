using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(UiIcon))]
public class NPC : MonoBehaviour, IAddon
{
    public static RectTransform Window = null;
    public static RectTransform PopUpMenuPrefab = null;
    public static TextMeshProUGUI TextPrefab = null;
    public static Button ButtonPrefab = null;

    public string PopUpText;

    private RectTransform _menu;

    void Awake()
    {
        GetComponent<UiIcon>().OnClick.AddListener(OnClick);
        if (Window == null)
            Window = GameObject.FindObjectOfType<WorldMapController>(true).transform.Find("MenuWindow") as RectTransform;
        if (PopUpMenuPrefab == null)
            PopUpMenuPrefab = Resources.Load<RectTransform>("PopUpMenu/PopUpMenu");
        if (TextPrefab == null)
            TextPrefab = Resources.Load<TMPro.TextMeshProUGUI>("PopUpMenu/Text");
        if (ButtonPrefab == null)
            ButtonPrefab = Resources.Load<UnityEngine.UI.Button>("PopUpMenu/Button");
    }

    public void OnClick(PointerEventData eventData)
    {
        if (_menu != null) return;

        if (eventData.button == PointerEventData.InputButton.Right)
            OpenMenu(eventData.position);
    }

    public void OpenMenu(Vector2 position)
    {
        _menu = Instantiate(PopUpMenuPrefab, Window);
        _menu.name = name;
        _menu.GetComponentInChildren<TextMeshProUGUI>().text = name;
        Instantiate(TextPrefab, _menu).text = PopUpText;
        _menu.position = position;
        UiIcon icon = GetComponent<UiIcon>();
        _menu.GetComponent<ShrinkToPointAnimation>().GetShrinkPoint = () => Camera.main.WorldToScreenPoint(icon.transform.position);
        _menu.GetComponent<ShrinkToPointAnimation>().Show(null);
    }

    void IAddon.Update(double deltaTime)
    { }

    public string Serialize()
    {
        return PopUpText;
    }

    public void Deserialize(string data)
    {
        PopUpText = data;
    }
}
