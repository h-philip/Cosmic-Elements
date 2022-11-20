using System;
using System.Collections.Generic;
using UnityEngine;

namespace Contracts {

    public class TimingTutorial : IContract
    {
        public Guid Id { get; set; }
        public bool Running { get; set; }
        public string Text => _description;
        public string Title => TITLE;
        public IContract.Todo[] TodoItems => _todoItems;
        public bool Fulfilled { get; set; }

        private IContract.Todo[] _todoItems;
        private const string TITLE = "This trick will make you rich";
        private string _description = @"Easy!
Now the thing is that people here are paid for the time they spend on the job, and not the results.
That is a tiny problem if a single mission can take days in which literally nothing happens.
Thankfully, we found a way to speed up time a bit...

When you are at the world map, you can press '.' to speed up time. If you want to go back to normal, just press ','.
(With this, it is actually also possible to completely pause time. Don't you dare to tell that our employees though!)

Anyway, wanna try it out?";

        private SpaceSimulation _spaceSimulation;

        public TimingTutorial()
        {
            _spaceSimulation = GameObject.FindObjectOfType<SpaceSimulation>();
            _todoItems = new IContract.Todo[]
            {
                new IContract.Todo("Set the time speed to 10.", TimeSpeed10)
            };
        }

        private bool TimeSpeed10()
        {
            try
            {
                return _spaceSimulation.TimeScale == 10;
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
            GameObject.FindObjectOfType<ContractsController>().NewContract(new FirstMission());
        }

        public void OnStart()
        {
            // Nothing
        }

        public void Hack()
        {
            try
            {
                _spaceSimulation.TimeScale = 10;
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