using UnityEngine;
using SG;

[RequireComponent(typeof(SG_Grabable))]
public class PlanetSnapToHand : MonoBehaviour
{
    [Header("SNAP POINT (inside HAND)")]
    public Transform snapPoint;

    [Header("HAND ROOT (SG_TrackedHand root)")]
    public Transform handTransform;

    [Header("Snapping Settings")]
    public float snapDistance = 0.12f;
    public float snapSpeed = 12f;
    public bool smoothSnap = true;

    private SG_Grabable grabable;
    private bool isGrabbed = false;

    void Start()
    {
        grabable = GetComponent<SG_Grabable>();

        Debug.Log($"[SnapDebug] Planet '{name}' START. Waiting for grab events...");

        grabable.ObjectGrabbed.AddListener((interactable, grabber) =>
        {
            Debug.Log($"[SnapDebug] '{name}' GRABBED by: {grabber.name}");
            OnGrab();
        });

        grabable.ObjectReleased.AddListener((interactable, grabber) =>
        {
            Debug.Log($"[SnapDebug] '{name}' RELEASED.");
            OnRelease();
        });
    }

    void Update()
    {
        if (snapPoint == null)
        {
            Debug.LogWarning($"[SnapDebug] '{name}' ❌ snapPoint is NULL!");
            return;
        }

        if (handTransform == null)
        {
            Debug.LogWarning($"[SnapDebug] '{name}' ❌ handTransform is NULL!");
            return;
        }

        if (!isGrabbed)
        {
            // Debug.Log($"[SnapDebug] '{name}' not grabbed yet.");
            return;
        }

        // Distance check
        float dist = Vector3.Distance(transform.position, handTransform.position);
        Debug.Log($"[SnapDebug] '{name}' distance to hand = {dist:F4} (snapDistance={snapDistance})");

        // If inside snap distance → SNAP
        if (dist <= snapDistance)
        {
            Debug.Log($"[SnapDebug] '{name}' → Inside SNAP RANGE!");

            if (smoothSnap)
            {
                Debug.Log($"[SnapDebug] '{name}' Smooth snapping... (speed={snapSpeed})");
                
                transform.position = Vector3.Lerp(
                    transform.position, 
                    snapPoint.position, 
                    snapSpeed * Time.deltaTime
                );

                transform.rotation = Quaternion.Lerp(
                    transform.rotation, 
                    snapPoint.rotation, 
                    snapSpeed * Time.deltaTime
                );
            }
            else
            {
                Debug.Log($"[SnapDebug] '{name}' Instant snap → Teleport to snapPoint!");
                transform.position = snapPoint.position;
                transform.rotation = snapPoint.rotation;
            }
        }
        else
        {
            Debug.Log($"[SnapDebug] '{name}' → Too far to snap.");
        }
    }

    // -------------------------
    // EVENTS
    // -------------------------

    void OnGrab()
    {
        Debug.Log($"[SnapDebug] '{name}' → OnGrab() fired.");
        isGrabbed = true;
    }

    void OnRelease()
    {
        Debug.Log($"[SnapDebug] '{name}' → OnRelease() fired.");
        isGrabbed = false;
    }
}
