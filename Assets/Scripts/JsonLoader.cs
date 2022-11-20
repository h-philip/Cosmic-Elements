using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class JsonLoader : MonoBehaviour
{
	[Serializable]
	public class JPLListContainer
	{
		public JPLElementsData[] OrbitsData = new JPLElementsData[0];
	}

	/// <summary>
	/// Data container for single body orbit.
	/// </summary>
	[Serializable]
	public class JPLElementsData
	{
		public string BodyName;
		public string AttractorName;
		public float AttractorMass;

		/// <summary>
		/// Eccentricity.
		/// </summary>
		[Tooltip("Eccentricity")]
		public double EC;

		/// <summary>
		/// Inclination (degrees).
		/// </summary>
		[Tooltip("Inclination")]
		public double IN;

		/// <summary>
		/// Longitude of Ascending Node (degrees).
		/// </summary>
		[Tooltip("Ascending node longitude")]
		public double OM;

		/// <summary>
		/// Argument of perifocus (degrees).
		/// </summary>
		[Tooltip("Argument of periapsis")]
		public double W;

		/// <summary>
		/// Mean anomaly (degrees).
		/// </summary>
		[Tooltip("Mean anomaly")]
		public double MA;

		/// <summary>
		/// Semi-major axis (au).
		/// </summary>
		[Tooltip("Semi-major axis")]
		public double A;
	}

	public double GConst = 6.674e-11d;
	public double Scale = 1;
	public Camera Camera;
	public float MaxDistanceToCamera = 1000;
	public CelestialBody BodyTemplate;
	public TextAsset JsonData;

	private Transform _root;

	private void Start()
	{
		if (JsonData != null)
		{
			JPLListContainer data = JsonUtility.FromJson<JPLListContainer>(JsonData.text);
			if (data != null)
			{
				foreach (JPLElementsData item in data.OrbitsData)
                {
					var attractorName = item.AttractorName != null ? item.AttractorName.Trim() : "";
					CelestialBody attractor = null;
					if (item.AttractorName != "")
                    {
						attractor = GameObject.Find(attractorName).GetComponent<CelestialBody>();
						attractor.Mass = item.AttractorMass;
					}
					CelestialBody body = Instantiate(BodyTemplate, attractor != null ? attractor.transform : null);
					body.name = item.BodyName;
					body.EC = item.EC;
					body.A = item.A;
					body.MA = item.MA;
					body.IN = item.IN;
					body.W = item.W;
					body.OM = item.OM;
					body.AttractorMass = item.AttractorMass;
					body.Attractor = attractor != null ? attractor.transform : this.transform;
					body.Init();
                }
			}
		}
	}
}
