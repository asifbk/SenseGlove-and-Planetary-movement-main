using UnityEngine;
using SG;

public class PlanetGestureRotationAndScale : MonoBehaviour
{
    [Header("Rotation Settings")]
    public SG_BasicGesture rotationGesture;    // Assign ThumbsUpGesture here
    public SG_TrackedHand trackedHand;         // Hand reference
    public Vector3 rotationAxis = new Vector3(0, 1, 0);
    public float baseSpeed = 20f;

    [Header("Scaling Settings")]
    public SG_BasicGesture pinchGesture;       // Assign PinchGesture (Gun pose) here
    public float scaleSpeed = 0.5f;
    public float minScale = 0.2f;
    public float maxScale = 3f;

    void Update()
    {
        if (trackedHand != null)
        {
            SG_HandPose pose;
            if (trackedHand.GetHandPose(out pose))
            {
                HandleRotation(pose);
                HandlePinchScaling(pose);
            }
        }
    }

    void HandleRotation(SG_HandPose pose)
    {
        if (rotationGesture != null && rotationGesture.IsGesturing)
        {
            float thumbFlex = pose.normalizedFlexion[0]; // Thumb flexion
            float speed = baseSpeed;

            // Speed thresholds
            if (thumbFlex < 0.25f) speed *= 1f;
            if (thumbFlex < 0.20f) speed *= 2f;
            if (thumbFlex < 0.15f) speed *= 4f;

            transform.Rotate(rotationAxis * speed * Time.deltaTime, Space.Self);

            Debug.Log($"[Rotation] Thumb={thumbFlex:F2}, Speed={speed:F1}");
        }
    }

    void HandlePinchScaling(SG_HandPose pose)
    {
        if (pinchGesture != null && pinchGesture.IsGesturing)
        {
            float thumbFlex = pose.normalizedFlexion[0]; // Thumb flexion indicator
            float scaleChange = 0f;

            
            if (thumbFlex < 0.3f) //  zoom in the scene
            {
                scaleChange = scaleSpeed * Time.deltaTime;
            }
            
            else if (thumbFlex > 0.7f)  // zoom out the scene
            {
                scaleChange = -scaleSpeed * Time.deltaTime;
            }

            if (scaleChange != 0f)
            {
                Vector3 newScale = transform.localScale + Vector3.one * scaleChange;
                newScale = Vector3.Max(newScale, Vector3.one * minScale);
                newScale = Vector3.Min(newScale, Vector3.one * maxScale);

                transform.localScale = newScale;

                Debug.Log($"[Pinch Scaling] Thumb={thumbFlex:F2}, Scale={transform.localScale}");
            }
        }
    }
}
