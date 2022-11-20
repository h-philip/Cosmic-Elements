using System;
using System.Collections.Generic;
using UnityEngine;

namespace Contracts {

    public class FirstMission : IContract
    {
        public Guid Id { get; set; }
        public bool Running { get; set; }
        public string Text => _description;
        public string Title => TITLE;
        public IContract.Todo[] TodoItems => _todoItems;
        public bool Fulfilled { get; set; }

        private IContract.Todo[] _todoItems;
        private const string TITLE = "First Mission";
        private string _description = @"Well done!
Next, why don't you start your very first mission?
We just found some spare resources in the basement. Just click ""Start"" and we'll send them to our control station in no time.

You'll first need to create a new blueprint. Blueprints are used to define which components to install on a new ship, which control script should be used, what the ship is called, and more.
To create a new blueprint, go to the blueprints screen. Once you create a new blueprint, you'll be able to add new components to the blueprint by clicking on them in the right of the screen.
If you want to remove them again, just click on them in the list in the middle.

After you've created a new blueprint, go to the control stations screen and select Earth.
You can then click on the ""Start New Mission"" button and select the blueprint that you just created.
If you added too many components, you won't be able to create the spaceship. That is because too many resources are required, which we don't have yet.
To fix that, just go back to the blueprints screen.

Once you created the new spaceship, we have a little surprise for you...";
        private ControlStation _earthControlStation;

        public FirstMission()
        {
            _earthControlStation = Array.Find(ControlStation.GetAllControlStations(true), _ => _.Location == "Earth");
            _todoItems = new IContract.Todo[]
            {
                new IContract.Todo("Create a new blueprint in the blueprints screen. Make sure that you don't add too many components as we don't have that many resources yet.", () =>
                {
                    try
                    {
                        return GameObject.FindObjectOfType<BlueprintsController>(true).Blueprints.Count > 0;
                    }
                    catch
                    {
                        return false;
                    }
                }),
                new IContract.Todo("Select the Earth control station at the control stations screen.", () =>
                {
                    try
                    {
                        return GameObject.FindObjectOfType<ControlStationsController>(true).SelectedStation == _earthControlStation;
                    }
                    catch
                    {
                        return false;
                    }
                }),
                new IContract.Todo("Start a new spaceship by clicking on the \"Start New Mission\" button.", () =>
                {
                    try
                    {
                        return Spaceship.GetAllSpaceships(true).Length > 0;
                    }
                    catch
                    {
                        return false;
                    }
                })
            };
        }


        public void OnAbort()
        {
            GameObject.FindObjectOfType<ContractsController>().NewContract(this);
        }

        public void OnFulfill()
        {
            int iron = 10, copper = 10, rareMinerals = 2;
            _earthControlStation.Iron += iron;
            _earthControlStation.Copper += copper;
            _earthControlStation.RareMinerals += rareMinerals;
            Component[] components = new Component[_earthControlStation.Components.Length + 1];
            _earthControlStation.Components.CopyTo(components, 0);
            components[components.Length - 1] = new SmallSolarPanel(_earthControlStation);
            _earthControlStation.Components = components;
            GameObject.FindObjectOfType<NotificationsController>().NewNotification(
                "Contract Reward",
                $"As a reward for fulfilling the {Title} contract, you were awarded {iron} Iron, {copper} Copper, and {rareMinerals} Rare Minerals at the {_earthControlStation.Location} control station. Also, a new {typeof(SmallSolarPanel).ToString()} was installed on the station.",
                onClick: () =>
                {
                    GameObject.FindObjectOfType<UIController>().SetActiveScreen(UIController.Screen.ControlStations);
                    GameObject.FindObjectOfType<ControlStationsController>(true).SelectControlStation(_earthControlStation);
                });

            GameObject.FindObjectOfType<ContractsController>().NewContract(new EcIntroduction());
        }

        public void OnStart()
        {
            _earthControlStation.Energy += 2000;
            _earthControlStation.Iron += 20;
            _earthControlStation.Copper += 10;
        }

        public void Hack()
        {
            Blueprint bp = new Blueprint("Hacked " + Title,
                "This blueprint has been created by HaCkInG the contract " + Title,
                new string[] {typeof(BasicStructure).ToString()});
            GameObject.FindObjectOfType<BlueprintsController>(true).Blueprints.Add(bp.Name, bp);
            GameObject.FindObjectOfType<ControlStationsController>(true).SelectedStation = _earthControlStation;
            GameObject.FindObjectOfType<ControlStationsController>(true).StartNewMission(bp);
        }

        public string Serialize()
        { return ""; } // Nothing

        public void Deserialize(string serialized)
        { } // Nothing
    }
}