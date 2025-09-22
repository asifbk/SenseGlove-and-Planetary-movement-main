using UnityEngine;
using SG;
using SGCore;
using SGCore.Nova;

public class SG_VibrationController : MonoBehaviour
{
    [Header("Vibration Intensities (0-1)")]
    [Range(0f, 1f)] public float thumbIntensity = 0.5f;
    [Range(0f, 1f)] public float indexIntensity = 0.5f;
    [Range(0f, 1f)] public float wristIntensity = 0.5f;

    [Header("Waveforms (Create via Assets > Create > SenseGlove > Custom Waveform)")]
    public SG_CustomWaveform thumbWaveform;
    public SG_CustomWaveform indexWaveform;
    public SG_CustomWaveform wristWaveform;

    private HapticGlove[] connectedGloves;

    void Start()
    {
        connectedGloves = HapticGlove.GetHapticGloves(true);
        if (connectedGloves.Length == 0)
        {
            Debug.LogWarning("No SenseGlove connected!");
        }
    }

    void Update()
    {
        if (connectedGloves == null || connectedGloves.Length == 0) return;

        foreach (var glove in connectedGloves)
        {
            if (glove is NovaGlove nova1) // Nova 1
            {
                SendWaveform(nova1, thumbWaveform, VibrationLocation.Thumb_Tip, thumbIntensity);
                SendWaveform(nova1, indexWaveform, VibrationLocation.Index_Tip, indexIntensity);
                SendWaveform(nova1, wristWaveform, VibrationLocation.WholeHand, wristIntensity);
            }
            else if (glove is Nova2Glove nova2) // Nova 2
            {
                SendWaveform(nova2, thumbWaveform, VibrationLocation.Thumb_Tip, thumbIntensity);
                SendWaveform(nova2, indexWaveform, VibrationLocation.Index_Tip, indexIntensity);
                SendWaveform(nova2, wristWaveform, VibrationLocation.Palm_IndexSide, wristIntensity);
            }
        }
    }

    private void SendWaveform(HapticGlove glove, SG_CustomWaveform baseWaveform, VibrationLocation location, float intensity)
    {
        if (baseWaveform == null) return;

        // Clone waveform and scale amplitude
        var wf = baseWaveform.GetWaveform();
        wf.Amplitude = Mathf.Clamp01(intensity);

        SG_CustomWaveform.CallCorrectWaveform(glove, wf, location);
    }
}
