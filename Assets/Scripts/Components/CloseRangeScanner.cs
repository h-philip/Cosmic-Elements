using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;
[Serializable]
public class CloseRangeScanner : Component, ICloseRangeScanner
{
    // Component
    public override double EnergyConsumption => 0;
    public override double Mass => 10;
    public override double Energy { get => 1000; }
    public override int Iron { get => 10; }
    public override int Copper { get => 5; }
    public override int RareMinerals { get => 1; }
    public override string ShortDescription { get => _shortDescription; }
    public override string LongDescription { get => _longDescription; }

    // ICloseRangeScanner
    public double Range => 50;
    public double EnergyPerScan => 1000;

    // Own

    // Private fields
    private string _shortDescription = @"TODO";
    private string _longDescription = @"TODO";

    // Constructor
    public CloseRangeScanner(Spaceship spaceship) : base(spaceship)
    {
        // Nothing
    }

    // Methods
    public override void Install()
    {
        // Nothing
    }

    public override void Uninstall()
    {
        // Nothing
    }

    public override void Update(double deltatime)
    {
        // Nothing
    }

    public ICloseRangeScanner.ScanObject[] Scan(double range)
    {
        // Cap range
        range = System.Math.Min(range, Range);
        // Check if there's enough energy
        if (range / Range * EnergyPerScan > Spaceship.Energy)
            return null;
        // Get spaceships
        List<ICloseRangeScanner.ScanObject> result = new List<ICloseRangeScanner.ScanObject>();
        foreach (Spaceship spaceship in GameObject.FindObjectsOfType<Spaceship>())
        {
            if (spaceship != Spaceship && (spaceship.Position - Spaceship.Position).Magnitude < range)
                result.Add(new ICloseRangeScanner.ScanObject(spaceship.Position - Spaceship.Position, spaceship.Velocity - Spaceship.Velocity, spaceship.IsControlStation));
        }
        // Subtract energy
        Spaceship.Energy -= range / Range * EnergyPerScan;
        // Return
        return result.ToArray();
    }
}
