using UnityEngine;

public class PlanetPlaceholder : MonoBehaviour
{
    [Header("Placeholder Settings")]
    [Tooltip("Expected mass value for this position (based on correct planet)")]
    public float expectedMass = 0f;
    
    [Tooltip("Position rank (1 = heaviest, 8 = lightest)")]
    public int rank = 1;
    
    [Header("Visual Feedback")]
    public MeshRenderer platformRenderer;
    public Material defaultMaterial;
    public Material correctMaterial;
    
    [Header("State")]
    public bool isOccupied = false;
    public GameObject placedPlanet = null;
    
    private PlacementValidator validator;
    
    void Start()
    {
        validator = FindObjectOfType<PlacementValidator>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Untagged") && !isOccupied)
        {
            GrabVibration grabVibration = other.GetComponent<GrabVibration>();
            if (grabVibration != null)
            {
                placedPlanet = other.gameObject;
                isOccupied = true;
                
                if (validator != null)
                {
                    validator.OnPlanetPlaced(this, placedPlanet);
                }
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == placedPlanet)
        {
            isOccupied = false;
            placedPlanet = null;
            
            if (platformRenderer != null && defaultMaterial != null)
            {
                platformRenderer.material = defaultMaterial;
            }
            
            if (validator != null)
            {
                validator.OnPlanetRemoved(this);
            }
        }
    }
    
    public void SetCorrectVisual(bool isCorrect)
    {
        if (platformRenderer != null)
        {
            platformRenderer.material = isCorrect ? correctMaterial : defaultMaterial;
        }
    }
    
    public void ClearPlacement()
    {
        isOccupied = false;
        placedPlanet = null;
        SetCorrectVisual(false);
    }
}
