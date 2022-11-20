using System.Collections;
using System.Collections.Generic;
using Components;
namespace Components
{
    public abstract class ControlScript
    {
        /// <summary>
        /// Persistant storage to keep data between play sessions.
        /// </summary>
        public virtual string Storage { get; set; }

        public ISpaceship Spaceship { get; set; }

        public readonly string Type;

        public ControlScript(string type)
        {
            Type = type;
        }

        /// <summary>
        /// Start method that is called when the control script first loads.
        /// 
        /// This should be used to load data from Storage if this isn't the first time the script runs.
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// Stop method that is called when the game is closed.
        /// 
        /// This should be used to save and data in Storage that should be kept.
        /// </summary>
        public virtual void Stop() { }

        /// <summary>
        /// Update method that is called continously.
        /// </summary>
        /// <param name="deltatime">Time passed since last run in seconds.</param>
        public virtual void Update(double deltatime) { }
    }
}