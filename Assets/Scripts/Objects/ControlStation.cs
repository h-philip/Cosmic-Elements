using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlStation : Spaceship
{
    public int Population { get; set; }

    public override string Location => GetComponent<CelestialBody>().Attractor.name;

    public CelestialBody OrbitedBody { get; private set; }

    public override bool IsControlStation => true;

    private new void Start()
    {
        base.Start();
        OrbitedBody = GetComponent<CelestialBody>().Attractor.GetComponent<CelestialBody>();
    }

    public int GetCommunicationStrength()
    { return 0; }

    public bool HasResources(Blueprint blueprint)
    {
        return Energy >= blueprint.Energy
            && Iron >= blueprint.Iron
            && Copper >= blueprint.Copper
            && RareMinerals >= blueprint.RareMinerals;
    }

    public static ControlStation[] GetAllControlStations(bool onlyPlayerControlled, bool includeInactive = false)
    {
        ControlStation[] controlStations = FindObjectsOfType<ControlStation>(includeInactive);
        if (onlyPlayerControlled)
        {
            List<ControlStation> realControlStations = new List<ControlStation>();
            foreach (ControlStation station in controlStations)
                if (station.PlayerControlled)
                    realControlStations.Add(station);
            return realControlStations.ToArray();
        }
        else
            return controlStations;
    }
}
