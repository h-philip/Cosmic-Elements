using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components;

namespace ControlScripts
{
    public class EC1Testing : ControlScript
    {
        private enum MoveMode {
            VelocityCancel,
            PositionApproach
        }

        private MoveMode _moveMode;
        private IEngine[] _engines;
        private Spaceship _thisShip;
        private Spaceship _ecSpaceship;
        private ControlStation _earthControlStation;
        private Spaceship _target;
        private double _closestApproach;

        public EC1Testing() : base(Contracts.EcIntroduction.SCRIPT_NAME)
        {
        }

        private bool IsInContract => Contracts.EcIntroduction.AnyRunning;
        private Components.Vector3 TargetRelativeVelocity => _target.Velocity - _thisShip.Velocity;
        private Components.Vector3 TargetRelativePosition => _target.Position - _thisShip.Position;
        private double MaxThrust { get { double max = 0; Array.ForEach(_engines, _ => max += _.Thrust); return max; } }
        private double MaxAcceleration => MaxThrust / Spaceship.Mass;

        public override void Start()
        {
            if (!IsInContract)
                return;
            _thisShip = Array.Find(global::Spaceship.GetAllSpaceships(true), _ => _.Script == this);
            _ecSpaceship = Array.Find(global::Spaceship.GetAllSpaceships(false), _ => _.name == "EC Spaceship");
            _earthControlStation = Array.Find(ControlStation.GetAllControlStations(false), _ => _.Location == "Earth");
            _engines = Spaceship.GetComponentsOfType<IEngine>(); // TODO: Change engine
            if (Storage == null)
                Storage = "";
            string[] split = Storage.Split(';');
            if (split[0] == "Station")
                _target = _earthControlStation;
            else if (split[0] == "Done")
                _target = null;
            else
                _target = _ecSpaceship;
            if (split.Length > 1 && split[1] == "PositionApproach")
                _moveMode = MoveMode.PositionApproach;
            else
                _moveMode = MoveMode.VelocityCancel;
            _closestApproach = double.PositiveInfinity;
            if (_engines.Length > 0 && Spaceship.GetComponentsOfType<SmallSolarPanel>().Length >= 2)
                Contracts.EcIntroduction.ShipExists = true;
        }

        public override void Update(double deltatime)
        {
            if (_target == null || !IsInContract)
                return;
            if (_moveMode == MoveMode.VelocityCancel && TargetRelativeVelocity.magnitude < 0.01)
                _moveMode = MoveMode.PositionApproach;
            else if (_moveMode == MoveMode.PositionApproach && TargetRelativePosition.magnitude < 3)
                _moveMode = MoveMode.VelocityCancel;
            _closestApproach = TargetRelativePosition.magnitude;

            Components.Vector3 flyPath;
            if (_moveMode == MoveMode.VelocityCancel)
            {
                flyPath = TargetRelativeVelocity;
            }
            else
            {
                flyPath = TargetRelativePosition;
            }
            double maxSpeedInDeltaTime = deltatime * MaxAcceleration;
            double thrustFraction = flyPath.magnitude / maxSpeedInDeltaTime;

            double breakTime = Math.Sqrt((2 * flyPath.magnitude) / MaxAcceleration);
            bool accelerate = MaxAcceleration * breakTime > TargetRelativeVelocity.magnitude;
            accelerate |= (TargetRelativePosition + TargetRelativeVelocity).magnitude > TargetRelativePosition.magnitude;
            //Debug.Log(accelerate + " - " + TargetRelativePosition.magnitude + " - " + _moveMode);

            Components.Vector3 thrust;
            if (accelerate)
                thrust = flyPath;
            else
                thrust = flyPath * -1;

            _thisShip.Energy = 2000;
            Array.ForEach(_engines, engine => engine.SetThrust(thrustFraction, thrust));
            if (TargetRelativePosition.magnitude <= 3 && TargetRelativeVelocity.magnitude < 1)
                TargetReached();
        }

        public override void Stop()
        {
            if (_target == _earthControlStation)
                Storage = "Station";
            else if (_target == null)
                Storage = "Done";
            if (_moveMode == MoveMode.PositionApproach)
                Storage += ";PositionApproach";
            else
                Storage += ";VelocityCancel";
        }

        private void TargetReached()
        {
            if (_target != null)
                Debug.Log("Reached target " + _target.name);
            if (_target == _ecSpaceship)
            {
                _target = _earthControlStation;
                Contracts.EcIntroduction.ShipReachedECSpaceship = true;
            }
            else if (_target == _earthControlStation)
            {
                _target = null;
                Contracts.EcIntroduction.ShipIsBackHome = true;
            }
            Array.ForEach(_engines, engine => engine.SetThrust(0, new Components.Vector3()));
        }
    }
}
