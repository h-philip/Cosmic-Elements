using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Contracts {

    [Serializable]
    public class EcIntroduction : IContract
    {
        public const string SCRIPT_NAME = "EC1 Testing";

        public static bool AnyRunning = false;
        public static bool ShipExists = false;
        public static bool ShipReachedECSpaceship = false;
        public static bool ShipIsBackHome = false;

        public Guid Id { get; set; }
        public bool Running { get; set; }
        public string Text => _description + (Running ? _afterStartAddition : "");
        public string Title => TITLE;
        public IContract.Todo[] TodoItems => _todoItems;
        public bool Fulfilled { get; set; }

        private IContract.Todo[] _todoItems;
        private GameObject _ecSpaceship;
        private const string TITLE = "Introduction from EC";
        private string _description = $@"We've received a transmission from a nearby Spaceship owned by EC corp.
EC corp. is a newly founded corporation on the rocket engine market. No other company is as advanced on the market as they are.

The transmission came from a spaceship close to our control station. They want to test their new {nameof(BasicEngine)} and ask whether we could help them with that.
More specifically, they would like us to install their new engine on one of our ships and then test the engine with some maneuvers.
In exchange for the service, they are willing to let us keep the engine and provide us with the plans for the engine so that we'd be able to build them ourselves.

They will send us the required resources to build the engine and a small spaceship.
";
        private string _afterStartAddition = @"

The plans for the new engine have arrived and are ready to be used. They are inspectable in the Components screen.
Also, the promised resources needed to build the spaceship have arrived at our control station.
Lastly, they sent us a control script that we need to install on the spaceship. It is ready to be selected in the Blueprints screen.";

        public EcIntroduction()
        {
            _todoItems = new IContract.Todo[]
            {
                new IContract.Todo($"Set up a blueprint with the new {nameof(BasicEngine)}, two small solar panels, and the \"{SCRIPT_NAME}\" script and start the spaceship.", () => ShipExists),
                new IContract.Todo("Wait for our new spaceship to reach the position of EC's spaceship.", () => ShipReachedECSpaceship),
                new IContract.Todo("Wait for our new spaceship to get back to our control station.", () => ShipIsBackHome)
            };
        }

        public void OnAbort()
        {
            GameObject.FindObjectOfType<ScriptsController>(true).ControlScripts.Remove(SCRIPT_NAME);
            GameObject.FindObjectOfType<ComponentsController>(true).LockComponent(typeof(BasicEngine).ToString());
            AnyRunning = false;
            ShipExists = false;
            ShipReachedECSpaceship = false;
            ShipIsBackHome = false;
            GameObject.Destroy(_ecSpaceship);
        }

        public void OnFulfill()
        {
            GameObject.FindObjectOfType<ScriptsController>(true).ControlScripts.Remove(SCRIPT_NAME);
            int iron = 10, copper = 10, rareMinerals = 2;
            ControlStation earth = Array.Find(ControlStation.GetAllControlStations(true), _ => _.Location == "Earth");
            earth.Iron += iron;
            earth.Copper += copper;
            earth.RareMinerals += rareMinerals;
            Component[] components = new Component[earth.Components.Length + 1];
            earth.Components.CopyTo(components, 0);
            components[components.Length - 1] = new SmallSolarPanel(earth);
            earth.Components = components;
            GameObject.FindObjectOfType<NotificationsController>().NewNotification(
                "Contract Reward",
                $"As a reward for fulfilling the {Title} contract, you were awarded {iron} Iron, {copper} Copper, and {rareMinerals} Rare Minerals at the {earth.Location} control station. Also, a new {typeof(SmallSolarPanel).ToString()} was installed on the station.",
                onClick: () =>
                {
                    GameObject.FindObjectOfType<UIController>().SetActiveScreen(UIController.Screen.ControlStations);
                    GameObject.FindObjectOfType<ControlStationsController>(true).SelectControlStation(earth);
                });

            GameObject.FindObjectOfType<ContractsController>().NewContract(new Scripting());
            AnyRunning = false;
            ShipExists = false;
            ShipReachedECSpaceship = false;
            ShipIsBackHome = false;
            GameObject.Destroy(_ecSpaceship);
        }

        public void OnStart()
        {
            AnyRunning = true;
            ControlStation earth = Array.Find(ControlStation.GetAllControlStations(true), _ => _.Location == "Earth");
            GameObject.FindObjectOfType<ComponentsController>(true).UnlockComponent(typeof(BasicEngine).ToString());
            GameObject.FindObjectOfType<ScriptsController>(true).ControlScripts.Add(SCRIPT_NAME, typeof(ControlScripts.EC1Testing));

            // EC Spaceship
            Spaceship spaceship = new Blueprint("EC Spaceship", components: new string[] {typeof(BasicStructure).ToString()}).CreateSpaceship(GameObject.FindObjectOfType<ControlStationsController>(true).SpaceshipPrefab);
            _ecSpaceship = spaceship.gameObject;
            spaceship.transform.parent = earth.transform.parent;
            CelestialBody body = spaceship.GetComponent<CelestialBody>();
            CelestialBody stationBody = earth.GetComponent<CelestialBody>();
            body.Attractor = stationBody.Attractor;
            body.Radius = 0.05f;
            body.Scale = stationBody.Scale;
            body.KeplerOrbitData = new SimpleKeplerOrbits.KeplerOrbitData(
                stationBody.KeplerOrbitData.Position + new SimpleKeplerOrbits.Vector3d(80, 0, 0),
                stationBody.KeplerOrbitData.Velocity,
                stationBody.KeplerOrbitData.AttractorMass,
                stationBody.KeplerOrbitData.GravConst);
            body.A = body.KeplerOrbitData.SemiMajorAxis;
            body.AttractorMass = body.KeplerOrbitData.AttractorMass;
            foreach (Component component in spaceship.Components)
                component.Install();
            spaceship.PlayerControlled = false;
            UiIcon icon = _ecSpaceship.GetComponent<UiIcon>();
            NPC npc = _ecSpaceship.AddComponent<NPC>();
            npc.PopUpText = "We sent you a transmission. Please take a look at your contracts.";
            ActionOnNearby actionOnNearby = _ecSpaceship.AddComponent<ActionOnNearby>();
            actionOnNearby.Range = 10;
            actionOnNearby.TriggerOnNoComms = true;
            actionOnNearby.TriggerOnControlStation = false;
            actionOnNearby.RequiredComponents = new HashSet<Type> { typeof(BasicStructure), typeof(BasicEngine), typeof(SmallSolarPanel) };
            actionOnNearby.OnNearby = OnNearbyAction;
            spaceship.Addons = new IAddon[] { npc, actionOnNearby };

            Blueprint bp = new Blueprint("", "", new string[] { typeof(BasicStructure).ToString(), typeof(BasicEngine).ToString(), typeof(SmallSolarPanel).ToString(), typeof(SmallSolarPanel).ToString() }, SCRIPT_NAME);
            earth.Energy += bp.Energy;
            earth.Iron += bp.Iron;
            earth.Copper += bp.Copper;
            earth.RareMinerals += bp.RareMinerals;
        }

        private void OnNearbyAction(Spaceship ship)
        {
            NPC npc = _ecSpaceship.GetComponent<NPC>();
            npc.PopUpText = "Hey! You got a very fancy looking spaceship there.";
            UiIcon icon = _ecSpaceship.GetComponent<UiIcon>();
            if (icon.IsInWindow(Camera.main.WorldToScreenPoint(icon.transform.position)))
                npc.OpenMenu(Camera.main.WorldToScreenPoint(icon.transform.position));
        }

        public void Hack()
        {
            ShipExists = true;
            ShipReachedECSpaceship = true;
            ShipIsBackHome = true;

            Blueprint bp = new Blueprint("Hacked " + Title,
                "This blueprint has been created by HaCkInG the contract " + Title,
                new string[] { typeof(BasicStructure).ToString(), typeof(BasicEngine).ToString(), typeof(SmallSolarPanel).ToString(), typeof(SmallSolarPanel).ToString() });
            GameObject.FindObjectOfType<BlueprintsController>(true).Blueprints.Add(bp.Name, bp);
            GameObject.FindObjectOfType<ControlStationsController>(true).SelectedStation = Array.Find(ControlStation.GetAllControlStations(true), _ => _.Location == "Earth");
            GameObject.FindObjectOfType<ControlStationsController>(true).StartNewMission(bp);
        }

        public string Serialize()
        {
            // Static variables and _ecSpaceship
            string id = _ecSpaceship != null ? _ecSpaceship.GetComponent<Spaceship>().Id.ToString() : "";
            return $"{AnyRunning},{ShipExists},{ShipReachedECSpaceship},{ShipIsBackHome};{id}";

        }

        public void Deserialize(string serialized)
        {
            try
            {
                string[] booleans = serialized.Split(';')[0].Split(',');
                AnyRunning = bool.Parse(booleans[0]);
                ShipExists = bool.Parse(booleans[1]);
                ShipReachedECSpaceship = bool.Parse(booleans[2]);
                ShipIsBackHome = bool.Parse(booleans[3]);
                if (Running)
                    GameObject.FindObjectOfType<ScriptsController>(true).ControlScripts.Add(SCRIPT_NAME, typeof(ControlScripts.EC1Testing));

                try
                {
                    _ecSpaceship = Array.Find(Spaceship.GetAllSpaceships(false), _ => _.Id == Guid.Parse(serialized.Split(';')[1])).gameObject;
                }
                catch (Exception)
                { }
                if (_ecSpaceship == null)
                {
                    GameObject go = new(nameof(EcIntroduction) + " Spaceship Finder", typeof(SpaceshipFinder));
                    SpaceshipFinder finder = go.GetComponent<SpaceshipFinder>();
                    finder.StartCoroutine(finder.FindSpaceship(1, () =>
                    {
                        try
                        {
                            _ecSpaceship = Array.Find(Spaceship.GetAllSpaceships(false), _ => _.Id == Guid.Parse(serialized.Split(';')[1])).gameObject;
                            if (_ecSpaceship != null)
                                _ecSpaceship.GetComponent<ActionOnNearby>().OnNearby = OnNearbyAction;
                        }
                        catch (Exception)
                        { }
                        return _ecSpaceship != null;
                    }));
                }
            }
            catch
            {
                throw new ArgumentException("Invalid serialized string given.");
            }
        }
        public class SpaceshipFinder : MonoBehaviour
        {
            public delegate bool BoolAction();
            public IEnumerator FindSpaceship(float seconds, BoolAction found)
            {
                while (!found())
                {
                    yield return new WaitForSeconds(seconds);
                }
            }
        }
    }
}