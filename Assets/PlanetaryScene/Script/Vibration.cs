using UnityEngine;
using TMPro; // for TextMeshPro
using SG;
using SGCore;
using SGCore.Nova;

public enum GaugeAxis { X, Y, Z }

[RequireComponent(typeof(SG_Grabable))]
public class GrabVibration : MonoBehaviour
{
    [Header("Vibration Amplitudes (0-1)")]
    [Range(0f, 1f)] public float thumbAmplitude = 0.3f;
    [Range(0f, 1f)] public float indexAmplitude = 0.6f;
    [Range(0f, 1f)] public float wristAmplitude = 0.8f;

    [Header("Waveforms (assign same type as Actuation Controller)")]
    public SG_CustomWaveform thumbWaveform;
    public SG_CustomWaveform indexWaveform;
    public SG_CustomWaveform wristWaveform;
    public SG.HandSide connectsTo = SG.HandSide.LeftHand;

    [Header("Optional Debug Graph")]
    public UIBarGraphWithLabels graph;

    [Header("Physical Parameters")]
    public float mass = 10f;     // kg
    public float alpha = 1f;     // scaling factor for force
    public float frequency = 180f; // Hz

    [Header("Gauge UI")]
    public TextMeshProUGUI forceText;  // Drag "Force Text" here
    public TextMeshProUGUI newtonText; // Drag "Newton" here
    public Transform pointer;          // Drag "Pointer" here

    [Header("Gauge Settings")]
    public float maxForce = 10f;         // Expected max force for full scale
    public float minRotation = 0f;     // Leftmost angle
    public float maxRotation = 180f;      // Rightmost angle
    public GaugeAxis rotationAxis = GaugeAxis.Z; // Default Z-axis

    private SG_Grabable grabable;
    private bool wasGrabbed = false;
    private float timeElapsed = 0f;
    private Quaternion baseRotation;      // store initial rotation
    private Vector3 baseLocalPosition;    // store initial local position

    void Start()
    {
        grabable = GetComponent<SG_Grabable>();

        if (pointer != null)
        {
            baseRotation = pointer.localRotation;      // initial rotation
            baseLocalPosition = pointer.localPosition; // initial position (e.g., 25,1,0)
        }
    }

    void Update()
    {
        if (grabable == null) return;

        bool isGrabbed = grabable.IsGrabbed();

        SG_DeviceSelector selectedDevice = null;
        SG_DeviceSelector[] allSelectors = FindObjectsOfType<SG_DeviceSelector>();

        foreach (var selector in allSelectors)
        {
            if (selector.intendedFor == connectsTo)
            {
                selectedDevice = selector;
                break;
            }
        }

        if (isGrabbed)
        {
            timeElapsed += Time.deltaTime;

            if (connectsTo == SG.HandSide.LeftHand)
            {
                // Only get left-handed gloves
                HapticGlove[] allGloves = HapticGlove.GetHapticGloves(true);
                foreach (var glove in allGloves)
                {
                    // Check if this is a left-handed glove
                    if (glove.IsRight() == false) // Left hand
                    {
                        SendVibrations(glove);
                        break; // Only send to one left glove
                    }
                }
            }
            else if (selectedDevice != null && selectedDevice.CurrentHaptics is HapticGlove)
            {
                SendVibrations((HapticGlove)selectedDevice.CurrentHaptics);
            }
        }
        else
        {
            if (wasGrabbed)
            {
                timeElapsed = 0f; // reset time

                if (connectsTo == SG.HandSide.LeftHand)
                {
                    // Only stop vibrations on left-handed gloves
                    HapticGlove[] allGloves = HapticGlove.GetHapticGloves(true);
                    foreach (var glove in allGloves)
                    {
                        if (glove.IsRight() == false) // Left hand
                        {
                            StopVibrations(glove);
                            break; // Only stop one left glove
                        }
                    }
                }
                else if (selectedDevice != null && selectedDevice.CurrentHaptics is HapticGlove)
                {
                    StopVibrations((HapticGlove)selectedDevice.CurrentHaptics);
                }
            }
        }

        wasGrabbed = isGrabbed;
    }

    private void SendVibrations(HapticGlove glove)
    {
        if (!glove.IsConnected()) return;

        // Waveform components
        float sinWave = indexAmplitude * Mathf.Sin(2 * Mathf.PI * frequency * timeElapsed);
        float triWave = thumbAmplitude * (2f / Mathf.PI) * Mathf.Asin(Mathf.Sin(2 * Mathf.PI * frequency * timeElapsed));
        float squareWave = wristAmplitude * Mathf.Sign(Mathf.Sin(2 * Mathf.PI * frequency * timeElapsed));

        // Total net force
        float totalForce = alpha * mass * (sinWave + triWave + squareWave);
        float abstotalForce = Mathf.Abs(totalForce);

        Debug.Log($"Thumb (Tri): {triWave:F2}, Index (Sin): {sinWave:F2}, Wrist (Square): {squareWave:F2}, Total Force: {abstotalForce:F2} N");

        // --- Update Gauge ---
        if (forceText != null)
            forceText.text = abstotalForce.ToString("F2"); // show 2 decimals

        if (newtonText != null)
            newtonText.text = "N"; // static label

        if (pointer != null)
        {
            // Keep pointer exactly at holder pivot
            pointer.position = pointer.parent.position; // world position locked

            // Rotate pointer around Z axis in world space
            pointer.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(minRotation, maxRotation, Mathf.Clamp01(abstotalForce / maxForce)));
        }

        // Update debug graph if present
        if (graph != null)
            graph.AddData(triWave, sinWave, squareWave);

        // --- Send haptic vibrations ---
        if (thumbWaveform != null)
        {
            var wfThumb = thumbWaveform.GetWaveform();
            wfThumb.Amplitude = Mathf.Clamp01(thumbAmplitude);
            SG_CustomWaveform.CallCorrectWaveform(glove, wfThumb, VibrationLocation.Thumb_Tip);
        }
        if (indexWaveform != null)
        {
            var wfIndex = indexWaveform.GetWaveform();
            wfIndex.Amplitude = Mathf.Clamp01(indexAmplitude);
            SG_CustomWaveform.CallCorrectWaveform(glove, wfIndex, VibrationLocation.Index_Tip);
        }
        if (wristWaveform != null)
        {
            var wfWrist = wristWaveform.GetWaveform();
            wfWrist.Amplitude = Mathf.Clamp01(wristAmplitude);
            if (glove is NovaGlove)
                SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.WholeHand);
            else
                SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.Palm_IndexSide);
        }
    }

    private void StopVibrations(HapticGlove glove)
    {
        if (!glove.IsConnected()) return;

        if (thumbWaveform != null)
        {
            var wfThumb = thumbWaveform.GetWaveform();
            wfThumb.Amplitude = 0f;
            SG_CustomWaveform.CallCorrectWaveform(glove, wfThumb, VibrationLocation.Thumb_Tip);
        }
        if (indexWaveform != null)
        {
            var wfIndex = indexWaveform.GetWaveform();
            wfIndex.Amplitude = 0f;
            SG_CustomWaveform.CallCorrectWaveform(glove, wfIndex, VibrationLocation.Index_Tip);
        }
        if (wristWaveform != null)
        {
            var wfWrist = wristWaveform.GetWaveform();
            wfWrist.Amplitude = 0f;
            if (glove is NovaGlove)
                SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.WholeHand);
            else
                SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.Palm_IndexSide);
        }
    }
}