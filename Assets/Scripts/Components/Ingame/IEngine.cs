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
        
        /// <summary>
        /// Thrust vector in kN that is currently being applied.
        /// </summary>
        public Vector3 CurrentThrust { get; }

        /// <summary>
        /// Set the thrust the engine should apply as a fraction of the maximum appliable thrust.
        /// </summary>
        /// <param name="thrustFraction">Thrust as fraction to apply.</param>
        /// <param name="thrustDirection">The direction of the thrust. This should point "forward" if accelerating forward.</param>
        /// <returns>The actual thrust fraction applied. This can differ from the input value based on engine restrictions.</returns>
        public double SetThrust(double thrustFraction, Vector3 thrustDirection);
    }
}
