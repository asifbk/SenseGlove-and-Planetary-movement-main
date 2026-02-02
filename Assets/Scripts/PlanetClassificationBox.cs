using UnityEngine;
using System.Collections.Generic;

public enum PlanetType
{
    Rocky,
    Gaseous
}

public class PlanetClassificationBox : MonoBehaviour
{
    [Header("Box Settings")]
    public PlanetType boxType;
    public Color normalColor = Color.white;
    public Color correctColor = Color.green;
    public Color incorrectColor = Color.red;
    
    [Header("Detection")]
    public float detectionRadius = 0.5f;
    public float resetDelayTime = 0.5f;
    
    private MeshRenderer boxRenderer;
    private List<GameObject> planetsInBox = new List<GameObject>();
    private Material boxMaterial;
    private bool detectionEnabled = true;
    private float detectionResumeTime = 0f;
    
    void Start()
    {
        boxRenderer = GetComponent<MeshRenderer>();
        if (boxRenderer != null)
        {
            boxMaterial = boxRenderer.material;
            ResetBoxColor();
        }
    }
    
    public void ResetBoxColor()
    {
        planetsInBox.Clear();
        if (boxMaterial != null)
        {
            boxMaterial.color = normalColor;
        }
        
        detectionEnabled = false;
        detectionResumeTime = Time.time + resetDelayTime;
        
        Debug.Log($"[PlanetClassificationBox] {boxType} box reset - detection disabled for {resetDelayTime}s");
    }
    
    void Update()
    {
        if (!detectionEnabled && Time.time >= detectionResumeTime)
        {
            detectionEnabled = true;
            Debug.Log($"[PlanetClassificationBox] Detection re-enabled for {boxType} box");
        }
        
        if (detectionEnabled)
        {
            DetectPlanetsInBox();
        }
    }
    
    void DetectPlanetsInBox()
    {
        planetsInBox.Clear();
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Planet") || col.GetComponent<FloatingObjectInfo>() != null)
            {
                planetsInBox.Add(col.gameObject);
                float distance = Vector3.Distance(transform.position, col.transform.position);
                Debug.Log($"[PlanetClassificationBox] {boxType} box detected {col.gameObject.name} at distance {distance:F2} units");
            }
        }
        
        UpdateBoxColor();
    }
    
    void UpdateBoxColor()
    {
        if (planetsInBox.Count == 0)
        {
            if (boxMaterial != null)
                boxMaterial.color = normalColor;
            return;
        }
        
        bool allCorrect = true;
        
        foreach (GameObject planet in planetsInBox)
        {
            PlanetType planetType = GetPlanetType(planet);
            
            if (planetType != boxType)
            {
                allCorrect = false;
                break;
            }
        }
        
        if (boxMaterial != null)
        {
            boxMaterial.color = allCorrect ? correctColor : incorrectColor;
        }
    }
    
    PlanetType GetPlanetType(GameObject planet)
    {
        SG.SG_MeshDeform meshDeform = planet.GetComponent<SG.SG_MeshDeform>();
        
        if (meshDeform != null)
        {
            return PlanetType.Gaseous;
        }
        else
        {
            return PlanetType.Rocky;
        }
    }
    
    public bool HasCorrectPlanets()
    {
        if (planetsInBox.Count == 0)
            return false;
        
        foreach (GameObject planet in planetsInBox)
        {
            PlanetType planetType = GetPlanetType(planet);
            if (planetType != boxType)
                return false;
        }
        
        return true;
    }
    
    public int GetPlanetCount()
    {
        return planetsInBox.Count;
    }
    
    public List<GameObject> GetPlanetsInBox()
    {
        return new List<GameObject>(planetsInBox);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = boxType == PlanetType.Rocky ? Color.yellow : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
