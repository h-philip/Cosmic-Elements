using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlStationsController : MonoBehaviour
{
    /// <summary>
    /// Diameter (in km) used to set stations' scales.
    /// </summary>
    public const float DIAMETER = 1f;

    public static GameObject s_ControlStationPrefab;
    public GameObject ControlStationPrefab;

    [Header("Stations List")]
    public RectTransform ListContent;
    public RectTransform ElementPrefab;
    public Color ActiveStationColor;

    [Header("Details View")]
    public RectTransform DetailsView;
    public RectTransform General;
    public RectTransform Resources;
    public RectTransform ComponentsContent;
    public RectTransform DockedSpaceshipsContent;
    public RectTransform BlueprintList;
    public RectTransform BlueprintListContent;
    public RectTransform BlueprintElementPrefab;

    [Header("Start New Mission")]
    public GameObject SpaceshipPrefab;

    private TextMeshProUGUI _generalContent, _generalLocation;
    private string _generalContentString, _generalLocationString;
    private TextMeshProUGUI _resourcesFoodWater, _resourcesIronCopper, _resourcesRareMinerals;
    private string _resourcesFoodWaterString, _resourcesIronCopperString, _resourcesRareMineralsString;

    private ControlStation[] _stations;
    public ControlStation SelectedStation { get; set; }

    private Color _defaultElementColor;

    private void Awake()
    {
        s_ControlStationPrefab = ControlStationPrefab;

        _generalContent = General.Find("Content").GetComponent<TextMeshProUGUI>();
        _generalContentString = _generalContent.text;
        _generalLocation = General.Find("Location").GetComponent<TextMeshProUGUI>();
        _generalLocationString = _generalLocation.text;

        Transform resourcesContent = Resources.Find("Content");
        _resourcesFoodWater = resourcesContent.Find("Food Water").GetComponent<TextMeshProUGUI>();
        _resourcesFoodWaterString = _resourcesFoodWater.text;
        _resourcesIronCopper = resourcesContent.Find("Iron Copper").GetComponent<TextMeshProUGUI>();
        _resourcesIronCopperString = _resourcesIronCopper.text;
        _resourcesRareMinerals = resourcesContent.Find("RareMinerals").GetComponent<TextMeshProUGUI>();
        _resourcesRareMineralsString = _resourcesRareMinerals.text;

        _defaultElementColor = ElementPrefab.GetComponent<Image>().color;
    }

    private void OnEnable()
    {
        _stations = ControlStation.GetAllControlStations(true);
        foreach (ControlStation station in _stations)
        {
            RectTransform element = Instantiate(ElementPrefab, ListContent);
            element.GetComponentInChildren<TextMeshProUGUI>().text = station.Location;
            element.name = station.Location;
            element.GetComponent<Button>().onClick.AddListener(() => SelectControlStation(station));
        }
        if (SelectedStation != null)
            SelectControlStation(SelectedStation);
        else
            DetailsView.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        foreach (Transform child in ListContent)
            Destroy(child.gameObject);
        DetailsView.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Update texts of Details View
        if (DetailsView.gameObject.activeInHierarchy)
        {
            _generalContent.text = string.Format(_generalContentString, TopBarController.FormatNumber(SelectedStation.Energy), SelectedStation.Population, -1); // TODO: Comm
            _generalLocation.text = string.Format(_generalLocationString, SelectedStation.Location);
            _resourcesFoodWater.text = string.Format(_resourcesFoodWaterString, SelectedStation.Food, SelectedStation.Water);
            _resourcesIronCopper.text = string.Format(_resourcesIronCopperString, SelectedStation.Iron, SelectedStation.Copper);
            _resourcesRareMinerals.text = string.Format(_resourcesRareMineralsString, SelectedStation.RareMinerals);
        }
    }

    public void SelectControlStation(ControlStation station)
    {
        DetailsView.gameObject.SetActive(true);
        SelectedStation = station;

        // Select list element colors
        foreach (RectTransform element in ListContent)
        {
            element.GetComponent<Image>().color = _defaultElementColor;
        }
        ListContent.Find(station.Location).GetComponent<Image>().color = ActiveStationColor;
    }

    public void StartNewMissionButton()
    {
        foreach (Transform child in BlueprintListContent.transform)
            Destroy(child.gameObject);
        foreach (Blueprint blueprint in FindObjectOfType<BlueprintsController>(true).Blueprints.Values)
        {
            Transform element = Instantiate(BlueprintElementPrefab, BlueprintListContent.transform);
            element.name = blueprint.Name;
            element.GetComponent<TextMeshProUGUI>().text = blueprint.Name;
            if (SelectedStation.HasResources(blueprint))
                element.GetComponent<Button>().onClick.AddListener(() =>
                {
                    BlueprintList.gameObject.SetActive(false);
                    StartNewMission(blueprint);
                });
            else
                element.GetComponent<Button>().interactable = false;
        }
        BlueprintList.gameObject.SetActive(true);
    }

    public void StartNewMission(Blueprint blueprint)
    {
        if (!SelectedStation.HasResources(blueprint))
            return;

        SelectedStation.Iron -= blueprint.Iron;
        SelectedStation.Copper -= blueprint.Copper;
        SelectedStation.RareMinerals -= blueprint.RareMinerals;
        SelectedStation.Energy -= blueprint.Energy;

        Spaceship ship = blueprint.CreateSpaceship(SpaceshipPrefab);
        ship.transform.parent = SelectedStation.transform.parent;
        CelestialBody shipBody = ship.GetComponent<CelestialBody>();
        CelestialBody stationBody = SelectedStation.GetComponent<CelestialBody>();
        shipBody.AttractorMass = stationBody.AttractorMass;
        shipBody.Attractor = stationBody.Attractor;
        shipBody.Radius = Spaceship.DIAMETER / 2;
        shipBody.Scale = stationBody.Scale;
        shipBody.KeplerOrbitData = new SimpleKeplerOrbits.KeplerOrbitData(stationBody.KeplerOrbitData.Position + new SimpleKeplerOrbits.Vector3d(1, 1, 1), stationBody.KeplerOrbitData.Velocity, stationBody.KeplerOrbitData.AttractorMass, stationBody.KeplerOrbitData.GravConst);
        foreach (Component component in ship.Components)
            component.Install();
        ship.PlayerControlled = true;
    }

    public void NewGame()
    {
        SpaceSimulation simulation = FindObjectOfType<SpaceSimulation>();

        // Earth Control Station

        ControlStation earthControlStation = CreateNewControlStation("Earth Control Station");
        earthControlStation.transform.parent = simulation.transform;
        CelestialBody body = earthControlStation.GetComponent<CelestialBody>();
        body.A = 0.0002818489;
        body.AttractorMass = 5.9722e+24;
        body.Radius = DIAMETER / 2;
        body.Attractor = simulation.transform.Find("Earth");
        body.Scale = simulation.DistanceScale;
        earthControlStation.Population = 1;
        earthControlStation.Food = 10;
        earthControlStation.Water = 10;
        List<Component> components = new List<Component>();
        components.Add(new BasicStructure(earthControlStation));
        components.Add(new SmallSolarPanel(earthControlStation));
        foreach (var component in components)
            component.Install();
        earthControlStation.Components = components.ToArray();
    }

    public static ControlStation CreateNewControlStation(string name)
    {
        GameObject stationObject = Instantiate(s_ControlStationPrefab);
        stationObject.name = name;

        // CelestialBody
        if (stationObject.GetComponent<CelestialBody>() == null)
            stationObject.AddComponent<CelestialBody>();

        // OrbitLineDisplay
        if (stationObject.GetComponent<OrbitLineDisplay>() == null)
            stationObject.AddComponent<OrbitLineDisplay>();

        // ControlStation
        ControlStation controlStation;
        if (!stationObject.TryGetComponent<ControlStation>(out controlStation))
            controlStation = stationObject.AddComponent<ControlStation>();
        controlStation.PlayerControlled = true;

        // UiIcon
        UiIcon uiIcon;
        if (!stationObject.TryGetComponent(out uiIcon))
            uiIcon = stationObject.AddComponent<UiIcon>();
        uiIcon.IconPrefab = UnityEngine.Resources.Load<RectTransform>("Icons/Icon");
        uiIcon.TextPrefab = UnityEngine.Resources.Load<RectTransform>("Icons/Text");
        uiIcon.Window = FindObjectOfType<WorldMapController>(true).transform.Find("IconWindow") as RectTransform;

        return controlStation;
    }
}
