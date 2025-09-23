using UnityEngine;
using SG;
using SGCore;
using SGCore.Nova;

[RequireComponent(typeof(SG_Grabable))]
public class GrabVibration : MonoBehaviour
{
    [Header("Vibration Amplitudes (0-1)")]
    [Range(0f, 1f)] public float thumbAmplitude = 0.3f;
    [Range(0f, 1f)] public float indexAmplitude = 0.6f;
    [Range(0f, 1f)] public float wristAmplitude = 0.8f;

    [Header("Waveforms (assign the same type used by your Actuation Controller)")]
    public SG_CustomWaveform thumbWaveform;
    public SG_CustomWaveform indexWaveform;
    public SG_CustomWaveform wristWaveform;
    public SG.HandSide connectsTo = SG.HandSide.LeftHand;

    [Header("Optional Debug Graph")]
    public UIBarGraphWithLabels graph;

    [Header("Physical Parameters")]
    public float mass = 10f;   // kg
    public float alpha = 1f;   // scaling factor for force
    public float frequency = 180f; // Hz

    private SG_Grabable grabable;
    private bool wasGrabbed = false;
    private float timeElapsed = 0f;

    void Start()
    {
        grabable = GetComponent<SG_Grabable>();
    }

    void Update()
    {
        if (grabable == null) return;

        bool isGrabbed = grabable.IsGrabbed();

        SG_DeviceSelector selectedDevice = null;
        SG_DeviceSelector[] allSelectors = FindObjectsOfType<SG_DeviceSelector>();

        foreach (var selector in allSelectors)
        {
            if (connectsTo == SG.HandSide.LeftHand)
                break;
            else if (selector.intendedFor == connectsTo)
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
                HapticGlove[] allGloves = HapticGlove.GetHapticGloves(true);
                foreach (var glove in allGloves)
                {
                    SendVibrations(glove);
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
                    HapticGlove[] allGloves = HapticGlove.GetHapticGloves(true);
                    foreach (var glove in allGloves)
                    {
                        StopVibrations(glove);
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

        // Calculate each waveform separately using its own amplitude
        float sinWave = indexAmplitude * Mathf.Sin(2 * Mathf.PI * frequency * timeElapsed);   // Index = sin
        float triWave = thumbAmplitude * (2f / Mathf.PI) * Mathf.Asin(Mathf.Sin(2 * Mathf.PI * frequency * timeElapsed)); // Thumb = triangle
        float squareWave = wristAmplitude * Mathf.Sign(Mathf.Sin(2 * Mathf.PI * frequency * timeElapsed)); // Wrist = square

        // Total force
        float totalForce = alpha * mass * (sinWave + triWave + squareWave);
        float abstotalForce = Mathf.Abs(totalForce);

        Debug.Log($"Thumb (Tri): {triWave:F2}, Index (Sin): {sinWave:F2}, Wrist (Square): {squareWave:F2}, Total Force: {abstotalForce:F2} N");

        // Update graph if assigned
        if (graph != null)
        {
            graph.AddData(triWave, sinWave, squareWave);
        }

        // Send haptic vibrations
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
