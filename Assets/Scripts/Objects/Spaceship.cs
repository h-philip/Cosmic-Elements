using Components;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CelestialBody))]
public class Spaceship : MonoBehaviour, ISpaceship
{
    /// <summary>
    /// Diameter (in km) used to set ships' scales.
    /// </summary>
    public const float DIAMETER = 1f;

    public bool PlayerControlled;
    public double Food { get; set; }
    public double Water { get; set; }
    public double Iron { get; set; }
    public double Copper { get; set; }
    public double RareMinerals { get; set; }
    public double Energy { get; set; }
    /// <summary>
    /// Mass in kg
    /// </summary>
    public double Mass { get; private set; }

    private Dictionary<IEngine, Components.Vector3> _perEngineThrusts = new Dictionary<IEngine, Components.Vector3>();
    /// <summary>
    /// Directed thrust in kN
    /// </summary>
    public Components.Vector3 Thrust { get; private set; } = new Components.Vector3(0,0,0);
    public IAddon[] Addons;

    private Component[] _components = new Component[0];
    public Component[] Components
    {
        get
        {
            return _components;
        }
        set
        {
            _components = value;
            double mass = 0;
            foreach (Component component in Components)
                try
                {
                    mass += component.Mass;
                }
                catch (NullReferenceException e)
                {
                    Debug.LogWarning("Component in Spaceship is null: " + e.Message);
                }
            Mass = mass;
        }
    }

    private Guid _id;
    public Guid Id
    {
        get
        {
            if (_id == Guid.Empty)
                _id = Guid.NewGuid();
            return _id;
        }
        set => _id = value;
    }

    public ControlScript Script = null;

    private CelestialBody _celestialBody;
    private Animator _animator;

    public virtual string Location
    {
        get
        {
            // TODO
            return "Orbit around " + GetComponent<CelestialBody>().Attractor.name;
        }
    }

    public virtual string Communication
    {
        get
        {
            return CanCommunicate ? "Yes" : "No"; // TODO
        }
    }

    public virtual bool CanCommunicate
    {
        get
        {
            return true; // TODO
        }
    }

    public virtual bool IsControlStation => false;

    public Components.Vector3 Position
    {
        get
        {
            try
            {
                SimpleKeplerOrbits.KeplerOrbitData data = GetComponent<CelestialBody>().KeplerOrbitData;
                return new Components.Vector3(data.Position.x, data.Position.y, data.Position.z);
            }
            catch (NullReferenceException)
            {
                return new Components.Vector3(0, 0, 0);
            }
        }
    }

    public Components.Vector3 Velocity
    {
        get
        {
            try
            {
                SimpleKeplerOrbits.KeplerOrbitData data = GetComponent<CelestialBody>().KeplerOrbitData;
                return new Components.Vector3(data.Velocity.x, data.Velocity.y, data.Velocity.z);
            }
            catch (NullReferenceException)
            {
                return new Components.Vector3(0, 0, 0);
            }
        }
    }

    protected void OnEnable()
    {
        if (!IsControlStation)
            FindObjectOfType<SpaceSimulation>().UpdateSpaceships();
        else
            FindObjectOfType<SpaceSimulation>().UpdateControlStations();
        if (Script != null)
            Script.Start();
    }
    protected void OnDisable()
    {
        if (Script != null)
            Script.Stop();
    }

    protected void OnDestroy()
    {
        try
        {
            if (!IsControlStation)
                FindObjectOfType<SpaceSimulation>().UpdateSpaceships();
            else
                FindObjectOfType<SpaceSimulation>().UpdateControlStations();
        }
        catch (NullReferenceException)
        { }
    }

    protected void Start()
    {
        _celestialBody = GetComponent<CelestialBody>();
        _animator = GetComponent<Animator>();
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
        Addons = GetComponents<IAddon>();
        if (Script != null)
            Script.Start();
    }

    public void SimulationUpdate(double deltaTime)
    {
        // Update components and script
        foreach (Component component in Components)
            component.Update(deltaTime);
        if (Script != null) // TODO: Remove
            Script.Update(deltaTime);

        // Update acceleration
        SimpleKeplerOrbits.Vector3d accel = new SimpleKeplerOrbits.Vector3d(Thrust.x, Thrust.y, Thrust.z);
        //accel *= 1000d;
        accel /= Mass;
        //// Acceleration now is in m/s, but we need km/s
        //accel /= 1000;
        _celestialBody.KeplerOrbitData.Velocity += accel * deltaTime;
        _celestialBody.KeplerOrbitData.CalculateOrbitStateFromOrbitalVectors();

        foreach (IAddon simulated in Addons)
        {
            simulated.Update(deltaTime);
        }
    }

    public static Spaceship[] GetAllSpaceships(bool onlyPlayerControlled, bool includeInactive = false)
    {
        Spaceship[] spaceships = FindObjectsOfType<Spaceship>(includeInactive);
        List<Spaceship> realSpaceships = new List<Spaceship>();
        foreach (Spaceship spaceship in spaceships)
            if (!spaceship.IsControlStation && (onlyPlayerControlled ? spaceship.PlayerControlled : true))
                realSpaceships.Add(spaceship);
        return realSpaceships.ToArray();
    }

    public void SetThrust(IEngine engine, Components.Vector3 thrust)
    {
        if (thrust == null || (thrust.x == 0 && thrust.y == 0 && thrust.z == 0))
        {
            if (_perEngineThrusts.ContainsKey(engine))
                _perEngineThrusts.Remove(engine);
        }
        else
        {
            _perEngineThrusts[engine] = thrust;
        }
        Components.Vector3 newThrust = new Components.Vector3(0,0,0);
        foreach (Components.Vector3 t in _perEngineThrusts.Values)
            newThrust += t;
        Thrust = newThrust;

        // Animation
        if (_animator != null)
        {
            if (Thrust != null && Thrust.magnitude > 0)
                _animator.SetBool("Accelerate", true);
            else
                _animator.SetBool("Accelerate", false);
        }
    }

    public IComponent[] GetComponents()
    {
        return _components;
    }
}
