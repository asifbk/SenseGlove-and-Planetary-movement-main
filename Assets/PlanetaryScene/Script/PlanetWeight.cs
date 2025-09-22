using UnityEngine;
using SG;
using SGCore;
using SGCore.Haptics;
using UnityEngine.XR;

[RequireComponent(typeof(SG_Grabable))]
public class PlanetWeight : MonoBehaviour
{
    [Header("Weight Simulation Settings")]
    [Range(0.0f, 1.0f)]
    public float relativeMass = 0.5f;

    private SG_Grabable grabbable;

    void OnEnable()
    {
        grabbable = GetComponent<SG_Grabable>();
        if (grabbable != null)
        {
            grabbable.ObjectGrabbed.AddListener(ApplyWeightEffect);
            grabbable.ObjectReleased.AddListener(StopWeightEffect);
        }
    }

    void OnDisable()
    {
        if (grabbable != null)
        {
            grabbable.ObjectGrabbed.RemoveListener(ApplyWeightEffect);
            grabbable.ObjectReleased.RemoveListener(StopWeightEffect);
        }
    }

    void ApplyWeightEffect(SG_Interactable interactable, SG_GrabScript grabScript)
    {
        if (grabScript != null && grabScript.TrackedHand != null)
        {
            // High-level force feedback
            IHandFeedbackDevice hapticDevice = grabScript.TrackedHand.HapticHardware;
            if (hapticDevice != null)
            {
                float forceLevel = Mathf.Clamp01(relativeMass);
                hapticDevice.QueueFFBCmd(new float[] { forceLevel, forceLevel, forceLevel, forceLevel, forceLevel });
            }

            // Method 1: Check through TrackedHand component
            SG_TrackedHand trackedHand = grabScript.TrackedHand;
            if (trackedHand != null)
            {
                // Get the hand chirality from TrackedHand
                bool isRight = trackedHand.TracksRightHand();
                SGCore.HapticGlove matchedCoreGlove = GetMatchedCoreGlove(isRight);

                if (matchedCoreGlove != null)
                {
                    SGCore.CustomWaveform wf = new SGCore.CustomWaveform();
                    wf.Amplitude = Mathf.Clamp01(relativeMass * 0.5f);
                    wf.SustainTime = 0.2f;

                    SG_CustomWaveform.CallCorrectWaveform(matchedCoreGlove, wf, VibrationLocation.WholeHand);
                }
            }

            // Alternative Method 2: Direct approach using hardware device
            // You can also try this if Method 1 doesn't work:
            
            if (hapticDevice is SGCore.HapticGlove coreGlove)
            {
                SGCore.CustomWaveform wf = new SGCore.CustomWaveform();
                wf.Amplitude = Mathf.Clamp01(relativeMass * 0.5f);
                wf.SustainTime = 0.2f;

                SG_CustomWaveform.CallCorrectWaveform(coreGlove, wf, VibrationLocation.WholeHand);
            }
        }
    }

    void StopWeightEffect(SG_Interactable interactable, SG_GrabScript grabScript)
    {
        if (grabScript != null && grabScript.TrackedHand != null)
        {
            IHandFeedbackDevice hapticDevice = grabScript.TrackedHand.HapticHardware;
            if (hapticDevice != null)
            {
                hapticDevice.StopHaptics();
            }
        }
    }

    private SGCore.HapticGlove GetMatchedCoreGlove(bool isRight)
    {
        SGCore.HapticGlove[] connectedGloves = SGCore.HapticGlove.GetHapticGloves(true);
        
        for (int i = 0; i < connectedGloves.Length; i++)
        {
            if (connectedGloves[i].IsRight() == isRight)
            {
                return connectedGloves[i];
            }
        }
        
        return null;
    }
}