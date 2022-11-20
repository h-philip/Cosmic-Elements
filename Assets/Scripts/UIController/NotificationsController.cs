using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class NotificationsController : MonoBehaviour
{
    [Header("Top Bar")]
    public TextMeshProUGUI Counter;

    [Header("Expandable")]
    public RectTransform ListContent;
    public Notification NotificationTemplate;

    private VerticalSplitAnimation _list;
    private bool _listVisible;

    private void Awake()
    {
        _list = transform.GetComponentInChildren<VerticalSplitAnimation>(true);
        _listVisible = _list.gameObject.activeSelf;
    }

    private void Update()
    {
        if (ListContent.transform.childCount == 0)
        {
            Counter.gameObject.SetActive(false);
        }
        else
        {
            Counter.gameObject.SetActive(true);
            Counter.text = ListContent.transform.childCount.ToString();
        }
        if (!_list.gameObject.activeSelf)
            System.Array.ForEach(ListContent.GetComponentsInChildren<Notification>(), _ =>
            {
                if (_.OnUpdate != null)
                    _.OnUpdate(_);
            });
    }

    public Notification NewNotification(string title, string message, UnityAction onClick = null, UnityAction<Notification> onUpdate = null)
    {
        Notification notification = Instantiate(NotificationTemplate, ListContent);
        notification.name = title;
        notification.Title = title;
        notification.Message = message;
        notification.OnClick = onClick;
        notification.OnUpdate = onUpdate;
        return notification;
    }

    public void RemoveNotification(Notification notification)
    {
        if (notification.transform.parent != ListContent)
            return;
        Destroy(notification.gameObject);
        ListContent.ForceUpdateRectTransforms();
    }

    public void ToggleList()
    {
        _listVisible = !_listVisible;

        if (_listVisible)
        {
            _list.gameObject.SetActive(true);
            _list.Show(null);
        }
        else
        {
            _list.gameObject.SetActive(true);
            _list.Hide(_ =>
            {
                _list.gameObject.SetActive(false);
            });
        }
    }
}
