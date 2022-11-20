using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;
[Serializable]
public class ProportionalNavigationUnit : Component, IProportionalNavigationUnit
{
    // Component
    public override double EnergyConsumption => 0.1;
    public override double Mass => 5;
    public override double Energy => 100;
    public override int Iron => 1;
    public override int Copper => 2;
    public override int RareMinerals => 1;
    public override string ShortDescription { get => _shortDescription; }
    public override string LongDescription { get => _longDescription; }

    // IProportionalNavigationUnit
    public double ProportionalityConstant { get; set; }
    public double TargetReachedMaxDistance { get; set; }
    public double MaxThrustUsage { get; set; }
    public IProportionalNavigationUnit.TargetReachedAction OnTargetReached { get; set; }

    // Own

    // Private fields
    [SerializeField]
    private Components.Vector3 _originalTargetPosition, _targetVelocity;
    [SerializeField]
    private double _timeSincePositionSet;
    private string _shortDescription = @"TODO";
    private string _longDescription = @"TODO";

    // Constructor
    public ProportionalNavigationUnit(Spaceship spaceship) : base(spaceship)
    {
        State = IComponent.OnOff.Off;
    }

    // Methods
    public void SetTargetPosition(Components.Vector3 targetPosition)
    {
        _timeSincePositionSet = 0;
        _originalTargetPosition = targetPosition;
    }

    public void SetTargetVelocity(Components.Vector3 targetVelocity)
    {
        SetTargetPosition(_originalTargetPosition + _targetVelocity * _timeSincePositionSet);
        _targetVelocity = targetVelocity;
    }

    public void StartNavigation()
    {
        State = IComponent.OnOff.On;
    }

    public void StopNavigation()
    {
        State = IComponent.OnOff.Off;
    }

    public override void Update(double deltatime)
    {
        base.Update(deltatime);
        if (State == IComponent.OnOff.Off)
            return;
        // TODO
    }
}
