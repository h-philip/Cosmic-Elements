using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Components;

public class ComponentsController : MonoBehaviour
{
    // Public fields
    public RectTransform ComponentsList;
    public RectTransform ListElementPrefab;
    public RectTransform DetailsView;
    public Color ActiveComponentColor;


    // Private fields
    private Dictionary<TextMeshProUGUI, string> _startStrings;
    private Color _defaultElementColor;

    // Public properties
    public Dictionary<string, Component.ComponentInfo> UnlockedComponents { get; private set; }

    private void Awake()
    {
        UnlockedComponents = new Dictionary<string, Component.ComponentInfo>();
        _startStrings = new();
        DetailsView.gameObject.SetActive(false);
        _defaultElementColor = ListElementPrefab.GetComponent<Image>().color;
    }

    // Details View

    private void OpenComponent(Component.ComponentInfo info)
    {
        TextMeshProUGUI text;

        // General
        RectTransform general = DetailsView.Find("General") as RectTransform;
        Dictionary<string, double> doubleValues = new();
        doubleValues["Energy Consumption"] = info.EnergyConsumption;
        doubleValues["Mass"] = info.Mass;
        doubleValues["Energy Cost"] = info.Energy;
        doubleValues["Iron Cost"] = info.Iron;
        doubleValues["Copper Cost"] = info.Copper;
        doubleValues["RareMinerals Cost"] = info.RareMinerals;
        foreach (var pair in doubleValues)
        {
            text = general.Find(pair.Key).GetComponent<TextMeshProUGUI>();
            if (!_startStrings.ContainsKey(text))
                _startStrings.Add(text, text.text);
            else
                text.SetText(_startStrings[text], false);
            text.text = string.Format(text.text, TopBarController.FormatNumber(pair.Value));
        }

        // Short Description
        RectTransform shortDescription = DetailsView.Find("Short Description") as RectTransform;
        Dictionary<string, string> stringValues = new();
        stringValues["Name"] = info.Name;
        stringValues["Description"] = info.ShortDescription;
        foreach (var pair in stringValues)
            shortDescription.Find(pair.Key).GetComponentInChildren<TextMeshProUGUI>().text = pair.Value;

        // Long Description
        RectTransform longDescription = DetailsView.Find("Long Description") as RectTransform;
        longDescription.GetComponentInChildren<TextMeshProUGUI>().text = info.LongDescription;

        DetailsView.gameObject.SetActive(true);
    }

    // List
    private RectTransform AddElementToList(Component.ComponentInfo info)
    {
        RectTransform element = Instantiate(ListElementPrefab, ComponentsList);
        element.name = info.Name;
        element.GetComponentInChildren<TextMeshProUGUI>().text = info.Name;
        element.GetComponent<Button>().onClick.AddListener(() => SelectComponentFromList(element));
        return element;
    }

    private bool RemoveElementFromList(string name)
    {
        foreach (RectTransform child in ComponentsList)
            if (child.name == name)
            {
                Destroy(child.gameObject);
                return true;
            }
        return false;
    }

    public void SelectComponentFromList(RectTransform element)
    {
        foreach (RectTransform child in ComponentsList)
        {
            Image image;
            if (child.TryGetComponent(out image))
                image.color = _defaultElementColor;
        }
        element.GetComponent<Image>().color = ActiveComponentColor;

        OpenComponent(UnlockedComponents[element.name]);
    }

    // Loading / Locking / Unlocking

    public void LoadComponents(string[] components)
    {
        foreach (string name in components)
            UnlockComponent(name);
    }

    public void UnlockComponent(string name)
    {
        if (UnlockedComponents.ContainsKey(name))
            return;
        Type type = Type.GetType(name);
        if (type == null || !type.IsSubclassOf(typeof(Component)))
        {
            Debug.LogWarning("Unknown component type: " + name);
            return;
        }
        object[] pars = { null };
        Component component = Activator.CreateInstance(type, pars) as Component;
        Component.ComponentInfo info = new Component.ComponentInfo()
        {
            Type = type,
            Name = name,
            EnergyConsumption = component.EnergyConsumption,
            Mass = component.Mass,
            Energy = component.Energy,
            Iron = component.Iron,
            Copper = component.Copper,
            RareMinerals = component.RareMinerals,
            ShortDescription = component.ShortDescription,
            LongDescription = component.LongDescription,
        };
        UnlockedComponents.Add(name, info);
        AddElementToList(info);
    }

    public void LockComponent(string name)
    {
        UnlockedComponents.Remove(name);
        RemoveElementFromList(name);
    }

    public void NewGame()
    {
        string[] components =
        {
            "BasicStructure", "SmallSolarPanel"
        };
        LoadComponents(components);
    }
}
