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
    // Use the HandSide enum from the SG namespace directly
    public SG.HandSide connectsTo = SG.HandSide.LeftHand;

    private SG_Grabable grabable;
    private bool wasGrabbed = false;

    void Start()
    {
        grabable = GetComponent<SG_Grabable>();
    }

    void Update()
    {
        if (grabable == null) return;

        bool isGrabbed = grabable.IsGrabbed();

        // Find the SG_DeviceSelector that matches the selected hand
        SG_DeviceSelector selectedDevice = null;
        SG_DeviceSelector[] allSelectors = FindObjectsOfType<SG_DeviceSelector>();

        foreach (var selector in allSelectors)
        {
            if (connectsTo == SG.HandSide.LeftHand)
            {
                // In 'AnyHand' mode, we process all connected devices
                break;
            }
            else if (selector.intendedFor == connectsTo)
            {
                selectedDevice = selector;
                break;
            }
        }

        // Handle the vibration logic for the selected glove(s)
        if (isGrabbed)
        {
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

    // Helper method to send vibrations to a specific glove
    private void SendVibrations(HapticGlove glove)
    {
        if (!glove.IsConnected()) return;

        if (thumbWaveform != null)
        {
            var wfThumb = thumbWaveform.GetWaveform();
            wfThumb.Amplitude = Mathf.Clamp01(thumbIntensity);
            SG_CustomWaveform.CallCorrectWaveform(glove, wfThumb, VibrationLocation.Thumb_Tip);
        }
        if (indexWaveform != null)
        {
            var wfIndex = indexWaveform.GetWaveform();
            wfIndex.Amplitude = Mathf.Clamp01(indexIntensity);
            SG_CustomWaveform.CallCorrectWaveform(glove, wfIndex, VibrationLocation.Index_Tip);
        }
        if (wristWaveform != null)
        {
            var wfWrist = wristWaveform.GetWaveform();
            wfWrist.Amplitude = Mathf.Clamp01(wristIntensity);
            if (glove is NovaGlove)
                SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.WholeHand);
            else
                SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.Palm_IndexSide);
        }
    }

    // Helper method to stop vibrations on a specific glove
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