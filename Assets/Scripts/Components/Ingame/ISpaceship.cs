using System;
using System.Collections;
using System.Collections.Generic;

namespace Components
{
    public interface ISpaceship
    {
        public bool IsControlStation { get; }
        public double Food { get; }
        public double Water { get; }
        public double Iron { get; }
        public double Copper { get; }
        public double RareMinerals { get; }
        public double Energy { get; }
        public double Mass { get; }
        public Vector3d Thrust { get; }

        public IComponent[] GetComponents();
        public T[] GetComponentsOfType<T>() where T : IComponent
        {
            List<T> components = new List<T>();
            foreach (var component in GetComponents())
            {
                if (component is T)
                {
                    components.Add((T)component);
                }
            }
            return components.ToArray();
        }
    }
}