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
    public float feedbackDisplayTime = 2f;
    
    private MeshRenderer boxRenderer;
    private List<GameObject> planetsInBox = new List<GameObject>();
    private Material boxMaterial;
    private bool detectionEnabled = true;
    private float detectionResumeTime = 0f;
    private Color boxOriginalColor;
    private UnityEngine.Coroutine colorResetCoroutine;
    private int lastPlanetCount = 0;
    
    void Start()
    {
        boxRenderer = GetComponent<MeshRenderer>();
        if (boxRenderer != null)
        {
            boxMaterial = boxRenderer.material;
            boxOriginalColor = boxMaterial.color;
            ResetBoxColor();
        }
    }
    
    public void ResetBoxColor()
    {
        planetsInBox.Clear();
        if (boxMaterial != null)
        {
            boxMaterial.color = boxOriginalColor;
        }
        
        detectionEnabled = false;
        detectionResumeTime = Time.time + resetDelayTime;
        lastPlanetCount = 0;
        
        if (colorResetCoroutine != null)
        {
            StopCoroutine(colorResetCoroutine);
            colorResetCoroutine = null;
        }
        
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
                boxMaterial.color = boxOriginalColor;
            lastPlanetCount = 0;
            return;
        }
        
        if (planetsInBox.Count != lastPlanetCount)
        {
            lastPlanetCount = planetsInBox.Count;
            
            if (colorResetCoroutine != null)
            {
                StopCoroutine(colorResetCoroutine);
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
            
            colorResetCoroutine = StartCoroutine(ResetColorAfterDelay());
        }
    }
    
    System.Collections.IEnumerator ResetColorAfterDelay()
    {
        yield return new UnityEngine.WaitForSeconds(feedbackDisplayTime);
        if (boxMaterial != null)
        {
            boxMaterial.color = boxOriginalColor;
        }
        Debug.Log($"[PlanetClassificationBox] {boxType} box returned to original color after feedback display");
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
