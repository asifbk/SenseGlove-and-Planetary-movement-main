using UnityEngine;
using SG;

public class PlanetGestureRotationAndScale : MonoBehaviour
{
    [Header("Rotation Settings")]
    public SG_BasicGesture rotationGesture;    
    public SG_TrackedHand trackedHand;         
    public float gestureYSpeed = 50f;          

    [Header("Scaling Settings")]
    public SG_BasicGesture pinchGesture;       
    public float scaleSpeed = 0.5f;
    public float minScale = 0.2f;
    public float maxScale = 3f;

    [Header("Grab Reference")]
    public MyPhysicsGrab grabWrapper;   // ✅ reference to our wrapper

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

    void HandleGestureYRotation(SG_HandPose pose)
    {
        if (rotationGesture != null && rotationGesture.IsGesturing)
        {
            float thumbFlex = pose.normalizedFlexion[0];
            float speed = gestureYSpeed;

            if (thumbFlex < 0.25f) speed *= 1f;
            if (thumbFlex < 0.20f) speed *= 2f;
            if (thumbFlex < 0.15f) speed *= 4f;

            transform.Rotate(Vector3.up * speed * Time.deltaTime, Space.Self);
        }
    }

    void HandlePinchScaling(SG_HandPose pose)
    {
        bool isHolding = grabWrapper != null && grabWrapper.IsGrabbing;

        if (pinchGesture != null && pinchGesture.IsGesturing && !isHolding)
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
        else if (!isHolding) // ✅ only auto-shrink when not grabbing
        {
            Vector3 targetScale = Vector3.one * minScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }
    }
}
