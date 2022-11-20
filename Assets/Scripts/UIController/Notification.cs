using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class Notification : MonoBehaviour, IPointerClickHandler
{
    public string Title;
    public string Message;
    public UnityAction OnClick;
    public UnityAction<Notification> OnUpdate;

    private TextMeshProUGUI _title, _message;

    private void Start()
    {
        _title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        _title.text = Title;
        _message = transform.Find("Message").GetComponent<TextMeshProUGUI>();
        _message.text = Message;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (OnClick != null)
                OnClick();
            Destroy(gameObject);
            FindObjectOfType<NotificationsController>().ToggleList();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Destroy(gameObject);
        }
    }

    public void Update()
    {
        if (OnUpdate != null)
        {
            OnUpdate(this);
            _title.text = Title;
            _message.text = Message;
        }
    }
}
