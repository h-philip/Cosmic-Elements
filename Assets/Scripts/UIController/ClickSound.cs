using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickSound : MonoBehaviour, IPointerDownHandler
{
    // Public
    public AudioSource DownSound;
    public bool ApplyToButton = true;
    public bool ApplyToIPointerClickHandler = false;
    public bool ApplyToIPointerDownHandler = false;

    // Private
    private bool _isChild = false;
    private Button _button;

    private void Start()
    {
        if (!_isChild)
        {
            IterateAllChildren();
            if (!(ApplyToButton && GetComponent<Button>() != null) &&
                !(ApplyToIPointerClickHandler && GetComponent<IPointerClickHandler>() != null) &&
                !(ApplyToIPointerDownHandler && GetComponent<IPointerDownHandler>() != null))
            {
                Destroy(this);
                return;
            }
        }

        _button = GetComponent<Button>();
    }

    private void IterateAllChildren()
    {
        Queue<Transform> children = new Queue<Transform>();
        children.Enqueue(transform);

        while (children.Count > 0)
        {
            Transform child = children.Dequeue();
            if ((ApplyToButton && child.GetComponent<Button>() != null) ||
                (ApplyToIPointerClickHandler && child.GetComponent<IPointerClickHandler>() != null) ||
                (ApplyToIPointerDownHandler && child.GetComponent<IPointerDownHandler>() != null))
            {
                ClickSound component = child.gameObject.AddComponent<ClickSound>();
                component.DownSound = DownSound;
                component._isChild = true;
            }
            for (int i = 0; i < child.childCount; i++)
                children.Enqueue(child.GetChild(i));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (DownSound != null && isActiveAndEnabled && (_button != null ? _button.interactable : true))
        {
            DownSound.Play();
        }
    }
}
