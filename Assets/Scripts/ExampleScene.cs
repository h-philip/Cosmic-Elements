using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleScene : MonoBehaviour
{
    public const double SCALE = 0.1;

    private RectTransform _iconPrefab, _iconTextPrefab;
    private RectTransform _popUpMenuPrefab;
    private UnityEngine.UI.Button _popButtonPrefab;
    private TMPro.TextMeshProUGUI _popTextPrefab;

//#if UNITY_EDITOR
    private void Awake()
    {
        _iconPrefab = Resources.Load<RectTransform>("Icons/Icon");
        _iconTextPrefab = Resources.Load<RectTransform>("Icons/Text");
        _popUpMenuPrefab = Resources.Load<RectTransform>("PopUpMenu/PopUpMenu");
        _popButtonPrefab = Resources.Load<UnityEngine.UI.Button>("PopUpMenu/Button");
        _popTextPrefab = Resources.Load<TMPro.TextMeshProUGUI>("PopUpMenu/Text");
    }

    private void Start()
    {
        // Earth Control Station
        GameObject earthControlStation = GameObject.CreatePrimitive(PrimitiveType.Cube);
        earthControlStation.transform.parent = gameObject.transform;
        earthControlStation.name = "Earth Control Station";
        CelestialBody body = earthControlStation.AddComponent<CelestialBody>();
        body.A = 0.0002818489;
        body.AttractorMass = 5.9722e+24;
        body.Radius = 1;
        body.Attractor = gameObject.transform.Find("Earth");
        body.Scale = SCALE;
        ControlStation controlStation = earthControlStation.AddComponent<ControlStation>();
        controlStation.Population = 1;
        controlStation.Food = 10;
        controlStation.Water = 10;
        List<Component> components = new List<Component>();
        components.Add(new BasicStructure(controlStation));
        components.Add(new SmallSolarPanel(controlStation));
        controlStation.Components = components.ToArray();
        controlStation.PlayerControlled = true;
        UiIcon icon = earthControlStation.AddComponent<UiIcon>();
        icon.IconPrefab = _iconPrefab;
        icon.TextPrefab = _iconTextPrefab;
        icon.Window = FindObjectOfType<WorldMapController>(true).transform.Find("IconWindow") as RectTransform;

        //yield return new WaitForSeconds(1);

        //// TODO: Remove
        //Contracts.EcIntroduction ecIntroduction = new Contracts.EcIntroduction();
        //FindObjectOfType<ContractsController>(true).NewContract(ecIntroduction);
        //FindObjectOfType<ContractsController>(true).StartContract(ecIntroduction);
        //Blueprint bp = new Blueprint("EC1 Test", "", new string[] { typeof(BasicStructure).ToString(), typeof(BasicEngine).ToString(), typeof(SmallSolarPanel).ToString(), typeof(SmallSolarPanel).ToString() }, Contracts.EcIntroduction.SCRIPT_NAME);
        //FindObjectOfType<BlueprintsController>(true).Blueprints.Add(bp.Name, bp);
        //FindObjectOfType<ControlStationsController>(true).SelectedStation = controlStation;
        //FindObjectOfType<ControlStationsController>(true).StartNewMission(bp);
    }
//#endif
}
