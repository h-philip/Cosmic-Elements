using System.Collections;
using System.Collections.Generic;

namespace Components
{
    public interface ISolarPanel : IComponent
    {
        /// <summary>
        /// Energy production in units/second.
        /// </summary>
        public double EnergyProduction { get; }
    }
}