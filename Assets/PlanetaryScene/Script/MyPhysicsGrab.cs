using UnityEngine;
using SG;

public class MyPhysicsGrab : MonoBehaviour
{
    private SG_PhysicsGrab grabScript;
    public bool IsGrabbing { get; private set; }

    void Awake()
    {
        grabScript = GetComponent<SG_PhysicsGrab>();
    }

    void Update()
    {
        if (grabScript == null)
        {
            IsGrabbing = false;
            return;
        }

        // Try to detect if an object is grabbed (common private field name in SDK)
        var grabbedField = grabScript.GetType().GetField("grabbedObject",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (grabbedField != null)
        {
            object val = grabbedField.GetValue(grabScript);
            IsGrabbing = (val != null);
        }
        else
        {
            // Fallback: check if a Grabable is attached under this hand
            SG_Grabable grab = grabScript.GetComponentInChildren<SG_Grabable>();
            IsGrabbing = grab != null;
        }
    }
}
