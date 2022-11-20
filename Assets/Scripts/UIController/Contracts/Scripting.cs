using System;
using System.Collections.Generic;
using UnityEngine;

namespace Contracts {

    public class Scripting : IContract
    {
        // Properties
        public Guid Id { get; set; }
        public bool Running { get; set; }
        public string Text => _description;
        public string Title => TITLE;
        public IContract.Todo[] TodoItems => _todoItems;
        public bool Fulfilled { get; set; }

        // Private fields
        private IContract.Todo[] _todoItems;
        private const string TITLE = "Scripts?!";
        private string _description = @"Okay so this was all fun and games (lol) but NOW we are gonna get to the real thing!
If you find your job boring so far, then worry not as fun programming is still to come!!1
For your next and final task of your probationary period, we need you to actually write a control script.

Don't worry though, as the super boss and CEO and manager and all that of this, you don't actually need to be able to do any real work.
We just need you to make people BELIEVE you'd actually know what you're doing.
So in conclusion: Please write a script.

Yes, there are no more requirement. It's like saying 'Cook anything' in a chef's interview.
We don't care at all, what the script does.
In fact, I personally couldn't care less about whether the script even does ANYTHING.

With that being said, GOOD LUCK! (I am sure you'll actually need it...)

Oh and one last thing: You need to actually test your script. Don't worry, that doesn't mean having to start a mission, but just pressing the ""Test"" button.";
        private ScriptsController _scriptsController;

        public Scripting()
        {
            _scriptsController = GameObject.FindObjectOfType<ScriptsController>(true);
            _todoItems = new IContract.Todo[]
            {
                new IContract.Todo("Create a script and compile (test) it.", () =>
                {
                    try
                    {
                        return _scriptsController.ControlScripts.Count > 0;
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
            GameObject.FindObjectOfType<ContractsController>().NewContract(new ThatsItForNow());
            RectTransform window = _scriptsController.transform.parent.Find("TutorialDoneWindow") as RectTransform;
            window.gameObject.SetActive(true);
        }

        public void OnStart()
        {
        }

        public void Hack()
        {
            if (_scriptsController.ControlScripts.Count == 0)
            {
                _scriptsController.NewScriptText.text = nameof(Scripting);
                _scriptsController.CreateNewScript();
                _scriptsController.CompileAndSaveScript(nameof(Scripting), _scriptsController.Editor.text);
            }
        }

        public string Serialize()
        { return ""; } // Nothing

        public void Deserialize(string serialized)
        { } // Nothing
    }
}