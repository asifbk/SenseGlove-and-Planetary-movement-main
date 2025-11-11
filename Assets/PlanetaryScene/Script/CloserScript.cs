using UnityEngine;
using SG;
using SGCore;
using SGCore.Nova;

public enum MoveAxis { X, Y, Z }

/// <summary>
/// Moves an object forward along a chosen axis while all non-thumb fingers
/// flex above the threshold. Stops instantly when flexion drops below threshold.
/// The object never returns to its original position and continues from where it stopped.
/// </summary>
public class FingerFlexion_MoveSimple : MonoBehaviour
{
    [Header("SenseGlove References")]
    public SG_TrackedHand trackedHand;   // Assign in Inspector (Left/Right Hand)
    public Transform targetObject;       // The SolarSystem or any parent transform

    [Header("Motion Settings")]
    public MoveAxis moveAxis = MoveAxis.Z;  // Axis selection
    public float moveSpeed = 0.5f;          // Movement speed (m/s)
    public float maxDistance = -2f;         // Maximum distance to move (negative = opposite direction)
    [Range(0f, 1f)]
    public float flexThreshold = 0.8f;      // Flexion threshold

    [Header("Debug")]
    public bool showDebug = false;

    private Vector3 initialPosition;
    private float distanceTraveled = 0f;

    void Start()
    {
        if (targetObject == null)
            targetObject = this.transform;
        
        initialPosition = targetObject.localPosition;
    }

    void Update()
    {
        if (trackedHand == null) return;

        SG_HandPose pose;
        if (!trackedHand.GetHandPose(out pose)) return;

        // Get normalized flexion values (0–1)
        float index = pose.normalizedFlexion[1];
        float middle = pose.normalizedFlexion[2];
        float ring = pose.normalizedFlexion[3];
        float pinky = pose.normalizedFlexion[4];

        bool allFlexed = index >= flexThreshold &&
                         middle >= flexThreshold &&
                         ring >= flexThreshold &&
                         pinky >= flexThreshold;

        // Determine direction vector
        Vector3 direction = Vector3.forward;
        switch (moveAxis)
        {
            case MoveAxis.X: direction = Vector3.right; break;
            case MoveAxis.Y: direction = Vector3.up; break;
            case MoveAxis.Z: direction = Vector3.forward; break;
        }

        // Apply simple, continuous translation with distance limit
        if (allFlexed)
        {
            // Check if we haven't exceeded max distance
            float absMoveAmount = moveSpeed * Time.deltaTime;
            float absMaxDistance = Mathf.Abs(maxDistance);
            
            if (distanceTraveled < absMaxDistance)
            {
                // Only move the remaining distance if we're close to the limit
                float remainingDistance = absMaxDistance - distanceTraveled;
                if (absMoveAmount > remainingDistance)
                {
                    absMoveAmount = remainingDistance;
                }
                
                targetObject.Translate(direction * absMoveAmount * Mathf.Sign(maxDistance), Space.Self);
                distanceTraveled += absMoveAmount;

                if (showDebug)
                {
                    Debug.Log($"Moving along {moveAxis} | Distance: {distanceTraveled:F2}/{absMaxDistance:F2} | " +
                              $"I={index:F2}, M={middle:F2}, R={ring:F2}, P={pinky:F2}");
                }
            }
            else if (showDebug)
            {
                Debug.Log($"Max distance reached ({distanceTraveled:F2}m)");
            }
        }
    }
}
