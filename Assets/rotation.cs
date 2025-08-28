using UnityEngine;
using SG;

public class PlanetGestureRotation : MonoBehaviour
{
    public SG_BasicGesture rotationGesture;   
    public SG_TrackedHand trackedHand;        

    public Vector3 rotationAxis = new Vector3(0, 1, 0);
    public float baseSpeed = 20f;   // base rotation speed

    void Update()
    {
        if (rotationGesture != null && rotationGesture.IsGesturing && trackedHand != null)
        {
            SG_HandPose pose;
            if (trackedHand.GetHandPose(out pose))
            {
                float thumbFlex = pose.normalizedFlexion[0]; // Flexion values: [0] = thumb, [1] = index, [2] = middle, [3] = ring, [4] = pinky
                float speed = baseSpeed;

                // Increase speed based on thumb flexion thresholds
                if (thumbFlex < 0.25f) speed *= 2f;   // 2x faster
                if (thumbFlex < 0.20f) speed *= 3f;   // 3x faster
                if (thumbFlex < 0.15f) speed *= 4f;   // 4x faster

                transform.Rotate(rotationAxis * speed * Time.deltaTime, Space.Self);

                Debug.Log($"Thumb={thumbFlex:F2}, Speed={speed:F1}");
            }
        }
    }
}