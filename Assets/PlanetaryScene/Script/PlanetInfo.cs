using UnityEngine;
using TMPro;
using SG;
using SGCore;
using SGCore.Nova;

[RequireComponent(typeof(SG_Grabable))]
public class FloatingObjectInfo : MonoBehaviour
{
    [Header("UI Prefab")]
    public GameObject infoCanvasPrefab; // Assign the prefab
    public Vector3 offset = new Vector3(0f, 0.2f, 0f); // Floating above object

    [Header("Planet Info (Editable)")]
    public float mass = 2.5f;               // kg
    public string diameter = "Medium";          // size description
    public float distanceFromSun = 1.0f;    // AU or any unit
    public float rotationSpeed = 10f;       // degrees per second

    private GameObject infoCanvasInstance;
    private TextMeshProUGUI infoText;
    private SG_Grabable grabable;

    void Start()
    {
        grabable = GetComponent<SG_Grabable>();

        // Instantiate canvas prefab
        if (infoCanvasPrefab != null)
        {
            infoCanvasInstance = Instantiate(infoCanvasPrefab);
            infoText = infoCanvasInstance.GetComponentInChildren<TextMeshProUGUI>();
            infoCanvasInstance.SetActive(false); // hide initially
        }
    }

    void Update()
    {
    if (grabable == null) return;
    if (infoCanvasInstance == null || infoText == null)
    {
        Debug.LogWarning("Info canvas or text not assigned properly!");
        return;
    }

    if (grabable.IsGrabbed())
    {
        infoCanvasInstance.SetActive(true);
        infoText.text =
            $"Name: {grabable.name}\n" +
            $"Mass: {mass} kg\n" +
            $"Diameter: {diameter}\n" +
            $"Distance from Sun: {distanceFromSun} AU\n" +
            $"Rotation Speed: {rotationSpeed}°/s";

        infoCanvasInstance.transform.position = transform.position + offset;

        if (Camera.main != null)
            infoCanvasInstance.transform.rotation = Camera.main.transform.rotation;
    }
    else
    {
        infoCanvasInstance.SetActive(false);
    }
     }
}
