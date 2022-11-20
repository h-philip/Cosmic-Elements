using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    public bool LoadGameOnStart = false;

    // Constants
    public const string SESSION_INFO_FILE = "session.json";
    public const string BLUEPRINTS_FILE = "blueprints.json";
    public const string COMPONENTS_FILE = "components.json";
    public const string CONTRACT_FILE = "contracts.json";
    public const string CELESTIAL_BODIES_FILE = "celestial_bodies.json";
    public const string SPACESHIPS_FILE  = "spaceships.json";

    // Static
    public static string SessionName;

    // Properties
    public string SessionDirectory => Path.Join(_sessionsDirectory, SessionName);
    public string SettingsFile => Path.Join(Application.persistentDataPath, "settings.json");

    // Private
    private string _sessionsDirectory;

    // Unity methods

    private void Awake()
    {
        _sessionsDirectory = Path.Join(Application.persistentDataPath, "Sessions");
        if (!Directory.Exists(_sessionsDirectory))
            Directory.CreateDirectory(_sessionsDirectory);
    }

    private void Start() // TODO: Move to awake or something to actually have this when loading
    {
        if (LoadGameOnStart)
        {
            SessionInfo[] loadableSessions = GetLoadableSessions();
            if (string.IsNullOrEmpty(SessionName))
                SessionName = "Editor";
            if (Array.FindIndex(loadableSessions, _ => _.Name == SessionName) == -1)
                NewGame();
            else
                LoadGame(SessionName);
        }
    }

    // New Game

    public void NewGame()
    {
        FindObjectOfType<ComponentsController>(true).NewGame();
        FindObjectOfType<ControlStationsController>(true).NewGame();
        FindObjectOfType<ContractsController>(true).NewGame();
    }

    // Load Game

    public void LoadGame(string sessionName)
    {
        SessionName = sessionName;
        LoadBlueprints();
        LoadComponents();
        LoadCelestialBodies();
        LoadContracts();
        LoadSpaceships();
    }

    public SessionInfo[] GetLoadableSessions()
    {
        List<SessionInfo> sessions = new();
        foreach (string directory in Directory.EnumerateDirectories(_sessionsDirectory))
        {
            bool files_exist = File.Exists(Path.Join(directory, BLUEPRINTS_FILE))
                && File.Exists(Path.Join(directory, COMPONENTS_FILE))
                && File.Exists(Path.Join(directory, CONTRACT_FILE));
            if (files_exist)
                try
                {
                    sessions.Add(LoadSessionInfo(new DirectoryInfo(directory).Name));
                }
                catch (Exception)
                {
                    continue;
                }
        }
        return sessions.ToArray();
    }

    // Save Game

    public void SaveGame()
    {
        // Create directory if needed
        if (!Directory.Exists(SessionDirectory))
            Directory.CreateDirectory(SessionDirectory);
        SaveSessionInfo();
        SaveBlueprints();
        SaveComponents();
        SaveCelestialBodies();
        SaveSpaceships();
        SaveContracts();
    }

    // Blueprints
    [Serializable]
    private class BlueprintsCollection
    {
        public Blueprint[] Blueprints;
    }
    private void LoadBlueprints()
    {
        string file = Path.Combine(SessionDirectory, BLUEPRINTS_FILE);
        Blueprint[] blueprints;
        if (File.Exists(file))
            blueprints = JsonUtility.FromJson<BlueprintsCollection>(File.ReadAllText(file)).Blueprints;
        else
            blueprints = new Blueprint[0];
        FindObjectOfType<BlueprintsController>(true).LoadBlueprints(blueprints);
        //Debug.Log($"Loaded blueprints ({blueprints.Length}) from {file}");
    }
    private void SaveBlueprints()
    {
        string file = Path.Join(SessionDirectory, BLUEPRINTS_FILE);
        var bpValues = FindObjectOfType<BlueprintsController>(true).Blueprints.Values;
        BlueprintsCollection bc = new BlueprintsCollection();
        bc.Blueprints = new Blueprint[bpValues.Count];
        bpValues.CopyTo(bc.Blueprints, 0);
        File.WriteAllText(file, JsonUtility.ToJson(bc, true));
        //Debug.Log($"Wrote blueprints ({bc.Blueprints.Length}) to {file}");
    }

    // Components
    [Serializable]
    private class ComponentsCollection
    {
        public string[] UnlockedComponents;
    }
    private void LoadComponents()
    {
        string file = Path.Join(SessionDirectory, COMPONENTS_FILE);
        string[] components;
        if (File.Exists(file))
            components = JsonUtility.FromJson<ComponentsCollection>(File.ReadAllText(file)).UnlockedComponents;
        else
            components = new string[0];
        FindObjectOfType<ComponentsController>(true).LoadComponents(components);
        //Debug.Log($"Loaded unlocked components ({components.Length}) from {file}");
    }
    private void SaveComponents()
    {
        string file = Path.Join(SessionDirectory, COMPONENTS_FILE);
        ComponentsCollection collection = new ComponentsCollection();
        collection.UnlockedComponents = new string[FindObjectOfType<ComponentsController>(true).UnlockedComponents.Count];
        int i = 0;
        foreach (string component in FindObjectOfType<ComponentsController>(true).UnlockedComponents.Keys)
            collection.UnlockedComponents[i++] = component;
        File.WriteAllText(file, JsonUtility.ToJson(collection));
        //Debug.Log($"Wrote unlocked components ({collection.UnlockedComponents.Length}) to {file}");
    }

    // Contracts
    [Serializable]
    private class ContractsCollection
    {
        [Serializable]
        public struct ContractData
        {
            public string Type;
            public Guid Id;
            public bool Running;
            public bool Fulfilled;
            public string Serialized;
        }

        public ContractData[] WaitingContracts;
        public ContractData[] RunningContracts;

        public static Contracts.IContract ToContract(ContractData data)
        {
            Contracts.IContract contract = Activator.CreateInstance(Type.GetType(data.Type)) as Contracts.IContract;
            contract.Id = data.Id;
            contract.Running = data.Running;
            contract.Fulfilled = data.Fulfilled;
            try
            {
                contract.Deserialize(data.Serialized);
            }
            catch (Exception e)
            {
                Debug.LogError($"Couldn't deserialize contract of type {data.Type} with data {data.Serialized}: {e.Message}");
            }
            return contract;
        }

        public static ContractData FromContract(Contracts.IContract contract)
        {
            return new ContractData()
            {
                Type = contract.GetType().ToString(),
                Id = contract.Id,
                Running = contract.Running,
                Fulfilled = contract.Fulfilled,
                Serialized = contract.Serialize()
            };
        }
    }
    private void LoadContracts()
    {
        string file = Path.Join(SessionDirectory, CONTRACT_FILE);
        ContractsCollection collection;
        if (File.Exists(file))
            collection = JsonUtility.FromJson<ContractsCollection>(File.ReadAllText(file));
        else
            collection = new()
            {
                WaitingContracts = new ContractsCollection.ContractData[0],
                RunningContracts = new ContractsCollection.ContractData[0]
            };
        Contracts.IContract[] waiting = new Contracts.IContract[collection.WaitingContracts.Length];
        Contracts.IContract[] running = new Contracts.IContract[collection.RunningContracts.Length];
        for (int i = 0; i < collection.WaitingContracts.Length; i++)
            waiting[i] = ContractsCollection.ToContract(collection.WaitingContracts[i]);
        for (int i = 0; i < collection.RunningContracts.Length; i++)
            running[i] = ContractsCollection.ToContract(collection.RunningContracts[i]);

        ContractsController controller = FindObjectOfType<ContractsController>(true);
        controller.LoadContracts(waiting, running);
        //Debug.Log($"Loaded waiting and running contracts ({waiting.Length}, {running.Length}) from {file}");
    }
    private void SaveContracts()
    {
        string file = Path.Join(SessionDirectory, CONTRACT_FILE);
        ContractsController controller = FindObjectOfType<ContractsController>(true);
        Contracts.IContract[] waiting = controller.WaitingContracts;
        Contracts.IContract[] running = controller.RunningContracts;
        ContractsCollection collection = new ContractsCollection()
        {
            WaitingContracts = new ContractsCollection.ContractData[waiting.Length],
            RunningContracts = new ContractsCollection.ContractData[running.Length],
        };
        for (int i = 0; i < waiting.Length; i++)
            collection.WaitingContracts[i] = ContractsCollection.FromContract(waiting[i]);
        for (int i = 0; i < running.Length; i++)
            collection.RunningContracts[i] = ContractsCollection.FromContract(running[i]);
        File.WriteAllText(file, JsonUtility.ToJson(collection));
        //Debug.Log($"Wrote waiting and running contracts ({waiting.Length}, {running.Length}) to {file}");
    }

    // CelestialBodies
    [Serializable]
    private class CelestialBodiesCollection
    {
        [Serializable]
        public struct CelestialBodyData
        {
            public string Name;
            public string Attractor;
            public double AttractorMass;
            public SimpleKeplerOrbits.Vector3d Position;
            public SimpleKeplerOrbits.Vector3d Velocity;
        }

        public CelestialBodyData[] Data;
    }
    private void LoadCelestialBodies()
    {
        // Note: This does not create new celestial bodies!

        // Read from file
        string file = Path.Join(SessionDirectory, CELESTIAL_BODIES_FILE);
        CelestialBodiesCollection collection;
        if (File.Exists(file))
            collection = JsonUtility.FromJson<CelestialBodiesCollection>(File.ReadAllText(file));
        else
            collection = new()
            {
                Data = new CelestialBodiesCollection.CelestialBodyData[0]
            };

        // Gather existing celestial bodies
        CelestialBody[] existingBodies = FindObjectsOfType<CelestialBody>();
        // Gather possible attractors (all bodies + SpaceSimulation)
        Transform[] validAttractors = new Transform[existingBodies.Length + 1];
        for (int i = 0; i < existingBodies.Length; i++)
        {
            validAttractors[i] = existingBodies[i].transform;
        }
        validAttractors[validAttractors.Length - 1] = FindObjectOfType<SpaceSimulation>().transform;
        // Apply loaded data to existing celestial bodies
        foreach (CelestialBodiesCollection.CelestialBodyData data in collection.Data)
        {
            CelestialBody body = Array.Find(existingBodies, _ => _.name == data.Name);
            if (body == null)
            {
                Debug.LogError($"Could not load CelestialBodyData {data.Name}: No existing body with that name found.");
                continue;
            }
            Transform attractor = Array.Find(validAttractors, _ => _.name == data.Attractor);
            if (attractor == null)
            {
                Debug.LogError($"Could not load CelestialBodyData {data.Name}: No attractor with name '{data.Attractor}' found.");
                continue;
            }
            body.Attractor = attractor;
            body.AttractorMass = data.AttractorMass;
            if (data.Position.x != 0 || data.Position.y != 0 ||data.Position.z != 0)
                body.KeplerOrbitData = new(data.Position, data.Velocity, data.AttractorMass, FindObjectOfType<SpaceSimulation>().GravConst);
        }
        //Debug.Log($"Loaded celestial bodies ({collection.Data.Length}) from {file}");
    }
    private void SaveCelestialBodies()
    {
        string file = Path.Join(SessionDirectory, CELESTIAL_BODIES_FILE);
        // Get all bodies
        CelestialBody[] bodies = FindObjectsOfType<CelestialBody>();
        List<CelestialBodiesCollection.CelestialBodyData> data = new();
        foreach (CelestialBody body in bodies)
        {
            // Skip bodies that are spaceships
            if (body.GetComponent<Spaceship>() != null)
                continue;
            CelestialBodiesCollection.CelestialBodyData bodyData = new()
            {
                Name = body.name,
                Attractor = body.Attractor.name,
                AttractorMass = body.AttractorMass,
                Position = body.KeplerOrbitData.Position,
                Velocity = body.KeplerOrbitData.Velocity,
            };
            data.Add(bodyData);
        }
        CelestialBodiesCollection collection = new()
        {
            Data = data.ToArray()
        };
        File.WriteAllText(file, JsonUtility.ToJson(collection));
        //Debug.Log($"Wrote celestial bodies ({collection.Data.Length}) to {file}");
    }

    // Spaceships
    private class SpaceshipsCollection
    {
        [Serializable]
        public struct SpaceshipData
        {
            public string Name;
            public bool PlayerControlled;
            public double Food;
            public double Water;
            public double Iron;
            public double Copper;
            public double RareMinerals;
            public double Energy;
            public bool IsControlStation;
            public ControlStationData ControlStationData;
            public ComponentData[] Components;
            public AddonData[] Addons;
            public string Id;
            public string ScriptType;
            public string ScriptData;
            public CelestialBodiesCollection.CelestialBodyData CelestialBodyData;
        }
        [Serializable]
        public struct ComponentData
        {
            public string Type;
            public string SerializedComponent;
        }
        [Serializable]
        public struct AddonData
        {
            public string Type;
            public string Serialized;
        }
        [Serializable]
        public struct ControlStationData
        {
            public int Population;
        }

        public SpaceshipData[] Spaceships;
    }
    private void LoadSpaceships()
    {
        // Read from file
        string file = Path.Join(SessionDirectory, SPACESHIPS_FILE);
        SpaceshipsCollection collection;
        if (File.Exists(file))
            collection = JsonUtility.FromJson<SpaceshipsCollection>(File.ReadAllText(file));
        else
            collection = new()
            {
                Spaceships = new SpaceshipsCollection.SpaceshipData[0]
            };

        // Gather possible attractors (all bodies + SpaceSimulation)
        CelestialBody[] existingBodies = FindObjectsOfType<CelestialBody>();
        Transform[] validAttractors = new Transform[existingBodies.Length + 1];
        for (int i = 0; i < existingBodies.Length; i++)
        {
            validAttractors[i] = existingBodies[i].transform;
        }
        validAttractors[validAttractors.Length - 1] = FindObjectOfType<SpaceSimulation>().transform;

        // Create new spaceships
        foreach (SpaceshipsCollection.SpaceshipData data in collection.Spaceships)
        {
            Spaceship ship;

            if (data.IsControlStation)
            {
                // ControlStation
                ControlStation station = ControlStationsController.CreateNewControlStation(data.Name);
                station.Population = data.ControlStationData.Population;
                ship = station;
            }
            else
            {
                // Spaceship
                Blueprint blueprint = new(data.Name);
                ship = blueprint.CreateSpaceship(FindObjectOfType<ControlStationsController>(true).SpaceshipPrefab);
            }
            ship.transform.parent = FindObjectOfType<SpaceSimulation>().transform;
            ship.name = data.Name;
            ship.PlayerControlled = data.PlayerControlled;
            ship.Food = data.Food;
            ship.Water = data.Water;
            ship.Iron = data.Iron;
            ship.Copper = data.Copper;
            ship.RareMinerals = data.RareMinerals;
            ship.Energy = data.Energy;
            List<Component> components = new();
            foreach (SpaceshipsCollection.ComponentData componentData in data.Components)
            {
                object[] parameters = { ship };
                Component component = Activator.CreateInstance(Type.GetType(componentData.Type), parameters) as Component;
                JsonUtility.FromJsonOverwrite(componentData.SerializedComponent, component);
                components.Add(component);
            }
            ship.Components = components.ToArray();
            List<IAddon> addons = new();
            foreach (SpaceshipsCollection.AddonData addonData in data.Addons)
            {
                IAddon addon = ship.gameObject.AddComponent(Type.GetType(addonData.Type)) as IAddon;
                addon.Deserialize(addonData.Serialized);
                addons.Add(addon);
            }
            ship.Addons = addons.ToArray();
            ship.Id = Guid.Parse(data.Id);
            if (!string.IsNullOrEmpty(data.ScriptType))
            {
                ship.Script = FindObjectOfType<ScriptsController>(true).GetNewScript(data.ScriptType);
                if (ship.Script != null)
                {
                    ship.Script.Spaceship = ship;
                    ship.Script.Storage = data.ScriptData;
                    ship.Script.Start();
                }
            }

            // CelestialBody

            CelestialBody body = ship.GetComponent<CelestialBody>();
            Transform attractor = Array.Find(validAttractors, _ => _.name == data.CelestialBodyData.Attractor);
            if (attractor == null)
            {
                Debug.LogError($"Could not load SpaceshipData {data.Name}: No attractor with name '{data.CelestialBodyData.Attractor}' found.");
                Destroy(ship.gameObject);
                continue;
            }
            body.AttractorMass = data.CelestialBodyData.AttractorMass;
            body.Radius = ControlStationsController.DIAMETER / 2;
            body.Attractor = attractor;
            body.Scale = FindObjectOfType<SpaceSimulation>().DistanceScale;
            body.KeplerOrbitData = new(data.CelestialBodyData.Position, data.CelestialBodyData.Velocity, data.CelestialBodyData.AttractorMass, FindObjectOfType<SpaceSimulation>().GravConst);
        }
        //Debug.Log($"Loaded spaceships ({collection.Spaceships.Length}) from {file}");
    }
    private void SaveSpaceships()
    {
        string file = Path.Join(SessionDirectory, SPACESHIPS_FILE);
        // Get all spaceships
        Spaceship[] spaceships = FindObjectsOfType<Spaceship>();
        List<SpaceshipsCollection.SpaceshipData> data = new();
        foreach (Spaceship ship in spaceships)
        {
            // CelestialBody
            CelestialBody body = ship.GetComponent<CelestialBody>();
            CelestialBodiesCollection.CelestialBodyData bodyData = new()
            {
                Name = body.name,
                Attractor = body.Attractor.name,
                AttractorMass = body.AttractorMass,
                Position = body.KeplerOrbitData.Position,
                Velocity = body.KeplerOrbitData.Velocity,
            };
            // ControlStation
            SpaceshipsCollection.ControlStationData controlStationData = new();
            if (ship.IsControlStation)
            {
                ControlStation station = ship as ControlStation;
                controlStationData = new()
                {
                    Population = station.Population
                };
            }
            // Components
            List<SpaceshipsCollection.ComponentData> components = new();
            foreach (Component component in ship.Components)
            {
                SpaceshipsCollection.ComponentData componentData = new()
                {
                    Type = component.GetType().ToString(),
                    SerializedComponent = JsonUtility.ToJson(component),
                };
                components.Add(componentData);
            }
            // Addons
            List<SpaceshipsCollection.AddonData> addons = new();
            foreach (IAddon addon in ship.Addons)
            {
                SpaceshipsCollection.AddonData addonData = new()
                {
                    Type = addon.GetType().ToString(),
                    Serialized = addon.Serialize(),
                };
                addons.Add(addonData);
            }
            // ControlScript
            string storage = null;
            string scriptType = null;
            if (ship.Script != null)
            {
                scriptType = ship.Script.Type;
                ship.Script.Stop();
                storage = ship.Script.Storage;
                ship.Script.Start();
            }

            // Spaceship
            SpaceshipsCollection.SpaceshipData shipData = new()
            {
                Name = ship.name,
                PlayerControlled = ship.PlayerControlled,
                Food = ship.Food,
                Water = ship.Water,
                Iron = ship.Iron,
                Copper = ship.Copper,
                RareMinerals = ship.RareMinerals,
                Energy = ship.Energy,
                IsControlStation = ship.IsControlStation,
                ControlStationData = controlStationData,
                Components = components.ToArray(),
                Addons = addons.ToArray(),
                Id = ship.Id.ToString(),
                ScriptType = scriptType,
                ScriptData = storage,
                CelestialBodyData = bodyData,
            };
            data.Add(shipData);
        }
        SpaceshipsCollection collection = new()
        {
            Spaceships = data.ToArray()
        };
        File.WriteAllText(file, JsonUtility.ToJson(collection));
        //Debug.Log($"Wrote spaceships ({collection.Spaceships.Length}) to {file}");
    }

    // Session Info
    [Serializable]
    public struct SessionInfo
    {
        public string Name;
        public double Energy;
        public int DiscoveredStars;
        public int ControlStations;
        public int Spaceships;
    }
    public SessionInfo GetCurrentSessionInfo()
    {
        SessionInfo sessionInfo = new SessionInfo();
        sessionInfo.Name = SessionName;
        // Energy and ControlStations and Spaceships
        sessionInfo.Energy = 0;
        sessionInfo.ControlStations = 0;
        sessionInfo.Spaceships = 0;
        Spaceship[] spaceships = FindObjectsOfType<Spaceship>();
        foreach (Spaceship ship in spaceships)
            if (ship.PlayerControlled)
            {
                sessionInfo.Energy += ship.Energy;
                if (ship.IsControlStation)
                    sessionInfo.ControlStations++;
                else
                    sessionInfo.Spaceships++;
            }
        // Discovered stars
        sessionInfo.DiscoveredStars = 0;
        Star[] stars = FindObjectsOfType<Star>(true);
        foreach (Star star in stars)
            if (star.IsDiscovered)
                sessionInfo.DiscoveredStars++;
        return sessionInfo;
    }
    public SessionInfo LoadSessionInfo(string name)
    {
        // Read from file
        string file = Path.Join(_sessionsDirectory, name, SESSION_INFO_FILE);
        try
        {
            SessionInfo sessionInfo = JsonUtility.FromJson<SessionInfo>(File.ReadAllText(file));
            //Debug.Log($"Loaded SessionInfo from {file}");
            return sessionInfo;
        }
        catch
        {
            return new SessionInfo();
        }
    }
    public void SaveSessionInfo()
    {
        string file = Path.Join(SessionDirectory, SESSION_INFO_FILE);
        SessionInfo sessionInfo = GetCurrentSessionInfo();
        File.WriteAllText(file, JsonUtility.ToJson(sessionInfo));
        //Debug.Log($"Wrote SessionInfo to {file}");
    }

    // Settings
    [Serializable]
    public struct Settings
    {
        public float MasterVolume;
        public float MusicVolume;
        public float UIVolume;
        public float IngameVolume;
    }
    public Settings LoadSettings()
    {
        Settings settings = JsonUtility.FromJson<Settings>(File.ReadAllText(SettingsFile));
        //Debug.Log($"Loaded Settings from {SettingsFile}");
        return settings;
    }
    public void SaveSettings(Settings settings)
    {
        File.WriteAllText(SettingsFile, JsonUtility.ToJson(settings));
        //Debug.Log($"Wrote Settings to {SettingsFile}");
    }
}
