using UnityEngine;
using SG;

public class PlanetSpinByHand : MonoBehaviour
{
    [Header("SenseGlove / Hand References")]
    public SG_TrackedHand rightHand;    // Assign your Right SG_TrackedHand in Inspector
    public float slapForceMultiplier = 5f; // Controls spin intensity
    public float spinDamping = 0.98f;      // Friction (1 = no slowdown, <1 = gradual stop)

    private Rigidbody planetRb;
    private Vector3 lastHandPos;
    private bool isTouching = false;

    void Start()
    {
        planetRb = GetComponent<Rigidbody>();
        if (planetRb == null)
        {
            Debug.LogError("PlanetSpinByHand requires a Rigidbody on the planet!");
        }
        if (rightHand != null)
            lastHandPos = rightHand.transform.position;
    }

    void Update()
    {
        if (rightHand == null || planetRb == null) return;

        Vector3 currentPos = rightHand.transform.position;
        Vector3 handVelocity = (currentPos - lastHandPos) / Time.deltaTime;

        // Check if touching planet
        if (isTouching)
        {
            float velocityMagnitude = handVelocity.magnitude;

            // Detect a "slap" → quick burst of motion
            if (velocityMagnitude > 0.4f) // threshold, tune this
            {
                // Compute rotation axis (perpendicular to hand motion)
                Vector3 spinAxis = Vector3.Cross(handVelocity.normalized, rightHand.transform.up);

                // Apply torque based on direction and force
                planetRb.AddTorque(spinAxis * velocityMagnitude * slapForceMultiplier, ForceMode.Impulse);

                Debug.Log($"Slap detected! Force: {velocityMagnitude:F2}, Axis: {spinAxis}");
            }
        }

        // Simulate friction so planet gradually slows down
        planetRb.angularVelocity *= spinDamping;

        lastHandPos = currentPos;
    }

    // Detect when hand enters or leaves planet collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("RightHand") || other.name.Contains("Right"))
        {
            isTouching = true;
            Debug.Log("Right hand touching planet");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("RightHand") || other.name.Contains("Right"))
        {
            isTouching = false;
            Debug.Log("Right hand left planet");
        }
    }
}
