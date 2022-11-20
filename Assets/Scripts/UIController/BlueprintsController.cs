using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BlueprintsController : MonoBehaviour
{
    [Header("Top Bar")]
    public TextMeshProUGUI BlueprintName;
    public TextMeshProUGUI EnergyCost;
    public TextMeshProUGUI IronCost;
    public TextMeshProUGUI CopperCost;
    public TextMeshProUGUI RareMineralsCost;
    // Private
    private string _energyTemplate, _ironTemplate, _copperTemplate, _rareMineralsTemplate;

    [Header("Blueprints List")]
    public RectTransform Content;
    public GameObject ElementPrefab;
    public Color ActiveBlueprintColor;

    [Header("New Blueprint Dialogue")]
    public GameObject NewBlueprintDialogueWindow;
    public TMP_InputField NewBlueprintText;
    public Button CreateButton;

    [Header("Editor")]
    public RectTransform Editor;
    public TMP_InputField Description;
    public TMP_Dropdown ScriptDropdown;
    public RectTransform InstalledContent;
    public RectTransform InstalledComponentPrefab;
    public RectTransform AvailableContent;
    public RectTransform AvailableComponentPrefab;
    // Private
    private Blueprint _openBlueprint;
    private Color _defaultElementColor;
    private Dictionary<string, Component.ComponentInfo> _availableComponents => FindObjectOfType<ComponentsController>(true).UnlockedComponents;
    private Dictionary<string, Type> _controlScripts => FindObjectOfType<ScriptsController>(true).ControlScripts;

    // Properties
    public Dictionary<string, Blueprint> Blueprints { get; private set; }

    private void Awake()
    {
        _energyTemplate = EnergyCost.text;
        _ironTemplate = IronCost.text;
        _copperTemplate = CopperCost.text;
        _rareMineralsTemplate = RareMineralsCost.text;

        _defaultElementColor = ElementPrefab.GetComponent<Image>().color;
        Blueprints = new Dictionary<string, Blueprint>();

        NewBlueprintText.onSubmit.AddListener(_ => CreateNewBlueprint());
    }

    private void OnEnable()
    {
        // Load available components
        if (_availableComponents != null)
        {
            foreach (string component in _availableComponents.Keys)
            {
                if (component == typeof(BasicStructure).ToString())
                    continue;
                RectTransform element = Instantiate(AvailableComponentPrefab, AvailableContent);
                element.name = component;
                element.GetComponentInChildren<TextMeshProUGUI>().text = component;
                element.GetComponent<Button>().onClick.AddListener(() => AddComponent(component));
            }
        }

        // Load available control scripts
        if (_controlScripts != null)
        {
            string[] options = new string[_controlScripts.Count];
            _controlScripts.Keys.CopyTo(options, 0);
            ScriptDropdown.AddOptions(new List<string>(options));
        }

        // Reload blueprints list
        foreach (string name in Blueprints.Keys)
            AddBlueprintToList(name);

        if (_openBlueprint != null)
            OpenBlueprint(_openBlueprint.Name);
        Editor.gameObject.SetActive(_openBlueprint != null);
    }

    private void OnDisable()
    {
        foreach (Transform child in Content.transform)
            Destroy(child.gameObject);
        foreach (Transform child in AvailableContent.transform)
            Destroy(child.gameObject);
        ScriptDropdown.ClearOptions();
    }

    // Content List

    private void AddBlueprintToList(string name)
    {
        GameObject go = Instantiate(ElementPrefab, Content.transform);
        go.name = name;
        go.GetComponentInChildren<TextMeshProUGUI>().text = name;
        go.GetComponent<Button>().onClick.AddListener(() => OpenBlueprint(name));
    }

    public void LoadBlueprints(Blueprint[] blueprints)
    {
        foreach (Blueprint bp in blueprints)
        {
            if (Blueprints.ContainsKey(bp.Name))
                continue;
            Blueprints.Add(bp.Name, bp);
        }
    }

    // Editor

    public void OpenBlueprint(string name)
    {
        // Set list element colors
        if (_openBlueprint != null)
        {
            Content.transform.Find(_openBlueprint.Name).GetComponent<Image>().color = _defaultElementColor;
            foreach (Transform child in InstalledContent.transform)
            {
                Destroy(child.gameObject);
            }
        }
        Content.transform.Find(name).GetComponent<Image>().color = ActiveBlueprintColor;

        // Try to get blueprint
        Blueprint blueprint;
        if (!Blueprints.TryGetValue(name, out blueprint))
        {
            Editor.gameObject.SetActive(false);
            _openBlueprint = null;
        }

        // Components
        foreach (string component in blueprint.Components)
        {
            AddComponent(component, false, false);
        }

        // Control script and description
        ScriptDropdown.SetValueWithoutNotify(ScriptDropdown.options.FindIndex(new Predicate<TMP_Dropdown.OptionData>(_ => _.text == blueprint.ControlScript)));
        Description.SetTextWithoutNotify(blueprint.Description);

        // Update top bar and activate editor
        _openBlueprint = blueprint;
        UpdateTopBar();
        Editor.gameObject.SetActive(true);
    }

    private void AddComponent(string component, bool updateTopBar = true, bool addToBlueprint = true)
    {
        Component.ComponentInfo info;
        if (!_availableComponents.TryGetValue(component, out info)
            || addToBlueprint ? !_openBlueprint.TryAddComponent(component) : false)
            throw new ArgumentException($"Can't add component '{component}.'");
        RectTransform element = Instantiate(InstalledComponentPrefab, InstalledContent);
        element.name = component;
        element.Find("Name").GetComponent<TextMeshProUGUI>().text = info.Name;
        element.Find("Energy").GetComponent<TextMeshProUGUI>().text = TopBarController.FormatNumber(info.Energy);
        element.Find("Iron").GetComponent<TextMeshProUGUI>().text = info.Iron.ToString();
        element.Find("Copper").GetComponent<TextMeshProUGUI>().text = info.Copper.ToString();
        element.Find("RareMinerals").GetComponent<TextMeshProUGUI>().text = info.RareMinerals.ToString();
        if (component == typeof(BasicStructure).ToString())
            element.GetComponent<Button>().interactable = false;
        else
            element.GetComponent<Button>().onClick.AddListener(() => RemoveComponent(element));
        if (updateTopBar)
            UpdateTopBar();
    }

    private void RemoveComponent(RectTransform element)
    {
        Component.ComponentInfo info;
        if (element.parent != InstalledContent
            || !_availableComponents.TryGetValue(element.name, out info)
            || !_openBlueprint.TryRemoveComponent(info.Name))
            return;
        Destroy(element.gameObject);
        UpdateTopBar();
    }

    private void UpdateTopBar()
    {
        // Name
        BlueprintName.text = _openBlueprint.Name;
        EnergyCost.text = string.Format(_energyTemplate, TopBarController.FormatNumber(_openBlueprint.Energy));
        IronCost.text = string.Format(_ironTemplate, _openBlueprint.Iron);
        CopperCost.text = string.Format(_copperTemplate, _openBlueprint.Copper);
        RareMineralsCost.text = string.Format(_rareMineralsTemplate, _openBlueprint.RareMinerals);
    }

    public void DescriptionChanged()
    {
        _openBlueprint.Description = Description.text;
    }

    public void ControlScriptChanged()
    {
        _openBlueprint.TrySetControlScript(ScriptDropdown.options[ScriptDropdown.value].text);
    }

    // New Blueprint

    public void OpenNewBlueprintDialogue()
    {
        NewBlueprintText.text = "";
        NewBlueprintDialogueWindow.SetActive(true);
        NewBlueprintText.Select();
    }

    public void CreateNewBlueprint()
    {
        if (!ValidateNewBlueprintName())
            return;
        string name = NewBlueprintText.text;
        NewBlueprintText.text = "";
        NewBlueprintDialogueWindow.SetActive(false);
        _openBlueprint = new Blueprint(name);
        _openBlueprint.TryAddComponent(typeof(BasicStructure).ToString());
        Blueprints.Add(name, _openBlueprint);
        AddBlueprintToList(name);
        OpenBlueprint(name);
    }

    public void NewBlueprintNameChanged()
    {
        CreateButton.interactable = ValidateNewBlueprintName();
    }

    public bool ValidateNewBlueprintName()
    {
        string name = NewBlueprintText.text.Trim();
        return name != "" && !Blueprints.ContainsKey(name);
    }
}
