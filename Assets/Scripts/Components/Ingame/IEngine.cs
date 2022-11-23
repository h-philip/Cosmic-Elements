using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Components
{
    public interface IEngine : IComponent
    {
        /// <summary>
        /// Max possible thrust in kN.
        /// </summary>
        public double Thrust { get; }

        /// <summary>
        /// The maximum energy consumption possible at full thrust.
        /// </summary>
        public double MaxEnergyConsumption { get; }
    }
}
