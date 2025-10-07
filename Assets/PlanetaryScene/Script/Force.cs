using UnityEngine;
using SGCore;
using SG;

public class SimpleFlexionDisplay : MonoBehaviour
{
    public SG_TrackedHand trackedHand;
    public float updateInterval = 0.1f;
    
    private float lastUpdateTime = 0f;

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval && trackedHand != null)
        {
            float[] flexions;
            if (trackedHand.GetNormalizedFlexion(out flexions) && flexions.Length >= 5)
            {
                // Convert flexion to force in Newtons (0-20N range)
                float thumbForce = flexions[0] * 20f;
                float indexForce = flexions[1] * 20f;
                float middleForce = flexions[2] * 20f;
                float ringForce = flexions[3] * 20f;
                float pinkyForce = flexions[4] * 20f;

                Debug.Log($"Thumb: {thumbForce:F1}N | " +
                         $"Index: {indexForce:F1}N | " +
                         $"Middle: {middleForce:F1}N | " +
                         $"Ring: {ringForce:F1}N | " +
                         $"Pinky: {pinkyForce:F1}N");
            }
            lastUpdateTime = Time.time;
        }
    }
}