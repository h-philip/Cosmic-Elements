using System.Collections;
using System.Collections.Generic;

namespace Components
{
    /// <summary>
    /// This component can scan the closely surrounding space for spaceships and control stations.
    /// </summary>
    public interface IProportionalNavigationUnit : IComponent
    {
        public delegate void TargetReachedAction();

        /// <summary>
        /// Max range in km at which objects can get found.
        /// </summary>
        public double ProportionalityConstant { get; set; }

        /// <summary>
        /// The maximum distance to the target at which it is reached.
        /// </summary>
        public double TargetReachedMaxDistance { get; set; }

        /// <summary>
        /// The maximum fraction of thrust to use [0,1].
        /// </summary>
        public double MaxThrustUsage { get; set; }

        /// <summary>
        /// The action that is called when the target is reached.
        /// </summary>
        public TargetReachedAction OnTargetReached { get; set; }

        /// <summary>
        /// Set relative position of the target at this moment.
        /// </summary>
        /// <param name="targetPosition">Target's current relative position</param>
        public void SetTargetPosition(Vector3 targetPosition);

        /// <summary>
        /// Set relative velocity of the target at this moment.
        /// </summary>
        /// <param name="targetVelocity">Target's current relative velocity</param>
        public void SetTargetVelocity(Vector3 targetVelocity);

        /// <summary>
        /// Start the automatic navigation to the target.
        /// </summary>
        public void StartNavigation();

        /// <summary>
        /// Stop the automatic navigation.
        /// Note: This won't stop the spaceship's motion but just not use thrusters anymore.
        /// </summary>
        public void StopNavigation();
    }
}