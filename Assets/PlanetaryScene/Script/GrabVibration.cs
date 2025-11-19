using UnityEngine;
using TMPro;
using SG;
using SGCore;
using SGCore.Nova;

public enum GaugeAxis { X, Y, Z }

[RequireComponent(typeof(SG_Grabable))]
public class GrabVibration : MonoBehaviour
{
    [Header("Haptics Scaling (Paper: O(t) = α M A(t) sin(2π t OF(t)) )")]
    [Tooltip("α in the paper – overall gain on the force term.")]
    public float alpha = 1.0f;

    [Tooltip("Dummy mass M used in the model (kg).")]
    public float mass = 1.0f;

    [Header("Waveform Amplitudes per Channel (0–1)")]
    [Range(0f, 1f)] public float thumbAmplitude = 0.3f;
    [Range(0f, 1f)] public float indexAmplitude = 0.6f;
    [Range(0f, 1f)] public float wristAmplitude = 0.8f;

    [Header("Base Waveforms (Create via Assets > Create > SenseGlove > Custom Waveform)")]
    public SG_CustomWaveform thumbWaveform;
    public SG_CustomWaveform indexWaveform;
    public SG_CustomWaveform wristWaveform;

    [Header("Which Hand Does This Object Belong To?")]
    public SG.HandSide connectsTo = SG.HandSide.LeftHand;

    [Header("Optional Graph (3-channel)")]
    public UIBarGraphWithLabels graph;  // keep same name as before

    [Header("Gauge UI")]
    public TextMeshProUGUI forceText;   // e.g., "Force: 3.2 N"
    public TextMeshProUGUI newtonText;  // raw numeric value if you used one
    public Transform pointer;           // pointer needle transform

    [Header("Gauge Settings")]
    public float maxForce = 10f;        // maps to maxRotation
    public float minRotation = 0f;      // degrees
    public float maxRotation = 180f;    // degrees
    public GaugeAxis rotationAxis = GaugeAxis.Z;

    [Header("Debug Options")]
    public bool logToConsole = true;

    // --- internal state ---
    private SG_Grabable grabable;

    private Vector3 lastPosition;
    private Vector3 lastVelocity;

    private bool wasGrabbed = false;
    private float dynamicTime = 0f;     // used for time-scaling to simulate frequency

    private Quaternion pointerBaseRotation;

    void Start()
    {
        grabable = GetComponent<SG_Grabable>();
        lastPosition = transform.position;
        lastVelocity = Vector3.zero;

        if (pointer != null)
        {
            pointerBaseRotation = pointer.localRotation;
        }
    }

    void Update()
    {
        if (grabable == null) return;

        bool isGrabbed = grabable.IsGrabbed();

        if (isGrabbed)
        {
            StepPhysicsAndHaptics();
        }
        else if (wasGrabbed && !isGrabbed)
        {
            // Just released this frame
            HapticGlove glove = GetGloveForSide(connectsTo);
            if (glove != null)
            {
                StopVibrations(glove);
            }
            dynamicTime = 0f;
        }

        wasGrabbed = isGrabbed;
    }

    // -------------------------------------------------------
    // Main physics & haptics update when grabbed
    // -------------------------------------------------------
    private void StepPhysicsAndHaptics()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // --- 1) Velocity & acceleration from transform ---
        Vector3 currentPos = transform.position;
        Vector3 velocity = (currentPos - lastPosition) / dt;
        Vector3 acceleration = (velocity - lastVelocity) / dt;

        lastPosition = currentPos;
        lastVelocity = velocity;

        float speed = velocity.magnitude;        // |V(t)|
        float accelMag = acceleration.magnitude; // |A(t)|

        // --- 2) Frequency OF(t) = 100 + 50 * |V| / (|V| + 2) ---
        float dynamicFreq = 100f + 50f * (speed / (speed + 2f + 1e-5f)); // small epsilon to avoid div/0

        // --- 3) Force term α M A(t) ---
        float force = alpha * mass * accelMag;   // this is proportional to αMA(t)
        float forceClamped = Mathf.Max(0f, force);

        // --- 4) Gauge + text UI from force ---
        UpdateGaugeAndText(forceClamped);

        // --- 5) Time-scaling to simulate sin(2π t OF(t)) ---
        // we don't have direct frequency in CustomWaveform,
        // so we simulate higher frequency by increasing phase speed.
        dynamicTime += dt * dynamicFreq;  // this mimics t * OF(t) in the paper

        float sineTerm = Mathf.Sin(2f * Mathf.PI * dynamicTime); // sin(2π * (t * OF(t)))

        // --- 6) Final "output" amplitude O(t) = α M A(t) sin(...)
        float rawOutput = forceClamped * sineTerm;

