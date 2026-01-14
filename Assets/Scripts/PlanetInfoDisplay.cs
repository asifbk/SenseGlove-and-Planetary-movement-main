using UnityEngine;
using TMPro;
using SG;

public class PlanetInfoDisplay : MonoBehaviour
{
    [System.Serializable]
    public class PlanetData
    {
        public string planetName;
        public float mass;
        public float diameter;
        public float orbitalPeriod;
        public float distanceFromSun;
    }
    
    [Header("Planet Data")]
    public PlanetData data;
    
    [Header("Info Panel")]
    [Tooltip("Prefab for the info canvas")]
    public GameObject infoCanvasPrefab;
    
    [Tooltip("Offset from planet center")]
    public Vector3 canvasOffset = new Vector3(0, 1.5f, 0);
    
    [Tooltip("Show info on hover")]
    public bool showOnHover = true;
    
    [Tooltip("Show info when grabbed")]
    public bool showWhenGrabbed = true;
    
    private GameObject infoCanvasInstance;
    private TextMeshProUGUI infoText;
    private SG_Grabable grabable;
    private bool isHovering;
    private Transform cameraTransform;
    
    void Start()
    {
        grabable = GetComponent<SG_Grabable>();
        cameraTransform = Camera.main != null ? Camera.main.transform : null;
        
        if (data.planetName == null || data.planetName == "")
        {
            data.planetName = gameObject.name;
        }
        
        if (infoCanvasPrefab != null)
        {
            CreateInfoCanvas();
        }
        
        if (infoCanvasInstance != null)
        {
            infoCanvasInstance.SetActive(false);
        }
    }
    
    void Update()
    {
        bool shouldShow = false;
        
        if (grabable != null && grabable.IsGrabbed() && showWhenGrabbed)
        {
            shouldShow = true;
        }
        else if (isHovering && showOnHover)
        {
            shouldShow = true;
        }
        
        if (infoCanvasInstance != null)
        {
            infoCanvasInstance.SetActive(shouldShow);
            
            if (shouldShow)
            {
                UpdateCanvasPosition();
            }
        }
    }
    
    private void CreateInfoCanvas()
    {
        infoCanvasInstance = Instantiate(infoCanvasPrefab, transform);
        infoCanvasInstance.transform.localPosition = canvasOffset;
        
        infoText = infoCanvasInstance.GetComponentInChildren<TextMeshProUGUI>();
        
        if (infoText != null)
        {
            UpdateInfoText();
        }
    }
    
    private void UpdateInfoText()
    {
        if (infoText == null) return;
        
        string info = $"<b>{data.planetName}</b>\n\n";
        info += $"Mass: {FormatMass(data.mass)}\n";
        info += $"Diameter: {FormatDistance(data.diameter)} km\n";
        info += $"Orbital Period: {FormatOrbitalPeriod(data.orbitalPeriod)}\n";
        info += $"Distance from Sun: {FormatDistance(data.distanceFromSun)} km";
        
        infoText.text = info;
    }
    
    private void UpdateCanvasPosition()
    {
        if (infoCanvasInstance == null) return;
        
        infoCanvasInstance.transform.localPosition = canvasOffset;
        
        if (cameraTransform != null)
        {
            infoCanvasInstance.transform.LookAt(cameraTransform);
            infoCanvasInstance.transform.Rotate(0, 180, 0);
        }
    }
    
    private string FormatMass(float mass)
    {
        if (mass >= 1e24f)
        {
            return $"{(mass / 1e24f):F2} × 10²⁴ kg";
        }
        else if (mass >= 1e21f)
        {
            return $"{(mass / 1e21f):F2} × 10²¹ kg";
        }
        else
        {
            return $"{mass:F2} kg";
        }
    }
    
    private string FormatDistance(float distance)
    {
        if (distance >= 1e9f)
        {
            return $"{(distance / 1e9f):F2} billion";
        }
        else if (distance >= 1e6f)
        {
            return $"{(distance / 1e6f):F2} million";
        }
        else if (distance >= 1e3f)
        {
            return $"{(distance / 1e3f):F2} thousand";
        }
        else
        {
            return $"{distance:F2}";
        }
    }
    
    private string FormatOrbitalPeriod(float days)
    {
        if (days >= 365f)
        {
            float years = days / 365f;
            return $"{years:F2} years";
        }
        else
        {
            return $"{days:F2} days";
        }
    }
    
    void OnMouseEnter()
    {
        isHovering = true;
    }
    
    void OnMouseExit()
    {
        isHovering = false;
    }
}
