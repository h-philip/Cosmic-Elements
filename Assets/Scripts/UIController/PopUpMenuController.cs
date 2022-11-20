using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PopUpMenuController : MonoBehaviour
{
    private class TopBarController : MonoBehaviour, IDragHandler
    {
        private bool _pointerDown;
        private RectTransform _parent;

        private void Awake()
        {
            _parent = transform.parent.GetComponent<RectTransform>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 position = _parent.position;
            position.x += eventData.delta.x;
            position.y += eventData.delta.y;
            _parent.position = position;
        }
    }

    private RectTransform _topBar;

    private void Awake()
    {
        _topBar = transform.Find("TopBar") as RectTransform;
        _topBar.gameObject.AddComponent<TopBarController>();
    }

    public void Close()
    {
        ShrinkToPointAnimation animation = GetComponent<ShrinkToPointAnimation>();
        if (animation == null)
            Destroy(gameObject);
        else
            animation.Hide(_ => Destroy(_.gameObject));

    }
}
