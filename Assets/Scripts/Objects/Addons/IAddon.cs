using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAddon
{
    public void Update(double deltaTime);

    public string Serialize();
    public void Deserialize(string data);
}
