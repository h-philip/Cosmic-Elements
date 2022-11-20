using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;
[Serializable]
public class SmallSolarPanel : Component, ISolarPanel
{
    // Component
    public override double EnergyConsumption => 0f;
    public override double Energy => 50f;
    public override double Mass => 10;
    public override int Iron { get => 1; }
    public override int Copper { get => 2; }
    public override int RareMinerals { get => 0; }
    public override string ShortDescription { get => _shortDescription; }
    public override string LongDescription { get => _longDescription; }

    // ISolarPanel
    public double EnergyProduction => 1f;

    // Own

    // Private fields
    private string _shortDescription = @"TODO";
    private string _longDescription = @"TODO";

    // Constructor
    public SmallSolarPanel(Spaceship spaceship) : base(spaceship)
    {
        // Nothing
    }

    // Methods

    public override void Update(double deltatime)
    {
        Spaceship.Energy += deltatime * EnergyProduction;
    }
}
