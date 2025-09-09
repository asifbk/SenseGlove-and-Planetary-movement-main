using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SG;
using TMPro;

public class PlanetWeightUI_Grab : MonoBehaviour
{
    [Header("SenseGlove Reference")]
    public SG_HapticGlove glove;

    [Header("Grab Settings")]
    public Transform grabbedPlanet;
    public float massMultiplier = 0.05f;
    public float grabDistance = 0.3f;

    [Header("UI")]
    public Canvas planetInfoCanvas;
    public TextMeshProUGUI planetInfoText;

    private Vector3 previousWristPos;
    private bool isGrabbing = false;

    void Start()
    {
        if (planetInfoCanvas != null)
            planetInfoCanvas.gameObject.SetActive(false); // hide at start
    }

    void Update()
    {
        if (glove == null || grabbedPlanet == null || planetInfoCanvas == null || planetInfoText == null)
            return;

        // Get hand pose
        SG_HandPose handPose;
        if (!glove.GetHandPose(out handPose)) return;

        Vector3 wristPos = handPose.wristPosition;

        // Calculate distance to planet
        float distance = Vector3.Distance(wristPos, grabbedPlanet.position);

        // Check if fingers are "closed" enough to grab (flexion threshold)
        float[] flexions;
        if (!glove.GetNormalizedFlexion(out flexions)) return;

        bool handClosed = true;
        for (int i = 0; i < flexions.Length; i++)
        {
            if (flexions[i] < 0.7f) // adjust threshold
            {
                handClosed = false;
                break;
            }
        }

        // Detect grab
        if (distance <= grabDistance && handClosed)
        {
            if (!isGrabbing)
            {
                isGrabbing = true;
                previousWristPos = wristPos;
                planetInfoCanvas.gameObject.SetActive(true); // show UI
            }

            ApplyWeightFeedback(wristPos);
            UpdateUI(distance);
        }
        else
        {
            if (isGrabbing)
            {
                isGrabbing = false;
                glove.StopAllVibrations();
                planetInfoCanvas.gameObject.SetActive(false); // hide UI
            }
        }
    }

    void ApplyWeightFeedback(Vector3 wristPos)
    {
        Vector3 handVelocity = wristPos - previousWristPos;

        Rigidbody rb = grabbedPlanet.GetComponent<Rigidbody>();
        float mass = rb != null ? rb.mass : 1f;

        float resistance = mass * massMultiplier * handVelocity.magnitude;
        float ffbValue = Mathf.Clamp01(resistance);

        float[] fingerForce = new float[5] { ffbValue, ffbValue, ffbValue, ffbValue, ffbValue };
        glove.QueueFFBCmd(fingerForce);

        previousWristPos = wristPos;
    }

    void UpdateUI(float distance)
    {
        Rigidbody rb = grabbedPlanet.GetComponent<Rigidbody>();
        float mass = rb != null ? rb.mass : 1f;
        float radius = grabbedPlanet.localScale.x * 0.5f;
        float weightValue = mass * 9.8f * massMultiplier;

        planetInfoText.text = $"Planet: {grabbedPlanet.name}\n" +
                              $"Mass: {mass:F2} kg\n" +
                              $"Radius: {radius:F2} m\n" +
                              $"Distance: {distance:F2} m\n" +
                              $"Weight: {weightValue:F2} N";
    }
}
