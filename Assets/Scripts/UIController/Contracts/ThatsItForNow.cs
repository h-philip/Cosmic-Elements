using System;
using System.Collections.Generic;
using UnityEngine;

namespace Contracts {

    public class ThatsItForNow : IContract
    {
        public Guid Id { get; set; }
        public bool Running { get; set; }
        public string Text => _description;
        public string Title => TITLE;
        public IContract.Todo[] TodoItems => _todoItems;
        public bool Fulfilled { get; set; }

        private IContract.Todo[] _todoItems;
        private const string TITLE = "That's it for now";
        private string _description = @"Sadly, there is nothing more I can offer you right now.
Come back later, and you might find this to be more interesting.";
        private bool hacked = false;

        public ThatsItForNow()
        {
            _todoItems = new IContract.Todo[]
            {
                new IContract.Todo("Do everything.", () => false)
            };
        }

        bool IContract.CanFulfill()
        {
            return hacked;
        }

        public void OnAbort()
        {
            // Nothing
        }

        public void OnFulfill()
        {
            // Nothing
        }

        public void OnStart()
        {
            _description += "\n\nStill nothing left...";
            Array.Resize(ref _todoItems, 2);
            _todoItems[1] = new IContract.Todo("Afterwards, continue.", () => false);
        }

        public void Hack()
        {
            hacked = true;
        }

        public string Serialize()
        { return ""; } // Nothing

        public void Deserialize(string serialized)
        { } // Nothing
    }
}