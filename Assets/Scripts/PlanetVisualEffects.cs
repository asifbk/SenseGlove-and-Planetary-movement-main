using UnityEngine;
using SG;

[RequireComponent(typeof(SG_Grabable))]
public class PlanetVisualEffects : MonoBehaviour
{
    [Header("Trail Settings")]
    [Tooltip("Enable velocity trail for this planet")]
    public bool enableTrail = true;
    
    [Tooltip("Trail duration in seconds")]
    public float trailTime = 2f;
    
    [Tooltip("Trail width")]
    public float trailWidth = 0.05f;
    
    [Tooltip("Trail color gradient")]
    public Gradient trailColorGradient;
    
    [Header("Grab Highlight Effect")]
    [Tooltip("Material to use when planet is grabbed")]
    public Material highlightMaterial;
    
    [Tooltip("Emission intensity multiplier")]
    public float emissionIntensity = 2f;
    
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed for texture (degrees per second)")]
    public float textureRotationSpeed = 10f;
    
    [Tooltip("Rotate around Y axis")]
    public bool rotateTexture = true;
    
    [Header("Atmospheric Glow")]
    [Tooltip("Enable atmospheric glow effect")]
    public bool hasAtmosphere = false;
    
    [Tooltip("Atmosphere color")]
    public Color atmosphereColor = new Color(0.5f, 0.7f, 1f, 0.3f);
    
    [Tooltip("Atmosphere scale multiplier")]
    public float atmosphereScale = 1.1f;
    
    private TrailRenderer trailRenderer;
    private SG_Grabable grabable;
    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    private Material runtimeMaterial;
    private GameObject atmosphereObject;
    private float textureOffset;
    
    private const string EMISSION_COLOR_PROPERTY = "_EmissionColor";
    private const string MAIN_TEX_PROPERTY = "_MainTex";
    
    void Start()
    {
        grabable = GetComponent<SG_Grabable>();
        meshRenderer = GetComponent<MeshRenderer>();
        
        if (meshRenderer != null && meshRenderer.sharedMaterial != null)
        {
            originalMaterial = meshRenderer.sharedMaterial;
            runtimeMaterial = new Material(originalMaterial);
            meshRenderer.material = runtimeMaterial;
        }
        
        SetupTrailRenderer();
        
        if (hasAtmosphere)
        {
            CreateAtmosphericGlow();
        }
        
        if (trailColorGradient == null)
        {
            trailColorGradient = CreateDefaultGradient();
        }
    }
    
    void Update()
    {
        if (grabable != null)
        {
            bool isGrabbed = grabable.IsGrabbed();
            
            if (trailRenderer != null)
            {
                trailRenderer.emitting = isGrabbed && enableTrail;
            }
            
            UpdateGrabHighlight(isGrabbed);
        }
        
        if (rotateTexture && runtimeMaterial != null)
        {
            UpdateTextureRotation();
        }
    }
    
    void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
    
    private void SetupTrailRenderer()
    {
        if (!enableTrail) return;
        
        trailRenderer = gameObject.GetComponent<TrailRenderer>();
        
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }
        
        trailRenderer.time = trailTime;
        trailRenderer.minVertexDistance = 0.01f;
        trailRenderer.widthCurve = AnimationCurve.Linear(0f, trailWidth, 1f, trailWidth * 0.2f);
        trailRenderer.colorGradient = trailColorGradient;
        trailRenderer.emitting = false;
        trailRenderer.numCornerVertices = 5;
        trailRenderer.numCapVertices = 5;
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trailRenderer.receiveShadows = false;
        
        if (originalMaterial != null)
        {
            Material trailMat = new Material(originalMaterial);
            trailMat.EnableKeyword("_EMISSION");
            trailRenderer.material = trailMat;
        }
    }
    
    private void UpdateGrabHighlight(bool isGrabbed)
    {
        if (runtimeMaterial == null) return;
        
        if (isGrabbed)
        {
            if (highlightMaterial != null)
            {
                meshRenderer.material = highlightMaterial;
            }
            else if (runtimeMaterial.HasProperty(EMISSION_COLOR_PROPERTY))
            {
                runtimeMaterial.EnableKeyword("_EMISSION");
                Color baseColor = runtimeMaterial.color;
                runtimeMaterial.SetColor(EMISSION_COLOR_PROPERTY, baseColor * emissionIntensity);
            }
        }
        else
        {
            if (highlightMaterial != null && meshRenderer.material == highlightMaterial)
            {
                meshRenderer.material = runtimeMaterial;
            }
            else if (runtimeMaterial.HasProperty(EMISSION_COLOR_PROPERTY))
            {
                runtimeMaterial.SetColor(EMISSION_COLOR_PROPERTY, Color.black);
            }
        }
    }
    
    private void UpdateTextureRotation()
    {
        textureOffset += textureRotationSpeed * Time.deltaTime / 360f;
        textureOffset = textureOffset % 1f;
        
        if (runtimeMaterial.HasProperty(MAIN_TEX_PROPERTY))
        {
            runtimeMaterial.SetTextureOffset(MAIN_TEX_PROPERTY, new Vector2(textureOffset, 0f));
        }
    }
    
    private void CreateAtmosphericGlow()
    {
        atmosphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        atmosphereObject.name = "Atmosphere";
        atmosphereObject.transform.SetParent(transform);
        atmosphereObject.transform.localPosition = Vector3.zero;
        atmosphereObject.transform.localScale = Vector3.one * atmosphereScale;
        
        Destroy(atmosphereObject.GetComponent<Collider>());
        
        MeshRenderer atmosphereRenderer = atmosphereObject.GetComponent<MeshRenderer>();
        Material atmosphereMaterial = new Material(Shader.Find("Standard"));
        atmosphereMaterial.SetFloat("_Mode", 3);
        atmosphereMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        atmosphereMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        atmosphereMaterial.SetInt("_ZWrite", 0);
        atmosphereMaterial.DisableKeyword("_ALPHATEST_ON");
        atmosphereMaterial.EnableKeyword("_ALPHABLEND_ON");
        atmosphereMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        atmosphereMaterial.renderQueue = 3000;
        atmosphereMaterial.color = atmosphereColor;
        atmosphereMaterial.EnableKeyword("_EMISSION");
        atmosphereMaterial.SetColor(EMISSION_COLOR_PROPERTY, atmosphereColor * 0.5f);
        
        atmosphereRenderer.material = atmosphereMaterial;
        atmosphereRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        atmosphereRenderer.receiveShadows = false;
    }
    
    private Gradient CreateDefaultGradient()
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        
        Color planetColor = meshRenderer != null && meshRenderer.sharedMaterial != null 
            ? meshRenderer.sharedMaterial.color 
            : Color.white;
        
        colorKeys[0] = new GradientColorKey(planetColor, 0f);
        colorKeys[1] = new GradientColorKey(planetColor * 0.5f, 1f);
        
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(0f, 1f);
        
        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }
}
