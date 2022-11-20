using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Components;
[Serializable]
public class BasicEngine : Component, IEngine
{
    // Component
    public override double EnergyConsumption => MaxEnergyConsumption * _thrustPercentage;
    public override double Energy => 200f;
    public override double Mass => 50;
    public override int Iron { get => 10; }
    public override int Copper { get => 10; }
    public override int RareMinerals { get => 0; }
    public override string ShortDescription { get => _shortDescription; }
    public override string LongDescription { get => _longDescription; }

    // IEngine
    public double Thrust => 1;
    public Components.Vector3 CurrentThrust => _thrustDirection.normalized * _thrustPercentage * Thrust;
    public double MaxEnergyConsumption => 10f;

    // Own

    // Private fields
    [SerializeField]
    private double _thrustPercentage = 0;
    [SerializeField]
    private Components.Vector3 _thrustDirection;
    private string _shortDescription = @"TODO";
    private string _longDescription = @"TODO";

    // Constructor
    public BasicEngine(Spaceship spaceship) : base(spaceship)
    {
        if (_thrustDirection == null)
            _thrustDirection = new Components.Vector3(0, 0, 0);
        SetThrust(_thrustPercentage, _thrustDirection);
    }

    // Methods
    public override void Update(double deltaTime)
    {
        bool wasPowered = Powered;
        base.Update(deltaTime);
        if (Powered && !wasPowered)
            Spaceship.SetThrust(this, CurrentThrust);
        else if (!Powered && wasPowered)
            Spaceship.SetThrust(this, null);
    }

    public double SetThrust(double thrustFraction, Components.Vector3 thrustDirection)
    {
        thrustFraction = Math.Min(1, thrustFraction);
        thrustFraction = Math.Max(0, thrustFraction);
        _thrustDirection = thrustDirection;
        _thrustPercentage = thrustFraction;
        if (Spaceship != null)
            Spaceship.SetThrust(this, CurrentThrust);
        return thrustFraction;
    }
}
