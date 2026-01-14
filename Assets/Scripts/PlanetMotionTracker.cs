using UnityEngine;
using System.Collections.Generic;
using SG;

public class PlanetMotionTracker : MonoBehaviour
{
    [System.Serializable]
    public class PlanetMotionData
    {
        public Transform planet;
        public Vector3 velocity;
        public Vector3 acceleration;
        public Vector3 previousPosition;
        public Vector3 previousVelocity;
    }

    public List<PlanetMotionData> planets = new List<PlanetMotionData>();
    public Transform currentlyGrabbedPlanet { get; private set; }

    private void Start()
    {
        foreach (Transform child in transform)
        {
            PlanetMotionData data = new PlanetMotionData
            {
                planet = child,
                previousPosition = child.position,
                velocity = Vector3.zero,
                acceleration = Vector3.zero,
                previousVelocity = Vector3.zero
            };
            planets.Add(data);
        }
    }

    private void Update()
    {
        currentlyGrabbedPlanet = null;

        foreach (PlanetMotionData data in planets)
        {
            if (data.planet != null)
            {
                Vector3 currentPosition = data.planet.position;
                
                data.velocity = (currentPosition - data.previousPosition) / Time.deltaTime;
                
                data.acceleration = (data.velocity - data.previousVelocity) / Time.deltaTime;
                
                data.previousPosition = currentPosition;
                data.previousVelocity = data.velocity;

                SG_Grabable grabable = data.planet.GetComponent<SG_Grabable>();
                if (grabable != null && grabable.IsGrabbed())
                {
                    currentlyGrabbedPlanet = data.planet;
                }
            }
        }
    }

    public PlanetMotionData GetPlanetData(string planetName)
    {
        return planets.Find(p => p.planet != null && p.planet.name == planetName);
    }

    public PlanetMotionData GetPlanetData(Transform planet)
    {
        return planets.Find(p => p.planet == planet);
    }
}
