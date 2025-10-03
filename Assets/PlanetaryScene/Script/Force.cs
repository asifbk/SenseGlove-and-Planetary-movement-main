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
                // Calculate force values based on flexion (0-100 scale)
                int thumbForce = Mathf.RoundToInt(flexions[0] * 100);
                int indexForce = Mathf.RoundToInt(flexions[1] * 100);
                int middleForce = Mathf.RoundToInt(flexions[2] * 100);
                int ringForce = Mathf.RoundToInt(flexions[3] * 100);
                int pinkyForce = Mathf.RoundToInt(flexions[4] * 100);

                Debug.Log($"Thumb: {flexions[0]:F3} ({thumbForce}%) | " +
                         $"Index: {flexions[1]:F3} ({indexForce}%) | " +
                         $"Middle: {flexions[2]:F3} ({middleForce}%) | " +
                         $"Ring: {flexions[3]:F3} ({ringForce}%) | " +
                         $"Pinky: {flexions[4]:F3} ({pinkyForce}%)");
            }
            lastUpdateTime = Time.time;
        }
    }
}