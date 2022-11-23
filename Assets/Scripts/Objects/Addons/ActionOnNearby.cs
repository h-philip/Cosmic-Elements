using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This can be used to trigger an action when a player spaceship is close to the celestial body.
/// </summary>
[RequireComponent(typeof(Spaceship))]
[Serializable]
public class ActionOnNearby : MonoBehaviour, IAddon
{
    /// <summary>
    /// Trigger range in km.
    /// </summary>
    public double Range = 20;
    /// <summary>
    /// Action to trigger on nearby. Parameter is the ship that triggered this.
    /// </summary>
    public UnityAction<Spaceship> OnNearby;
    public bool TriggerOnNoComms = false;
    public bool TriggerOnControlStation = false;
    /// <summary>
    /// Array of components that the player spaceship needs to have installed and powered in order to trigger the action.
    /// </summary>
    public HashSet<Type> RequiredComponents;
    public bool Triggered = false;

    private Spaceship _spaceship;

    private void Awake()
    {
        _spaceship = GetComponent<Spaceship>();
    }

    void IAddon.Update(double deltaTime)
    {
        if (Triggered) return;

        // Get spaceships in range
        foreach (Spaceship spaceship in GameObject.FindObjectsOfType<Spaceship>())
        {
            bool triggered = spaceship != _spaceship && (spaceship.Position - _spaceship.Position).Magnitude < Range;
            if (!TriggerOnNoComms)
                triggered &= spaceship.CanCommunicate;
            if (!TriggerOnControlStation)
                triggered &= !spaceship.IsControlStation;
            if (triggered && RequiredComponents != null && RequiredComponents.Count > 0)
            {
                foreach (Type type in RequiredComponents)
                {
                    bool found = false;
                    foreach (Components.IComponent component in spaceship.Components)
                    {
                        if (component.GetType() == type && component.Powered)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        triggered = false;
                        break;
                    }
                }
            }
            if (triggered)
            {
                OnNearby(spaceship);
                Triggered = true;
                break;
            }

        }
    }

    string IAddon.Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    void IAddon.Deserialize(string data)
    {
        JsonUtility.FromJsonOverwrite(data, this);
    }
}
