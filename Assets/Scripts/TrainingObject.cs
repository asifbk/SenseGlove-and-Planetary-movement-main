using UnityEngine;

public class TrainingObject : MonoBehaviour
{
    [Header("Training Object Settings")]
    [Tooltip("Mass of this reference object")]
    public float mass = 100f;
    
    [Tooltip("Label for this object (e.g., 'Light', 'Medium', 'Heavy')")]
    public string label = "Reference";
    
    [Header("Initial Position")]
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        GrabVibration grabVibration = GetComponent<GrabVibration>();
        if (grabVibration != null)
        {
            grabVibration.mass = mass;
        }
    }
    
    void Update()
    {
        if (Vector3.Distance(transform.position, initialPosition) > 5f)
        {
            ResetPosition();
        }
    }
    
    public void ResetPosition()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
