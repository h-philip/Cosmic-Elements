using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;
[Serializable]
public class BasicStructure : Component, IBasicStructure
{
    // Component
    public override double EnergyConsumption => 1;
    public override double Mass => 100;
    public override double Energy { get => 1000; }
    public override int Iron { get => 10; }
    public override int Copper { get => 5; }
    public override int RareMinerals { get => 0; }
    public override string ShortDescription { get => _shortDescription; }
    public override string LongDescription
    {
        get
        {
            return string.Format(
                _longDescription,
                TopBarController.FormatNumber(FoodCapacity),
                TopBarController.FormatNumber(WaterCapacity),
                TopBarController.FormatNumber(IronCapacity),
                TopBarController.FormatNumber(CopperCapacity),
                TopBarController.FormatNumber(RareMineralsCapacity),
                TopBarController.FormatNumber(EnergyCapacity)
                );
        }
    }

    // IBasicStructure
    public double FoodCapacity => 0;
    public double WaterCapacity => 0;
    public double IronCapacity => 0;
    public double CopperCapacity => 0;
    public double RareMineralsCapacity => 0;
    public double EnergyCapacity => 1000;

    // Own

    // Private fields
    private string _shortDescription = @"Core component of every spaceship and control station.";
    private string _longDescription = @"This component is and has to be installed on every spaceship and control station.

== Storage ==
This component allows for the storage of up to:
- {0} Food
- {1} Water
- {2} Iron
- {3} Copper
- {4} Raw Minerals
- {5} Energy";


    // Constructor
    public BasicStructure(Spaceship spaceship) : base(spaceship)
    {
    }

    // Methods
    public override void Install()
    {
        Spaceship.Energy += 500;
    }
}
