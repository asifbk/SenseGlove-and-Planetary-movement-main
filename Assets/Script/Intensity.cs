using UnityEngine;
using SG;
using SGCore;

[RequireComponent(typeof(SG_Grabable))]
public class SG_PlayWaveformOnGrab : MonoBehaviour
{
    [Header("Assign your Custom Waveforms")]
    public SG_CustomWaveform thumbWaveform;
    public SG_CustomWaveform indexWaveform;
    public SG_CustomWaveform wristWaveform;

    private HapticGlove[] connectedGloves;
    private bool isGrabbed = false;

    void Start()
    {
        connectedGloves = HapticGlove.GetHapticGloves(true);

        // Find the GrabScript on the hand
        SG_GrabScript grabScript = FindObjectOfType<SG_GrabScript>();
        if (grabScript != null)
        {
            grabScript.GrabbedObject.AddListener(OnGrabbed);
            grabScript.ReleasedObject.AddListener(OnReleased);
        }
        else
        {
            Debug.LogWarning("No SG_GrabScript found in scene. Make sure your hand prefab has one.");
        }
    }

    void Update()
    {
        if (isGrabbed && connectedGloves != null)
        {
            foreach (var glove in connectedGloves)
            {
                if (thumbWaveform != null)
                    SG_CustomWaveform.CallCorrectWaveform(glove, thumbWaveform.GetWaveform(), VibrationLocation.Thumb_Tip);

                if (indexWaveform != null)
                    SG_CustomWaveform.CallCorrectWaveform(glove, indexWaveform.GetWaveform(), VibrationLocation.Index_Tip);

                if (wristWaveform != null)
                    SG_CustomWaveform.CallCorrectWaveform(glove, wristWaveform.GetWaveform(), VibrationLocation.WholeHand);
            }
        }
    }

    private void OnGrabbed(SG_Interactable interactable, SG_GrabScript grabber)
    {
        // Only vibrate if THIS object was grabbed
        if (interactable == GetComponent<SG_Grabable>())
        {
            isGrabbed = true;
        }
    }

    private void OnReleased(SG_Interactable interactable, SG_GrabScript grabber)
    {
        if (interactable == GetComponent<SG_Grabable>())
        {
            isGrabbed = false;

            // Stop waveforms by sending zero amplitude
            foreach (var glove in connectedGloves)
            {
                StopWaveform(glove, VibrationLocation.Thumb_Tip);
                StopWaveform(glove, VibrationLocation.Index_Tip);
                StopWaveform(glove, VibrationLocation.WholeHand);
            }
        }
    }

    private void StopWaveform(HapticGlove glove, VibrationLocation location)
    {
        SGCore.CustomWaveform stop = new SGCore.CustomWaveform();
        stop.Amplitude = 0f;
        SG_CustomWaveform.CallCorrectWaveform(glove, stop, location);
    }
}
