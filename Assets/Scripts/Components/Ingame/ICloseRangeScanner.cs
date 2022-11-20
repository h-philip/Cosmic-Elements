using System.Collections;
using System.Collections.Generic;

namespace Components
{
    /// <summary>
    /// This component can scan the closely surrounding space for spaceships and control stations.
    /// </summary>
    public interface ICloseRangeScanner : IComponent
    {
        /// <summary>
        /// Max range in km at which objects can get found.
        /// </summary>
        public double Range { get; }

        /// <summary>
        /// The energy cost per scan at maximum distance.
        /// </summary>
        public double EnergyPerScan { get; }

        /// <summary>
        /// Perform a scan with the given range.
        /// </summary>
        /// <param name="range">The max range in km at which to find objects. Is capped at Range.</param>
        /// <returns>A list with all found ScanObject objects in range.</returns>
        public ScanObject[] Scan(double range);

        public struct ScanObject
        {
            public Vector3 RelativePosition;
            public Vector3 RelativeVelocity;
            public bool IsControlStation;
            public ScanObject(Vector3 relPos, Vector3 relVel, bool isControlStation)
            {
                RelativePosition = relPos;
                RelativeVelocity = relVel;
                IsControlStation = isControlStation;
            }
        }
    }
}