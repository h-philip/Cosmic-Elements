using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ClickSound : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public AudioSource DownSound;
    public AudioSource UpSound;

    private Button _button;
    private bool _isDown = false;

    private void Start()
    {
        _button = GetComponent<Button>();
    }

    private bool canClick()
    {
        if (_button != null)
        {
            return _button.isActiveAndEnabled;
        }
        else
        {
            return isActiveAndEnabled;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (canClick())
        {
            DownSound.Play();
            _isDown = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isDown)
        {
            UpSound.Play();
            _isDown = false;
        }
    }
}
