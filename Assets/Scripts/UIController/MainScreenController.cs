using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScreenController : MonoBehaviour
{
    public UIController uiController;

    public void WorldMap()
    {
        uiController.SetActiveScreen(UIController.Screen.WorldMap);
    }

    public void Blueprints()
    {
        uiController.SetActiveScreen(UIController.Screen.Blueprints);
    }
    
    public void ControlStations()
    {
        uiController.SetActiveScreen(UIController.Screen.ControlStations);
    }

    public void Components()
    {
        uiController.SetActiveScreen(UIController.Screen.Components);
    }

    public void Scripts()
    {
        uiController.SetActiveScreen(UIController.Screen.Scripts);
    }

    public void Contracts()
    {
        uiController.SetActiveScreen(UIController.Screen.Contracts);
    }
}
