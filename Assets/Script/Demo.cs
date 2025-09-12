using UnityEngine;

public class NovaWeightHaptics : MonoBehaviour
{
    [Header("References")]
    public Rigidbody targetRb;        // the object you’re holding
    public Transform wrist;           // tracked wrist/controller on the glove hand

    [Header("Grasp Detection (set these from your grab code)")]
    public bool isGrabbing = false;
    public bool pinchGrasp = false;   // true = pinch (thumb+index); false = power (thumb+index+middle)

    [Header("Physics → Haptics Mapping")]
    [Range(0.2f, 1.0f)] public float mu = 0.5f;     // contact friction guess
    [Range(1.0f, 3.0f)] public float safety = 1.7f; // safety factor
    public float FmaxThumb  = 18f;
    public float FmaxIndex  = 18f;
    public float FmaxMiddle = 18f;
    public float FmaxRing   = 18f;

    [Header("Vibe Settings")]
    public float liftPulseMs = 90f;
    public float setdownPulseMs = 70f;

    Vector3 prevWristPos;
    Vector3 prevWristVel;
    bool lastGrabbing = false;
    bool wasAirborne = false; // simple set-down detector

    void Start()
    {
        if (wrist != null) prevWristPos = wrist.position;
    }

    void Update()
    {
        // Track wrist velocity / acceleration
        Vector3 wristVel = Vector3.zero;
        if (wrist != null)
        {
            wristVel = (wrist.position - prevWristPos) / Mathf.Max(Time.deltaTime, 1e-4f);
            prevWristPos = wrist.position;
        }
        Vector3 wristAcc = (wristVel - prevWristVel) / Mathf.Max(Time.deltaTime, 1e-4f);
        prevWristVel = wristVel;

        // Lift-off / set-down vibrotactile cues
        if (isGrabbing && !lastGrabbing) OnLiftOff();
        lastGrabbing = isGrabbing;

        if (!isGrabbing)
        {
            // Not holding—zero forces each frame.
            SendToNova(0f, 0f, 0f, 0f);
            return;
        }

        if (targetRb == null) return;

        // Acceleration component along gravity (positive when moving downward faster)
        float aParallel = Vector3.Dot(wristAcc, Physics.gravity.normalized);
        aParallel = Mathf.Max(0f, aParallel); // only add when it increases load

        // Load to support (N)
        float g = Physics.gravity.magnitude;
        float load = targetRb.mass * (g + aParallel);

        // Fingers participating
        int k = pinchGrasp ? 2 : 3;

        // Required normal force per finger
        float requiredPerFinger = safety * load / Mathf.Max(0.1f, mu) / k;

        // Normalize to 0–1 levels
        float Lthumb  = Mathf.Clamp01(requiredPerFinger / Mathf.Max(1f, FmaxThumb));
        float Lindex  = Mathf.Clamp01(requiredPerFinger / Mathf.Max(1f, FmaxIndex));
        float Lmiddle = pinchGrasp ? 0f : Mathf.Clamp01(requiredPerFinger / Mathf.Max(1f, FmaxMiddle));
        float Lring   = 0f; // optional: include for very heavy power grasps

        // Send per-finger forces
        SendToNova(Lthumb, Lindex, Lmiddle, Lring);

        // Simple set-down detector: if object contact sensors say it's resting, trigger OnSetDown()
        // If you don't have sensors, you can approximate with low velocity + near-ground check.
        if (IsSetDown())
        {
            OnSetDown();
        }
    }

    // TODO: Replace this with your SenseGlove API call that accepts normalized [0..1] per-finger force levels.
    void SendToNova(float thumb, float index, float middle, float ring)
    {
        // Example placeholder:
        // glove.SetFingerForcesNormalized(thumb, index, middle, ring);
    }

    void OnLiftOff()
    {
        if (targetRb == null) return;
        float amp = Mathf.Clamp01(targetRb.mass / 3.0f); // scale 0..1 around ~3 kg
        // Call your vibrotactile API here, e.g., pulse thumb/index for liftPulseMs with 'amp'
        // glove.PulseFingertips(amp, liftPulseMs);
    }

    void OnSetDown()
    {
        if (targetRb == null) return;
        float amp = Mathf.Clamp01(targetRb.mass / 3.0f);
        // glove.PulseFingertips(amp * 0.8f, setdownPulseMs);
    }

    bool IsSetDown()
    {
        // Stub: replace with your contact logic (raycast, collision flags, or interactable state).
        return false;
    }
}