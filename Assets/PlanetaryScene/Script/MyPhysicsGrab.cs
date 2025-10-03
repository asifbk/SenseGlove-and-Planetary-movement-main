using UnityEngine;
using SG;

public class MyPhysicsGrab : MonoBehaviour
{
    private SG_PhysicsGrab grabScript;
    [Header("References")]
    public SG_PhysicsGrab grabScriptOverride; // optional: drag your SG_PhysicsGrab here if not on same GameObject
    public bool IsGrabbing { get; private set; }
    [Header("Debug")]
    public bool debugShowForces = false; // show force feedback while grabbing

    // cached reference to possible feedback layer on the hand
    private SG.SG_HandFeedback handFeedback;
    private SG.SG_FingerFeedback[] fingerFeedbackChildren = new SG.SG_FingerFeedback[0];
    private float consoleLogInterval = 0.2f;
    private float consoleLogTimer = 0f;
    private float statusLogInterval = 1.0f;
    private float statusLogTimer = 0f;

    void Awake()
    {
        grabScript = grabScriptOverride != null ? grabScriptOverride : GetComponent<SG_PhysicsGrab>();
        // try to find a feedback layer on the same GameObject or parents
        handFeedback = GetComponentInParent<SG.SG_HandFeedback>();
        if (handFeedback == null)
        {
            fingerFeedbackChildren = GetComponentsInChildren<SG.SG_FingerFeedback>();
        }
        if (debugShowForces)
        {
            Debug.LogFormat(this, "MyPhysicsGrab Awake: grabScript override set? {0}, grabScript found? {1}, handFeedback found? {2}",
                grabScriptOverride != null, grabScript != null, handFeedback != null);
        }
    }

    void Update()
    {
        if (grabScript == null)
        {
            // Try to recover by checking override or parent
            grabScript = grabScriptOverride != null ? grabScriptOverride : GetComponentInParent<SG_PhysicsGrab>();
            if (grabScript == null)
            {
                IsGrabbing = false;
                if (debugShowForces) Debug.LogWarning("MyPhysicsGrab: No SG_PhysicsGrab found on this object or parents. Attach SG_PhysicsGrab or set grabScriptOverride.", this.gameObject);
                return;
            }
        }

        // Prefer public property if available
        bool newIsGrabbing = false;
        try
        {
            newIsGrabbing = grabScript != null && grabScript.IsGrabbing; // SG_GrabScript provides this property
        }
        catch
        {
            newIsGrabbing = false;
        }

        // If public property didn't report grabbing, fall back to previous reflection/child checks
        if (!newIsGrabbing)
        {
            var grabbedField = grabScript.GetType().GetField("grabbedObject",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (grabbedField != null)
            {
                object val = grabbedField.GetValue(grabScript);
                newIsGrabbing = (val != null);
            }
            else
            {
                // Fallback: check if a Grabable is attached under this hand
                SG_Grabable grab = grabScript.GetComponentInChildren<SG_Grabable>();
                newIsGrabbing = grab != null;
            }
        }

        if (newIsGrabbing && !IsGrabbing)
        {
            if (debugShowForces) Debug.Log("MyPhysicsGrab: Grab started", this.gameObject);
        }
        else if (!newIsGrabbing && IsGrabbing)
        {
            if (debugShowForces) Debug.Log("MyPhysicsGrab: Grab ended", this.gameObject);
        }
        IsGrabbing = newIsGrabbing;
        {
            if (debugShowForces)
            {
                // indicate that the private field lookup failed for this SG_PhysicsGrab version
                statusLogTimer -= Time.deltaTime;
                if (statusLogTimer <= 0f)
                {
                    statusLogTimer = statusLogInterval;
                    Debug.LogWarning("MyPhysicsGrab: Could not find 'grabbedObject' private field on SG_PhysicsGrab. Falling back to child SG_Grabable check.", this.gameObject);
                }
            }

            // Fallback: check if a Grabable is attached under this hand
            SG_Grabable grab = grabScript.GetComponentInChildren<SG_Grabable>();
            IsGrabbing = grab != null;
        }
        // keep handFeedback up to date if present
        if (handFeedback == null)
        {
            handFeedback = GetComponentInParent<SG.SG_HandFeedback>();
            if (handFeedback == null)
            {
                // try to refresh child list
                fingerFeedbackChildren = GetComponentsInChildren<SG.SG_FingerFeedback>();
                if (fingerFeedbackChildren.Length == 0 && debugShowForces && IsGrabbing)
                {
                    Debug.LogWarning("MyPhysicsGrab: No SG_HandFeedback or SG_FingerFeedback children found. Cannot log per-finger forces.", this.gameObject);
                }
            }
        }
    }

    // No GUI required — console-only logging

    void LateUpdate()
    {
        // throttled console logging
        if (!debugShowForces) return;

        // status logging to help debug missing output
        statusLogTimer -= Time.deltaTime;
        if (statusLogTimer <= 0f)
        {
            statusLogTimer = statusLogInterval;
            Debug.LogFormat(this, "MyPhysicsGrab Status: IsGrabbing={0}, grabScript={1}, handFeedback={2}", IsGrabbing, grabScript != null, handFeedback != null);
        }

        if (!IsGrabbing) return;
        // Build finger data either from handFeedback wrapper or directly from child feedback scripts
        consoleLogTimer -= Time.deltaTime;
        if (consoleLogTimer > 0f) return;
        consoleLogTimer = consoleLogInterval;

        float t = Time.time;
        // Print one line per finger for readability
        if (handFeedback != null)
        {
            for (int i = 0; i < handFeedback.fingerFeedbackScripts.Length; i++)
            {
                SG.SG_FingerFeedback f = handFeedback.GetFeedbackScript(i);
                if (f != null)
                {
                    var mat = f.TouchedMaterialScript;
                    float maxForce = mat != null ? mat.MaxForce : 1f;
                    float maxDist = mat != null ? mat.MaxForceDistance : 0f;
                    float normalized = f.ForceLevel / 100f;
                    Debug.LogFormat(this, "{0:F2}s - Finger {1}: {2}m / {3}% -> {4} (mat maxForce={5}, maxDist={6})",
                        t, i, System.Math.Round(f.DistanceInCollider, 4), f.ForceLevel, System.Math.Round(normalized, 3), maxForce, maxDist);
                }
                else
                {
                    Debug.LogFormat(this, "{0:F2}s - Finger {1}: - (no data)", t, i);
                }
            }
        }
        else if (fingerFeedbackChildren != null && fingerFeedbackChildren.Length > 0)
        {
            for (int i = 0; i < fingerFeedbackChildren.Length; i++)
            {
                var f = fingerFeedbackChildren[i];
                var mat = f.TouchedMaterialScript;
                float maxForce = mat != null ? mat.MaxForce : 1f;
                float maxDist = mat != null ? mat.MaxForceDistance : 0f;
                float normalized = f.ForceLevel / 100f;
                Debug.LogFormat(this, "{0:F2}s - Finger {1}: {2}m / {3}% -> {4} (mat maxForce={5}, maxDist={6})",
                    t, i, System.Math.Round(f.DistanceInCollider, 4), f.ForceLevel, System.Math.Round(normalized, 3), maxForce, maxDist);
            }
        }
        else
        {
            Debug.LogFormat(this, "{0:F2}s - <no finger data>", t);
        }
    }
}
