using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum Screen
    {
        WorldMap,
        Components,
        Blueprints,
        Scripts,
        ControlStations,
        Contracts,
        MainScreen
    }

    private Screen _activeScreen = Screen.MainScreen;
    private Dictionary<Screen, GameObject> _screenMap = new Dictionary<Screen, GameObject>();
    private SpaceSimulation _simulation;

    private void Start()
    {
        _screenMap[Screen.WorldMap] = transform.Find("World Map").gameObject;
        _screenMap[Screen.Components] = transform.Find("Components").gameObject;
        _screenMap[Screen.Blueprints] = transform.Find("Blueprints").gameObject;
        _screenMap[Screen.Scripts] = transform.Find("Scripts").gameObject;
        _screenMap[Screen.ControlStations] = transform.Find("Control Stations").gameObject;
        _screenMap[Screen.Contracts] = transform.Find("Contracts").gameObject;
        _screenMap[Screen.MainScreen] = transform.Find("Main Screen").gameObject;

        // Make sure every screen was active once but end with only _activeScreen being active
        foreach (var pair in _screenMap)
        {
            pair.Value.SetActive(true);
            if (_activeScreen != pair.Key)
                pair.Value.SetActive(false);
        }

        _simulation = FindObjectOfType<SpaceSimulation>();
    }

    public void SetActiveScreen(Screen screen)
    {
        _screenMap[_activeScreen].SetActive(false);
        _screenMap[screen].SetActive(true);
        _activeScreen = screen;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _simulation.PointerOverUi = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _simulation.PointerOverUi = false;
    }
}
