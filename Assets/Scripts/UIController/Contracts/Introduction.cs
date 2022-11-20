using System;
using System.Collections.Generic;
using UnityEngine;

namespace Contracts {

    public class Introduction : IContract
    {
        public Guid Id { get; set; }
        public bool Running { get; set; }
        public string Text => _description;
        public string Title => TITLE;
        public IContract.Todo[] TodoItems => _todoItems;
        public bool Fulfilled { get; set; }

        private IContract.Todo[] _todoItems;
        private const string TITLE = "Introduction";
        private string _description = @"Hello there and welcome to Cosmic Elements!
During your career here you will need to command hundreds of missions, design new spaceships, program control scripts, manage resources, and more.
We know, that you are new in this field. Because of that, we will guide you through everything in the beginning.

For now, why don't you take a look at the world map and make yourself comfortable with the basic controls.
You can rotate the camera around the focused object by holding the left mouse button or using WASD/arrow keys.
To zoom in and out, use the mouse wheel or hold the right mouse button.

On the right side of the screen, you can switch between listing all of our control stations, your available spaceships, discovered stars, and other objects.

Try out all controls and switch between the different categories. Focus the view on our control station around planet Earth.";

        private SpaceSimulation _spaceSimulation;

        public Introduction()
        {
            _spaceSimulation = GameObject.FindObjectOfType<SpaceSimulation>();
            _todoItems = new IContract.Todo[]
            {
                new IContract.Todo("Focus the view on Earth Control Station around planet Earth.", StationFocused)
            };
        }

        private bool StationFocused()
        {
            try
            {
                return _spaceSimulation.ReferenceObject.GetComponent<ControlStation>().Location == "Earth";
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        public void OnAbort()
        {
            GameObject.FindObjectOfType<ContractsController>().NewContract(this);
        }

        public void OnFulfill()
        {
            GameObject.FindObjectOfType<ContractsController>().NewContract(new TimingTutorial());
        }

        public void OnStart()
        {
            // Nothing
        }

        public void Hack()
        {
            try
            {
                ControlStation earth = Array.Find(ControlStation.GetAllControlStations(true), _ => _.Location == "Earth");
                GameObject.FindObjectOfType<WorldMapController>(true).FocusView(earth.GetComponent<CelestialBody>());
            }
            catch (NullReferenceException)
            { }
        }

        public string Serialize()
        { return ""; } // Nothing

        public void Deserialize(string serialized)
        { } // Nothing
    }
}