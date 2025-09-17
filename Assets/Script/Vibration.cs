using UnityEngine;
using SG;
using SGCore;
using SGCore.Nova;

/// <summary>
/// Attach to any GameObject with SG_Grabable.
/// Sends the assigned waveforms while the object is grabbed,
/// and sends zero-amplitude versions once when the object is released to stop vibrations.
/// Uses the same SG_CustomWaveform.CallCorrectWaveform(...) pattern as your Actuation Controller.
/// </summary>
[RequireComponent(typeof(SG_Grabable))]
public class GrabVibration : MonoBehaviour
{
    [Header("Vibration Intensities (0-1)")]
    [Range(0f, 1f)] public float thumbIntensity = 0.5f;
    [Range(0f, 1f)] public float indexIntensity = 0.5f;
    [Range(0f, 1f)] public float wristIntensity = 0.5f;

    [Header("Waveforms (assign the same type used by your Actuation Controller)")]
    public SG_CustomWaveform thumbWaveform;
    public SG_CustomWaveform indexWaveform;
    public SG_CustomWaveform wristWaveform;

    private SG_Grabable grabable;
    private HapticGlove[] connectedGloves;
    private bool wasGrabbed = false; // track transitions to send stop once

    void Start()
    {
        grabable = GetComponent<SG_Grabable>();
        connectedGloves = HapticGlove.GetHapticGloves(true);

        if (connectedGloves == null || connectedGloves.Length == 0)
        {
            Debug.LogWarning("GrabVibration: no SenseGlove found by HapticGlove.GetHapticGloves(true).");
        }
    }

    void Update()
    {
        if (grabable == null || connectedGloves == null) return;

        bool isGrabbed = grabable.IsGrabbed();

        if (isGrabbed)
        {
            // While grabbed: continuously send configured waveforms (same pattern as Actuation Controller)
            foreach (var glove in connectedGloves)
            {
                // Thumb
                if (thumbWaveform != null)
                {
                    var wfThumb = thumbWaveform.GetWaveform();
                    wfThumb.Amplitude = Mathf.Clamp01(thumbIntensity);
                    SG_CustomWaveform.CallCorrectWaveform(glove, wfThumb, VibrationLocation.Thumb_Tip);
                }

                // Index
                if (indexWaveform != null)
                {
                    var wfIndex = indexWaveform.GetWaveform();
                    wfIndex.Amplitude = Mathf.Clamp01(indexIntensity);
                    SG_CustomWaveform.CallCorrectWaveform(glove, wfIndex, VibrationLocation.Index_Tip);
                }

                // Wrist (choose location per glove type, matching your Actuation Controller)
                if (wristWaveform != null)
                {
                    var wfWrist = wristWaveform.GetWaveform();
                    wfWrist.Amplitude = Mathf.Clamp01(wristIntensity);

                    if (glove is NovaGlove)
                        SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.WholeHand);
                    else // Nova2Glove and others
                        SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.Palm_IndexSide);
                }
            }
        }
        else
        {
            // On release: only when transitioning from grabbed->released, send zero-amplitude waveforms once to stop vibrations.
            if (wasGrabbed)
            {
                foreach (var glove in connectedGloves)
                {
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
        }

        wasGrabbed = isGrabbed;
    }
}
