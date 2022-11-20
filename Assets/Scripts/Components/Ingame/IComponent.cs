using System.Collections;
using System.Collections.Generic;

namespace Components
{
    /// <summary>
    /// This interface will be made accessible to ControllScripts.
    /// </summary>
    public interface IComponent
    {
        public enum OnOff { On, Off }

        /// <summary>
        /// Energy consumption in units/second.
        /// </summary>
        public double EnergyConsumption { get; }

        /// <summary>
        /// Mass in kg.
        /// </summary>
        public double Mass { get; }

        /// <summary>
        /// Whether this component is currently powered.
        /// </summary>
        public bool Powered { get; }

        /// <summary>
        /// A short description for this component.
        /// </summary>
        public string ShortDescription { get; }

        /// <summary>
        /// A long description for this component.
        /// </summary>
        public string LongDescription { get; }

        public OnOff State { get; set; }
    }
}