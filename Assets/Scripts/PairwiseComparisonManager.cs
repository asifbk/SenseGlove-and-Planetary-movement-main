using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PairwiseComparisonManager : MonoBehaviour
{
    [Header("Bucket References")]
    public GameObject heavierBucket;
    public GameObject lighterBucket;
    
    [Header("Visual Feedback")]
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    public Color neutralColor = Color.white;
    
    [Header("Settings")]
    public float comparisonCooldown = 1f;
    public float feedbackDisplayTime = 2f;
    
    [Header("UI References")]
    public GameObject proceedButton;
    
    private StudyManager studyManager;
    private Dictionary<string, float> planetMasses;
    private HashSet<string> planetsInHeavierBucket = new HashSet<string>();
    private HashSet<string> planetsInLighterBucket = new HashSet<string>();
    private float lastComparisonTime;
    private Renderer heavierRenderer;
    private Renderer lighterRenderer;
    private bool hasCompletedOnce = false;
    private Color heavierBucketOriginalColor;
    private Color lighterBucketOriginalColor;
    private UnityEngine.Coroutine colorResetCoroutine;
    
    void Start()
    {
        studyManager = FindObjectOfType<StudyManager>();
        
        if (heavierBucket != null)
        {
            heavierRenderer = heavierBucket.GetComponent<Renderer>();
            if (heavierRenderer != null)
                heavierBucketOriginalColor = heavierRenderer.material.color;
        }
        
        if (lighterBucket != null)
        {
            lighterRenderer = lighterBucket.GetComponent<Renderer>();
            if (lighterRenderer != null)
                lighterBucketOriginalColor = lighterRenderer.material.color;
        }
        
        InitializePlanetMasses();
        ResetBucketColors();
    }
    
    void InitializePlanetMasses()
    {
        planetMasses = new Dictionary<string, float>
        {
            { "Jupiter", 1898f },
            { "Saturn", 568f },
            { "Neptune", 102f },
            { "Uranus", 86.8f },
            { "Earth", 5.97f },
            { "Venus", 4.87f },
            { "Mars", 0.642f },
            { "Mercury", 0.330f }
        };
    }
    
    public void OnTaskStarted()
    {
        planetsInHeavierBucket.Clear();
        planetsInLighterBucket.Clear();
        ResetBucketColors();
        hasCompletedOnce = false;
        
        if (proceedButton != null)
            proceedButton.SetActive(false);
        
        Debug.Log("[PairwiseComparison] Task started - compare planetary masses!");
    }
    
    public void OnPlanetEnteredBucket(string planetName, bool isHeavierBucket)
    {
        if (Time.time - lastComparisonTime < comparisonCooldown)
            return;
        
        if (isHeavierBucket)
        {
            planetsInHeavierBucket.Add(planetName);
            planetsInLighterBucket.Remove(planetName);
        }
        else
        {
            planetsInLighterBucket.Add(planetName);
            planetsInHeavierBucket.Remove(planetName);
        }
        
        ValidateComparison();
    }
    
    public void OnPlanetExitedBucket(string planetName, bool isHeavierBucket)
    {
        if (isHeavierBucket)
            planetsInHeavierBucket.Remove(planetName);
        else
            planetsInLighterBucket.Remove(planetName);
        
        if (planetsInHeavierBucket.Count == 0 && planetsInLighterBucket.Count == 0)
        {
            ResetBucketColors();
        }
    }
    
    void ValidateComparison()
    {
        if (planetsInHeavierBucket.Count == 0 || planetsInLighterBucket.Count == 0)
        {
            ResetBucketColors();
            return;
        }
        
        bool isValid = true;
        
        foreach (string heavyPlanet in planetsInHeavierBucket)
        {
            if (!planetMasses.ContainsKey(heavyPlanet)) continue;
            
            float heavyMass = planetMasses[heavyPlanet];
            
            foreach (string lightPlanet in planetsInLighterBucket)
            {
                if (!planetMasses.ContainsKey(lightPlanet)) continue;
                
                float lightMass = planetMasses[lightPlanet];
                
                if (heavyMass <= lightMass)
                {
                    isValid = false;
                    break;
                }
            }
            
            if (!isValid) break;
        }
        
        lastComparisonTime = Time.time;
        
        if (colorResetCoroutine != null)
        {
            StopCoroutine(colorResetCoroutine);
        }
        
        if (isValid)
        {
            SetBucketColors(correctColor, correctColor);
            Debug.Log($"[PairwiseComparison] ✓ CORRECT! {GetHeaviestPlanet(planetsInHeavierBucket)} > {GetLightestPlanet(planetsInLighterBucket)}");
            
            if (!hasCompletedOnce)
            {
                hasCompletedOnce = true;
                ShowProceedButton();
            }
        }
        else
        {
            SetBucketColors(incorrectColor, incorrectColor);
            Debug.Log($"[PairwiseComparison] ✗ INCORRECT! {GetHeaviestPlanet(planetsInHeavierBucket)} is NOT heavier than {GetLightestPlanet(planetsInLighterBucket)}");
        }
        
        colorResetCoroutine = StartCoroutine(ResetColorsAfterDelay());
    }
    
    void ShowProceedButton()
    {
        if (proceedButton != null)
        {
            proceedButton.SetActive(true);
            Debug.Log("[PairwiseComparison] Proceed button shown - user can advance to classification!");
        }
    }
    
    void SetBucketColors(Color heavyColor, Color lightColor)
    {
        if (heavierRenderer != null)
            heavierRenderer.material.color = heavyColor;
        
        if (lighterRenderer != null)
            lighterRenderer.material.color = lightColor;
    }
    
    void ResetBucketColors()
    {
        SetBucketColors(heavierBucketOriginalColor, lighterBucketOriginalColor);
    }
    
    System.Collections.IEnumerator ResetColorsAfterDelay()
    {
        yield return new UnityEngine.WaitForSeconds(feedbackDisplayTime);
        ResetBucketColors();
        Debug.Log("[PairwiseComparison] Buckets returned to original color after feedback display");
    }
    
    string GetHeaviestPlanet(HashSet<string> planets)
    {
        if (planets.Count == 0) return "None";
        
        string heaviest = planets.First();
        float maxMass = planetMasses.ContainsKey(heaviest) ? planetMasses[heaviest] : 0f;
        
        foreach (string planet in planets)
        {
            if (planetMasses.ContainsKey(planet) && planetMasses[planet] > maxMass)
            {
                heaviest = planet;
                maxMass = planetMasses[planet];
            }
        }
        
        return heaviest;
    }
    
    string GetLightestPlanet(HashSet<string> planets)
    {
        if (planets.Count == 0) return "None";
        
        string lightest = planets.First();
        float minMass = planetMasses.ContainsKey(lightest) ? planetMasses[lightest] : float.MaxValue;
        
        foreach (string planet in planets)
        {
            if (planetMasses.ContainsKey(planet) && planetMasses[planet] < minMass)
            {
                lightest = planet;
                minMass = planetMasses[planet];
            }
        }
        
        return lightest;
    }
}
