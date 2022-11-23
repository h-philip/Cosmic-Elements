using System;
using System.Collections;
using System.Collections.Generic;
using Components;

namespace ControlScripts
{
    public class Follow : ControlScript
    {
        private ICloseRangeScanner _scanner;
        private IEngine[] _engines;
        private ICloseRangeScanner.ScanObject _target;
        private bool _targetFound = false;
        private double _time, _lastScan;

        private double _distance => _target.RelativePosition.Magnitude;

        private const double MAX_RANGE = 2;

        public Follow() : base(nameof(Follow))
        {
        }

        public override void Start()
        {
            try
            {
                _scanner = Spaceship.GetComponentsOfType<ICloseRangeScanner>()[0];
                _engines = Spaceship.GetComponentsOfType<IEngine>();
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
        }

        public override void Update(double deltatime)
        {
            _time += deltatime;
            if (_scanner == null) return;
            if (!_targetFound || _time > _lastScan + 5)
            {
                double range = _targetFound ? _distance + 5 : 25;
                var found = _scanner.Scan(range);
                if (found == null) return; // Not enough energy
                _targetFound = found.Length > 0;
                if (_targetFound)
                {
                    _target = found[0];
                    _lastScan = _time;
                }
                else
                    return;
            }
            Vector3d thrust = null;
            //if (_target.RelativeVelocity.magnitude > 1)
            //    thrust += _target.RelativeVelocity;
            if (_distance > MAX_RANGE)
                thrust = _target.RelativePosition;
            else if (_target.RelativeVelocity.Magnitude > 1)
                thrust = _target.RelativeVelocity;
            Array.ForEach(_engines, engine => engine.SetThrust(engine.Thrust, thrust));
        }
    }
}