        // Scale to [0,1] for haptics amplitude
        // you can tune 1/maxForceScaling to change intensity feel.
        float maxEffectiveForce = Mathf.Max(0.001f, maxForce); // avoid div/0
        float normalizedAmp = Mathf.Clamp01(Mathf.Abs(rawOutput) / maxEffectiveForce);

        if (logToConsole)
        {
            Debug.Log(
                $"[GrabVibration] |V|={speed:F3} m/s, |A|={accelMag:F3} m/s², OF(t)={dynamicFreq:F1} Hz, " +
                $"Force={forceClamped:F3} N, OutputAmp={normalizedAmp:F3}"
            );
        }

        // --- 7) Optional graph display ---
        if (graph != null)
        {
            // Use slight variations so channels are visually distinct
            float thumbVal = normalizedAmp * thumbAmplitude;
            float indexVal = normalizedAmp * indexAmplitude;
            float wristVal = normalizedAmp * wristAmplitude;
            graph.AddData(thumbVal, indexVal, wristVal);
        }

        // --- 8) Send vibration to glove ---
        HapticGlove glove = GetGloveForSide(connectsTo);
        if (glove != null && glove.IsConnected())
        {
            SendVibrations(glove, normalizedAmp);
        }
    }

    // -------------------------------------------------------
    // UI: gauge + text
    // -------------------------------------------------------
    private void UpdateGaugeAndText(float forceValue)
    {
        // Texts
        if (forceText != null)
        {
            forceText.text = $"Force: {forceValue:F2} N";
        }

        if (newtonText != null)
        {
            newtonText.text = $"{forceValue:F2} N";
        }

        // Pointer needle
        if (pointer != null)
        {
            float t = Mathf.Clamp01(forceValue / Mathf.Max(maxForce, 0.001f));
            float angle = Mathf.Lerp(minRotation, maxRotation, t);

            Vector3 euler = pointerBaseRotation.eulerAngles;
            switch (rotationAxis)
            {
                case GaugeAxis.X:
                    euler.x = angle;
                    break;
                case GaugeAxis.Y:
                    euler.y = angle;
                    break;
                case GaugeAxis.Z:
                    euler.z = angle;
                    break;
            }
            pointer.localRotation = Quaternion.Euler(euler);
        }
    }

    // -------------------------------------------------------
    // Haptic glove helpers
    // -------------------------------------------------------
    private HapticGlove GetGloveForSide(SG.HandSide side)
    {
        HapticGlove[] allGloves = HapticGlove.GetHapticGloves(true);
        foreach (var glove in allGloves)
        {
            bool isRight = glove.IsRight();
            if (side == SG.HandSide.LeftHand && !isRight) return glove;
            if (side == SG.HandSide.RightHand && isRight) return glove;
        }
        return null;
    }

    private void SendVibrations(HapticGlove glove, float globalAmp)
    {
        // globalAmp ∈ [0,1] based on O(t); per-channel amplitudes scale it.

        if (thumbWaveform != null)
        {
            var wfThumb = thumbWaveform.GetWaveform();
            wfThumb.Amplitude = Mathf.Clamp01(globalAmp * thumbAmplitude);
            SG_CustomWaveform.CallCorrectWaveform(glove, wfThumb, VibrationLocation.Thumb_Tip);
        }

        if (indexWaveform != null)
        {
            var wfIndex = indexWaveform.GetWaveform();
            wfIndex.Amplitude = Mathf.Clamp01(globalAmp * indexAmplitude);
            SG_CustomWaveform.CallCorrectWaveform(glove, wfIndex, VibrationLocation.Index_Tip);
        }

        if (wristWaveform != null)
        {
            var wfWrist = wristWaveform.GetWaveform();
            wfWrist.Amplitude = Mathf.Clamp01(globalAmp * wristAmplitude);

            if (glove is NovaGlove)
                SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.WholeHand);
            else
                SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.Palm_IndexSide);
        }
    }

    private void StopVibrations(HapticGlove glove)
    {
        var wfZero = new SGCore.CustomWaveform();
        wfZero.Amplitude = 0f;

        SG_CustomWaveform.CallCorrectWaveform(glove, wfZero, VibrationLocation.Thumb_Tip);
        SG_CustomWaveform.CallCorrectWaveform(glove, wfZero, VibrationLocation.Index_Tip);

        if (glove is NovaGlove)
            SG_CustomWaveform.CallCorrectWaveform(glove, wfZero, VibrationLocation.WholeHand);
        else
            SG_CustomWaveform.CallCorrectWaveform(glove, wfZero, VibrationLocation.Palm_IndexSide);
    }
}
