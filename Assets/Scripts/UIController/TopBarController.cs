using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Globalization;

public class TopBarController : MonoBehaviour
{
    public TextMeshProUGUI ControlStations, Spaceships, TotalEnergy, DiscoveredStars, TotalPopulation;
    private string _controlStationsTemplate, _spaceshipsTemplate, _totalEnergyTemplate, _discoveredStarsTemplate, _totalPopulationTemplate;

    private SpaceSimulation _simulation;
    private GameMenuController _gameMenu;
    private UIController _uiController;
    private NotificationsController _notificationsController;

    private static Dictionary<double, string> s_metricPrefix = new Dictionary<double, string>();

    private void Awake()
    {
        s_metricPrefix[1e24d] = "Y";
        s_metricPrefix[1e21d] = "Z";
        s_metricPrefix[1e18d] = "E";
        s_metricPrefix[1e15d] = "P";
        s_metricPrefix[1e12d] = "T";
        s_metricPrefix[1e9d] = "G";
        s_metricPrefix[1e6d] = "M";
        s_metricPrefix[1e3d] = "k";
        s_metricPrefix[1e0d] = "";

        _gameMenu = FindObjectOfType<GameMenuController>(true);
        _uiController = FindObjectOfType<UIController>(true);
        _notificationsController = FindObjectOfType<NotificationsController>(true);
        _simulation = FindObjectOfType<SpaceSimulation>();

        _controlStationsTemplate = ControlStations.text;
        _spaceshipsTemplate = Spaceships.text;
        _totalEnergyTemplate = TotalEnergy.text;
        _discoveredStarsTemplate = DiscoveredStars.text;
        _totalPopulationTemplate = TotalPopulation.text;
    }

    private void Update()
    {
        // Update info shown in top
        ControlStation[] controlStations = ControlStation.GetAllControlStations(true);
        ControlStations.text = string.Format(_controlStationsTemplate, FormatNumber(controlStations.Length));
        Spaceship[] spaceships = Spaceship.GetAllSpaceships(true);
        Spaceships.text = string.Format(_spaceshipsTemplate, FormatNumber(spaceships.Length));
        double totalEnergy = 0;
        int totalPopulation = 0;
        foreach (ControlStation station in controlStations)
        {
            totalPopulation += station.Population;
            totalEnergy += station.Energy;
        }
        foreach (Spaceship ship in spaceships)
            totalEnergy += ship.Energy;
        TotalEnergy.text = string.Format(_totalEnergyTemplate, FormatNumber(totalEnergy));
        TotalPopulation.text = string.Format(_totalPopulationTemplate, FormatNumber(totalPopulation));
        Star[] stars = FindObjectsOfType<Star>(true);
        int discoveredStars = 0;
        foreach (Star star in stars)
            if (star.IsDiscovered)
                discoveredStars++;
        DiscoveredStars.text = string.Format(_discoveredStarsTemplate, FormatNumber(discoveredStars));
    }

    public void GameMenu()
    {
        _gameMenu.gameObject.SetActive(true);
    }

    public void Notifications()
    {
        _notificationsController.ToggleList();
    }

    public void WorldMap()
    {
        _uiController.SetActiveScreen(UIController.Screen.WorldMap);
    }

    public void MainScreen()
    {
        _uiController.SetActiveScreen(UIController.Screen.MainScreen);
    }

    public static string FormatNumber(double number)
    {
        string s = number.ToString();
        foreach (var pair in s_metricPrefix)
        {
            double num = System.Math.Floor(number * 1e2 / pair.Key) / 1e2;
            s = num.ToString(pair.Value == "" ? "F0" : "F2", CultureInfo.InvariantCulture) + pair.Value;
            if (number > pair.Key)
                return s;
        }
        return s;
    }
}
