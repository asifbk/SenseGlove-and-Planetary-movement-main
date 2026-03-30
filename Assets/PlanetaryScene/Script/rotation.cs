using UnityEngine;
using SG;

public class PlanetGestureRotationAndScale : MonoBehaviour
{
    [Header("Rotation Settings")]
    public SG_BasicGesture rotationGesture; 		
    public SG_TrackedHand trackedHand; 			
    public float gestureYSpeed = 50f; 			

    [Header("Scaling Settings")]
    // NOTE: Renaming to reflect positional movement instead of scaling
    public SG_BasicGesture pinchGesture; 		
    public float moveSpeed = 0.2f; // Renamed for clarity, using the old scaleSpeed value
    public enum MovementMode { NegativeX }
    public MovementMode movementMode = MovementMode.NegativeX;

    // X-axis movement config (used when movementMode == NegativeX)
    public float minXPosition = -20f; // minimum X boundary (localPosition.x)
    public float maxXPosition = 2f;  // maximum X boundary (localPosition.x)

    // Camera-based movement config (used when movementMode == TowardCamera)
    public Transform cameraRig; 
    public float minDistance = 0.5f; // minimum distance from camera
    public float maxDistance = 5f;   // maximum distance from camera
    public float smoothSpeed = 8f;   // smoothing for the motion
    
    [Header("Grab Reference")]
    public MyPhysicsGrab grabWrapper; 		
    

    void Update()
    {
        if (trackedHand != null)
        {
            SG_HandPose pose;
            if (trackedHand.GetHandPose(out pose))
            {
                HandleGestureYRotation(pose);
                HandlePinchMovement(pose); // Renamed the method call
            }
        }
    }

    void HandleGestureYRotation(SG_HandPose pose)
    {
        if (rotationGesture != null && rotationGesture.IsGesturing)
        {
            float thumbFlex = pose.normalizedFlexion[0];
            float speed = gestureYSpeed;

            // Apply rotation speed multipliers based on how tight the rotation gesture is
            if (thumbFlex < 0.25f) speed *= 1f;
            if (thumbFlex < 0.20f) speed *= 2f;
            if (thumbFlex < 0.15f) speed *= 4f;
            
            transform.Rotate(Vector3.up * speed * Time.deltaTime, Space.Self);
        }
    }

    // Renamed from HandlePinchScaling for clarity, but keeps the same structure
    void HandlePinchMovement(SG_HandPose pose) 
    {
        // Using the old scaleSpeed variable as moveSpeed now
        float speed = moveSpeed; 
        
        // Use a safe check for grabbing status
        bool isHolding = grabWrapper != null && grabWrapper.IsGrabbing;

        if (pinchGesture != null && pinchGesture.IsGesturing && !isHolding)
        {
            float thumbFlex = pose.normalizedFlexion[0];

            // --- Negative X axis mode (simple local X translation) ---
            if (movementMode == MovementMode.NegativeX)
            {
                float positionChange = 0f;

                // Map pinch to X-axis: pinch closed -> move positive X, pinch open -> move negative X
                if (thumbFlex < 0.3f) positionChange = speed * Time.deltaTime; // move toward positive X
                else if (thumbFlex > 0.7f) positionChange = -speed * Time.deltaTime; // move toward negative X

                if (positionChange != 0f)
                {
                    Vector3 newPosition = transform.localPosition; // local position for X boundaries
                    newPosition.x += positionChange;
                    newPosition.x = Mathf.Clamp(newPosition.x, minXPosition, maxXPosition);
                    transform.localPosition = newPosition;
                }
            }
        }
    }
}