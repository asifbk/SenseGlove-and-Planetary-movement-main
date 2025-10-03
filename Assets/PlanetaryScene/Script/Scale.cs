using UnityEngine;
using SG;

public class ThumbGestureMovePanel : MonoBehaviour
{
    [Header("Gesture Settings")]
    public SG_BasicGesture thumbGesture;
    public SG_TrackedHand trackedHand;

    [Header("Movement Settings")]
    public Transform solarPanelParent;
    public float moveSpeed = 2f;

    public float minDistanceToCamera = 0.5f; // stop distance

    void Update()
    {
        if (trackedHand == null || thumbGesture == null || solarPanelParent == null)
            return;

        // Check if gesture is active
        if (thumbGesture.IsGesturing)
        {
            SG_HandPose pose;
            if (trackedHand.GetHandPose(out pose))
            {
                float thumbFlex = pose.normalizedFlexion[0];

                // Only move if thumb is flexed enough
                if (thumbFlex >= 0.35f)
                {
                    Vector3 direction = (Camera.main.transform.position - solarPanelParent.position).normalized;
                    direction.y = 0; // horizontal movement only

                    float distance = Vector3.Distance(Camera.main.transform.position, solarPanelParent.position);

                    if (distance > minDistanceToCamera)
                    {
                        solarPanelParent.position += direction * moveSpeed * Time.deltaTime;
                    }
                }
            }
        }
    }
}
