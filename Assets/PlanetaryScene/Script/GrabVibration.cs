using UnityEngine;
using TMPro;
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

    [Header("Waveforms")]
    public SG_CustomWaveform thumbWaveform;
    public SG_CustomWaveform indexWaveform;
    public SG_CustomWaveform wristWaveform;
    public SG.HandSide connectsTo = SG.HandSide.LeftHand;

    [Header("Manual Frequency Override (Hz)")]
    public float thumbFrequency = 180f;
    public float indexFrequency = 180f;
    public float wristFrequency = 100f;

    [Header("Optional Debug Graph")]
    public UIBarGraphWithLabels graph;

    [Header("Physical Parameters")]
    [Tooltip("Set per planet (e.g., Earth=1, Jupiter=10, etc.)")]
    public float mass = 1f;

    [Header("Gauge UI")]
    public TextMeshProUGUI forceText;
    public TextMeshProUGUI newtonText;
    public Transform pointer;

    [Header("Gauge Settings")]
    public float maxForce = 10f;
    public float minRotation = 0f;
    public float maxRotation = 180f;
    public GaugeAxis rotationAxis = GaugeAxis.Z;

    private SG_Grabable grabable;
    private bool wasGrabbed = false;
    private float timeElapsed = 0f;
    private Quaternion baseRotation;
    private Vector3 baseLocalPosition;

    void Start()
    {
        grabable = GetComponent<SG_Grabable>();
        if (pointer != null)
        {
            baseRotation = pointer.localRotation;
            baseLocalPosition = pointer.localPosition;
            
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
                HapticGlove[] allGloves = HapticGlove.GetHapticGloves(true);
                foreach (var glove in allGloves)
                {
                    if (!glove.IsRight())
                    {
                        SendVibrations(glove);
                        break;
                    }
                }
            }
            else if (selectedDevice?.CurrentHaptics is HapticGlove glove)
            {
                SendVibrations(glove);
            }
        }
        else if (wasGrabbed)
        {
            timeElapsed = 0f;
            if (connectsTo == SG.HandSide.LeftHand)
            {
                HapticGlove[] allGloves = HapticGlove.GetHapticGloves(true);
                foreach (var glove in allGloves)
                {
                    if (!glove.IsRight())
                    {
                        StopVibrations(glove);
                        break;
                    }
                }
            }
            else if (selectedDevice?.CurrentHaptics is HapticGlove glove)
            {
                StopVibrations(glove);
            }
        }

        wasGrabbed = isGrabbed;
    }

    private void SendVibrations(HapticGlove glove)
    {
        if (!glove.IsConnected()) return;

        // Frequencies (not used for force metric, only for signal)
        float f_index = indexFrequency;
        float f_thumb = thumbFrequency;
        float f_wrist = wristFrequency;

        // Generate time-based waveforms for graph
        float sinWave = indexAmplitude * Mathf.Sin(2 * Mathf.PI * f_index * timeElapsed);
        float triWave = thumbAmplitude * (2f / Mathf.PI) * Mathf.Asin(Mathf.Sin(2 * Mathf.PI * f_thumb * timeElapsed));
        float squareWave = wristAmplitude * Mathf.Sign(Mathf.Sin(2 * Mathf.PI * f_wrist * timeElapsed));

        // --- Force calculation (RMS-based metric) ---
        const float SINE_RMS = 0.70710678f;   // 1/√2
        const float TRI_RMS  = 0.57735027f;   // 1/√3
        const float SQR_RMS  = 1.0f;          // square

        float a_i = indexAmplitude * SINE_RMS; // sine
        float a_t = thumbAmplitude * TRI_RMS;  // triangle
        float a_w = wristAmplitude * SQR_RMS;  // square

        float A_eff = Mathf.Sqrt(a_i * a_i + a_t * a_t + a_w * a_w);
        float totalForce = mass * A_eff;
        float absTotalForce = Mathf.Abs(totalForce);

        // --- Update Gauge ---
        if (forceText != null)
            forceText.text = absTotalForce.ToString("F2");

        if (newtonText != null)
            newtonText.text = "N";

        if (pointer != null)
        {
            pointer.position = pointer.parent.position;
            pointer.rotation = Quaternion.Euler(
                0f, 0f,
                Mathf.Lerp(minRotation, maxRotation, Mathf.Clamp01(absTotalForce / maxForce))
            );
        }

        // --- Update Debug Graph ---
        if (graph != null)
            graph.AddData(triWave, sinWave, squareWave);

        // --- Send Vibrations to Glove ---
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
