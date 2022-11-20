using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class WorldMapController : MonoBehaviour
{
    public RectTransform ListContent;
    public Color HightlightColor;
    public TextMeshProUGUI SpeedText;

    [Header("Categories")]
    public RectTransform ControlStationsCategory;
    public RectTransform ControlStationTemplate;
    public RectTransform SpaceshipsCategory;
    public RectTransform SpaceshipTemplate;
    public RectTransform StarsCategory;
    public RectTransform StarTemplate;
    public RectTransform OthersCategory;
    //public OtherElement m_OtherTemplate;

    private Dictionary<RectTransform, Color> _defaultColors = new Dictionary<RectTransform, Color>();
    private SpaceSimulation _simulation;
    private string _speedTextDefault;

    public delegate void ElementUpdate();

    private List<ElementUpdate> _elementUpdates = new List<ElementUpdate>();

    private void Awake()
    {
        _defaultColors.Add(ControlStationsCategory, ControlStationsCategory.GetComponent<Image>().color);
        _defaultColors.Add(SpaceshipsCategory, SpaceshipsCategory.GetComponent<Image>().color);
        _defaultColors.Add(StarsCategory, StarsCategory.GetComponent<Image>().color);
        _defaultColors.Add(OthersCategory, OthersCategory.GetComponent<Image>().color);

        _simulation = FindObjectOfType<SpaceSimulation>();
        _speedTextDefault = SpeedText.text;
    }

    private void OnEnable()
    {
        FindObjectOfType<PlayerInput>(true).ActivateInput();
        SetTimeScaleText(_simulation.TimeScale);
    }

    private void Update()
    {
        foreach (ElementUpdate elementUpdate in _elementUpdates)
            elementUpdate();
    }

    private void OnDisable()
    {
        try
        {
            FindObjectOfType<PlayerInput>(true).DeactivateInput();
        }
        catch (System.NullReferenceException)
        { }
    }

    void PrepareNewCategory(RectTransform category)
    {
        // Highlight correct category
        foreach (var pair in _defaultColors)
        {
            pair.Key.GetComponent<Image>().color = pair.Value;
        }
        category.GetComponent<Image>().color = HightlightColor;

        // Delete old elements
        foreach (Transform child in ListContent.transform)
            Destroy(child.gameObject);
        _elementUpdates = new List<ElementUpdate>();

        // Reset parent size
        ListContent.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
    }

    void ListControlStations()
    {
        PrepareNewCategory(ControlStationsCategory);

        ControlStation[] controlStations = FindObjectsOfType<ControlStation>();
        foreach (ControlStation controlStation in controlStations)
        {
            if (!controlStation.PlayerControlled)
                continue;
            RectTransform element = Instantiate(ControlStationTemplate, ListContent);
            element.name = controlStation.Location;
            // Texts
            TextMeshProUGUI location = element.Find("Location").GetComponent<TextMeshProUGUI>(),
                energy = element.Find("Energy").GetComponent<TextMeshProUGUI>();
            string energyTemplate = energy.text;
            location.text = controlStation.Location;
            // Texts that need to be updated
            _elementUpdates.Add(() => energy.text = string.Format(energyTemplate, TopBarController.FormatNumber(controlStation.Energy)));
            // Button listener
            element.Find("View").GetComponent<Button>().onClick.AddListener(() => FocusView(controlStation.GetComponent<CelestialBody>()));
            element.Find("Open").GetComponent<Button>().onClick.AddListener(() =>
            {
                FindObjectOfType<UIController>().SetActiveScreen(UIController.Screen.ControlStations);
                FindObjectOfType<ControlStationsController>().SelectControlStation(controlStation);
            });

        }
    }

    void ListSpaceships()
    {
        PrepareNewCategory(SpaceshipsCategory);

        Spaceship[] spaceships = Spaceship.GetAllSpaceships(true);
        foreach (Spaceship spaceship in spaceships)
        {
            RectTransform element = Instantiate(SpaceshipTemplate, ListContent);
            // Texts
            TextMeshProUGUI name = element.Find("Name").GetComponent<TextMeshProUGUI>(),
                location = element.Find("Location").GetComponent<TextMeshProUGUI>(),
                energy = element.Find("Energy").GetComponent<TextMeshProUGUI>(),
                communication = element.Find("Communication").GetComponent<TextMeshProUGUI>();
            string locationTemplate = location.text,
                energyTemplate = energy.text,
                communicationTemplate = communication.text;
            // Texts that need to be updated
            _elementUpdates.Add(() =>
            {
                element.name = spaceship.name;
                name.text = spaceship.name;
                location.text = string.Format(locationTemplate, spaceship.Location);
                energy.text = string.Format(energyTemplate, TopBarController.FormatNumber(spaceship.Energy));
                communication.text = string.Format(communicationTemplate, spaceship.Communication);
            });
            // Button listener
            element.Find("View").GetComponent<Button>().onClick.AddListener(() => FocusView(spaceship.GetComponent<CelestialBody>()));
            element.Find("Open").GetComponent<Button>().onClick.AddListener(() =>
            {
                Debug.Log("\"Open\" for Spaceship not implemented yet."); // TODO
            });
        }
    }

    void ListStars()
    {
        PrepareNewCategory(StarsCategory);

        Star[] stars = FindObjectsOfType<Star>();
        foreach (Star star in stars)
        {
            // Ignore the ones that aren't discovered yet
            if (!star.IsDiscovered)
                continue;

            RectTransform element = Instantiate(StarTemplate, ListContent);
            element.name = star.name;
            // Texts
            TextMeshProUGUI name = element.Find("Name").GetComponent<TextMeshProUGUI>();
            name.text = star.name;
            // Button listener
            element.Find("View").GetComponent<Button>().onClick.AddListener(() => FocusView(star.GetComponent<CelestialBody>()));
        }
    }

    void ListOthers()
    {
        PrepareNewCategory(OthersCategory);

        // TODO
    }

    public void ChangeCategory(RectTransform category)
    {
        if (category == ControlStationsCategory)
            ListControlStations();
        else if (category == SpaceshipsCategory)
            ListSpaceships();
        else if  (category == StarsCategory)
            ListStars();
        else if (category == OthersCategory)
            ListOthers();
        else
            Debug.LogWarning("Unknow category: " + category);
    }

    public void FocusView(CelestialBody body)
    {
        _simulation.ReferenceObject = body;
    }

    public void SetTimeScaleText(double timeScale)
    {
        SpeedText.text = string.Format(_speedTextDefault, timeScale);
    }
}
