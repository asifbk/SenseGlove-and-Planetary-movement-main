using UnityEngine;
using System.Collections.Generic;

public class SolarSystemController : MonoBehaviour
{
    [System.Serializable]
    public class PlanetData
    {
        public Transform planet;
        public float rotationSpeed;
        public Vector3 rotationAxis = Vector3.right;
    }
    
    [Header("Planet Rotations")]
    public List<PlanetData> planets = new List<PlanetData>();
    
    [Header("Speed Settings")]
    public float globalSpeedMultiplier = 1f;
    public bool autoFindPlanets = true;
    
    void Start()
    {
        if (autoFindPlanets)
        {
            FindAndSetupPlanets();
        }
    }
    
    void FindAndSetupPlanets()
    {
        planets.Clear();
        
        // Get all direct children (planets)
        foreach (Transform child in transform)
        {
            if (child.name != "SolarPanel") // Skip the SolarPanel itself if it's a child
            {
                PlanetData newPlanet = new PlanetData();
                newPlanet.planet = child;
                newPlanet.rotationSpeed = Random.Range(5f, 25f);
                newPlanet.rotationAxis = Vector3.right; // X-axis
                
                planets.Add(newPlanet);
            }
        }
    }
    
    void Update()
    {
        foreach (PlanetData planet in planets)
        {
            if (planet.planet != null)
            {
                // Rotate each planet around its local X-axis
                planet.planet.Rotate(planet.rotationAxis, 
                    planet.rotationSpeed * globalSpeedMultiplier * Time.deltaTime, 
                    Space.Self);
            }
        }
    }
    
    // Optional: Manual control methods
    public void SetPlanetSpeed(string planetName, float newSpeed)
    {
        foreach (PlanetData planet in planets)
        {
            if (planet.planet.name == planetName)
            {
                planet.rotationSpeed = newSpeed;
                break;
            }
        }
    }
    
    public void SetAllSpeeds(float speedMultiplier)
    {
        globalSpeedMultiplier = speedMultiplier;
    }
}