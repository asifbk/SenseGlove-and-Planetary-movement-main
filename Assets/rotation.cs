using UnityEngine;
using SG;

public class PlanetGestureRotationAndScale : MonoBehaviour
{
    [Header("Rotation Settings")]
    public SG_BasicGesture rotationGesture;    // Gesture for Y-axis rotation
    public SG_TrackedHand trackedHand;         // Hand reference
    public float gestureYSpeed = 50f;          // Y-axis rotation speed via gesture

    [Header("Scaling Settings")]
    public SG_BasicGesture pinchGesture;       // Pinch gesture
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
                HandleGestureYRotation(pose);
                HandlePinchScaling(pose);
            }
        }
    }

    // Y-axis rotation via gesture
    void HandleGestureYRotation(SG_HandPose pose)
    {
        if (rotationGesture != null && rotationGesture.IsGesturing)
        {
            float thumbFlex = pose.normalizedFlexion[0]; // Thumb flexion
            float speed = gestureYSpeed;

            if (thumbFlex < 0.25f) speed *= 1f;
            if (thumbFlex < 0.20f) speed *= 2f;
            if (thumbFlex < 0.15f) speed *= 4f;

            transform.Rotate(Vector3.up * speed * Time.deltaTime, Space.Self);
        }
    }

    // Pinch scaling with smooth shrink back
    void HandlePinchScaling(SG_HandPose pose)
    {
        if (pinchGesture != null && pinchGesture.IsGesturing)
        {
            float thumbFlex = pose.normalizedFlexion[0];
            float scaleChange = 0f;

            if (thumbFlex < 0.3f) scaleChange = scaleSpeed * Time.deltaTime;
            else if (thumbFlex > 0.7f) scaleChange = -scaleSpeed * Time.deltaTime;

            if (scaleChange != 0f)
            {
                Vector3 newScale = transform.localScale + Vector3.one * scaleChange;
                newScale = Vector3.Max(newScale, Vector3.one * minScale);
                newScale = Vector3.Min(newScale, Vector3.one * maxScale);
                transform.localScale = newScale;
            }
        }
        else
        {
            Vector3 targetScale = Vector3.one * minScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }
    }

}
