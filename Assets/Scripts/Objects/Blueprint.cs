using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Blueprint
{
    // Public
    public string Name;
    public string Description;

    // Private
    [SerializeField]
    private List<string> _components;
    [SerializeField]
    private string _controlScript;

    // Properties
    public List<string> Components { get => _components; private set => _components = value; }
    public string ControlScript { get => _controlScript; private set => _controlScript = value; }

    // Private
    private ComponentsController _componentsController => GameObject.FindObjectOfType<ComponentsController>(true);

    // Properties
    public double Energy
    {
        get
        {
            double energy = 0;
            foreach (string component in Components)
                energy += _componentsController.UnlockedComponents[component].Energy;
            return energy;
        }
    }

    public int Iron
    {
        get
        {
            int iron = 0;
            foreach (string component in Components)
                iron += _componentsController.UnlockedComponents[component].Iron;
            return iron;
        }
    }

    public int Copper
    {
        get
        {
            int copper = 0;
            foreach (string component in Components)
                copper += _componentsController.UnlockedComponents[component].Copper;
            return copper;
        }
    }

    public int RareMinerals
    {
        get
        {
            int rareMinerals = 0;
            foreach (string component in Components)
                rareMinerals += _componentsController.UnlockedComponents[component].RareMinerals;
            return rareMinerals;
        }
    }

    // Methods
    public Blueprint(string name, string description = "", string[] components = null, string controlScript = null)
    {
        Name = name;
        Description = description;
        Components = components != null ? new List<string>(components) : new List<string>();
        if (controlScript != null && !TrySetControlScript(controlScript))
            throw new ArgumentException("controlScript is invalid");
        ControlScript = controlScript;
    }

    public bool TryAddComponent(string component)
    {
        Type type = Type.GetType(component);
        bool valid = type != null
            && type.IsSubclassOf(typeof(Component))
            && !type.IsAbstract;
        if (valid)
            Components.Add(component);
        return valid;
    }

    public bool TryRemoveComponent(string component)
    {
        return Components.Remove(component);
    }

    public bool TrySetControlScript(string script)
    {
        Type type = GameObject.FindObjectOfType<ScriptsController>(true).ControlScripts.GetValueOrDefault(script, null);
        bool valid = type != null
            && type.IsSubclassOf(typeof(Components.ControlScript))
            && !type.IsAbstract;
        if (valid)
            ControlScript = script;
        return valid;
    }

    public void ResetControlScript()
    {
        ControlScript = null;
    }

    public Spaceship CreateSpaceship(GameObject original)
    {
        // GameObject
        GameObject shipObject = GameObject.Instantiate(original);
        shipObject.name = Name;

        // Size
        // TODO

        // CelestialBody
        if (shipObject.GetComponent<CelestialBody>() == null)
            shipObject.AddComponent<CelestialBody>();

        // OrbitLineDisplay
        if (shipObject.GetComponent<OrbitLineDisplay>() == null)
            shipObject.AddComponent<OrbitLineDisplay>();

        // Spaceship
        Spaceship spaceship;
        if (!shipObject.TryGetComponent<Spaceship>(out spaceship))
            spaceship = shipObject.AddComponent<Spaceship>();
        // Components
        Component[] components = new Component[Components.Count];
        object[] pars = { spaceship };
        for (int i = 0; i < Components.Count; i++)
        {
            components[i] = Activator.CreateInstance(Type.GetType(Components[i]), pars) as Component;
            components[i].Install();
        }
        spaceship.Components = components;
        // ControlScript
        if (!string.IsNullOrEmpty(ControlScript))
        {
            spaceship.Script = GameObject.FindObjectOfType<ScriptsController>(true).GetNewScript(ControlScript);
            spaceship.Script.Spaceship = spaceship;
        }

        // UiIcon
        UiIcon uiIcon;
        if (!shipObject.TryGetComponent(out uiIcon))
            uiIcon = shipObject.AddComponent<UiIcon>();
        uiIcon.IconPrefab = UnityEngine.Resources.Load<RectTransform>("Icons/Icon");
        uiIcon.TextPrefab = UnityEngine.Resources.Load<RectTransform>("Icons/Text");
        uiIcon.Window = GameObject.FindObjectOfType<WorldMapController>(true).transform.Find("IconWindow") as RectTransform;

        return spaceship;
    }
}
