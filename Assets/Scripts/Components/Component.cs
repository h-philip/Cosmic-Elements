using System;
using System.Collections;
using UnityEngine;
using Components;
/// <summary>
/// This abstract class needs to be inherited by components in order to allow the game world to interact with them correctly.
/// </summary>
[Serializable]
public abstract class Component : IComponent
{
    // IComponent
    public abstract double EnergyConsumption { get; }
    public abstract double Mass { get; }
    public virtual bool Powered => _powered;
    public abstract string ShortDescription { get; }
    public abstract string LongDescription { get; }
    public IComponent.OnOff State { get => _state; set => _state = value; }

    // Own

    // Public properties
    public abstract double Energy { get; }
    public abstract int Iron { get; }
    public abstract int Copper { get; }
    public abstract int RareMinerals { get; }

    // Private fields
    protected readonly Spaceship Spaceship;
    [SerializeField]
    private bool _powered = true;
    [SerializeField]
    private IComponent.OnOff _state = IComponent.OnOff.On;

    // Constructor
    public Component(Spaceship spaceship)
    {
        Spaceship = spaceship;
    }

    // Methods

    /// <summary>
    /// This method is regularly called by the spaceship it is installed on.
    /// </summary>
    /// <param name="deltatime">The time since the last time this method was called in seconds.</param>
    public virtual void Update(double deltatime)
    {
        double energy = EnergyConsumption * deltatime;
        _powered = State == IComponent.OnOff.On && energy <= Spaceship.Energy;
        if (Powered)
            Spaceship.Energy -= energy;
    }

    /// <summary>
    /// This method is called when the component is installed on a spaceship.
    /// </summary>
    public virtual void Install() { }

    /// <summary>
    /// This method is called when the component is uninstalled from a spaceship.
    /// </summary>
    public virtual void Uninstall() { }

    public struct ComponentInfo
    {
        public Type Type;
        public string Name;
        public double EnergyConsumption;
        public double Mass;
        public double Energy;
        public int Iron, Copper, RareMinerals;
        public string ShortDescription;
        public string LongDescription;
    }
}
