using UnityEngine;
using SG;

/// <summary>
/// SenseGlove SDK v2.8.1 compatible.
/// Prevents fingers from visually penetrating objects during touch / grab
/// by limiting flexion based on finger collider penetration depth.
/// No vibration output.
/// </summary>
[RequireComponent(typeof(SG_HandFeedback))]
public class FingerSurfaceLimiter_NoVibration : MonoBehaviour
{
    [Header("References")]
    public SG_HandFeedback feedbackLayer;      // auto-filled on Start
    public float maxPenetration = 0.003f;      // m — how far fingers can sink
    public float feedbackGain = 1.0f;

    private IHandFeedbackDevice gloveDevice;

    void Start()
    {
        if (feedbackLayer == null)
            feedbackLayer = GetComponent<SG_HandFeedback>();

        gloveDevice = feedbackLayer != null ? feedbackLayer.hapticHardware : null;
    }

    void Update()
    {
        if (feedbackLayer == null || gloveDevice == null || !gloveDevice.IsConnected())
            return;

        var ff = feedbackLayer.fingerFeedbackScripts;
        if (ff == null || ff.Length == 0) return;

        bool[] lockFingers = new bool[5];
        float[] lockValues = new float[5];

        for (int i = 0; i < ff.Length && i < 5; i++)
        {
            float dist = ff[i].DistanceInCollider;

            if (dist > 0 && dist < maxPenetration)
            {
                float ratio = Mathf.Clamp01(1f - (dist / maxPenetration)) * feedbackGain;
                lockFingers[i] = true;
                lockValues[i] = ratio;
            }
            else
            {
                lockFingers[i] = false;
                lockValues[i] = 0f;
            }
        }

        // Apply proportional flexion locks to stop finger penetration
        gloveDevice.SetFlexionLocks(lockFingers, lockValues);
    }
}
