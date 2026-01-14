using UnityEngine;
using SGCore;
using SG;
using DataEcho;
using System.Collections.Generic;

public class SimpleFlexionDisplay : MonoBehaviour
{
    public SG_TrackedHand trackedHandLeft;
    public SG_TrackedHand trackedHandRight;
    public float updateInterval = 0.1f;

    private float lastUpdateTime = 0f;

    void Start()
    {
        DataEcho.EventsListener.SetCaptureExternalInputDeviceDataCallback(() =>
        {
            List<XRInputDeviceData> inputDevicesData = new List<XRInputDeviceData>();

            // Helper local function to capture data from each hand
            void CaptureHandData(SG_TrackedHand trackedHand)
            {
                if (trackedHand == null) return;

                float[] flexions;
                if (trackedHand.GetNormalizedFlexion(out flexions) && flexions.Length >= 5)
                {
                    float thumbForce = flexions[0];
                    float indexForce = flexions[1];
                    float middleForce = flexions[2];
                    float ringForce = flexions[3];
                    float pinkyForce = flexions[4];

                    string handSide = trackedHand.handModel.handSide == HandSide.RightHand ? "RightHand" : "LeftHand";

                    // Debug.Log($"{handSide} → " +
                    //           $"Thumb: {thumbForce:F1}N | " +
                    //           $"Index: {indexForce:F1}N | " +
                    //           $"Middle: {middleForce:F1}N | " +
                    //           $"Ring: {ringForce:F1}N | " +
                    //           $"Pinky: {pinkyForce:F1}N");

                    var device = new XRInputDeviceData
                    {
                        deviceName = "SenseGlove",
                        deviceRole = handSide,
                        characteristics = new List<string> { "HandTracking", "Glove", handSide },
                        features = new Dictionary<string, object>
                        {
                            { "ThumbFingerForce", thumbForce },
                            { "IndexFingerForce", indexForce },
                            { "MiddleFingerForce", middleForce },
                            { "RingFingerForce", ringForce },
                            { "PinkyFingerForce", pinkyForce }
                        }
                    };

                    inputDevicesData.Add(device);
                }
            }

            // Capture both hands
            CaptureHandData(trackedHandLeft);
            CaptureHandData(trackedHandRight);

            return inputDevicesData;
        });
    }
}
