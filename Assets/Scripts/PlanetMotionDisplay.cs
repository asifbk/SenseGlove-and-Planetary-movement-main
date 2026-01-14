using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlanetMotionDisplay : MonoBehaviour
{
    public PlanetMotionTracker motionTracker;
    public TextMeshProUGUI displayText;
    public Image backgroundImage;
    public float updateInterval = 0.1f;
    public float accelerationThreshold = 5f;

    private float nextUpdateTime;
    private Color normalColor = new Color(0f, 0f, 0f, 0.8f);
    private Color warningColor = new Color(0f, 0.8f, 0f, 0.8f);

    private void Start()
    {
        if (motionTracker == null)
        {
            motionTracker = FindObjectOfType<PlanetMotionTracker>();
        }

        if (displayText == null)
        {
            displayText = GetComponent<TextMeshProUGUI>();
        }

        if (backgroundImage == null)
        {
            backgroundImage = GetComponentInParent<Image>();
        }
    }

    private void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateDisplay();
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void UpdateDisplay()
    {
        if (motionTracker == null || displayText == null)
            return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        Transform grabbedPlanet = motionTracker.currentlyGrabbedPlanet;
        
        if (grabbedPlanet != null)
        {
            PlanetMotionTracker.PlanetMotionData data = motionTracker.GetPlanetData(grabbedPlanet);
            
            if (data != null)
            {
                sb.AppendLine($"<b><size=20>{data.planet.name}</size></b>\n");
                sb.AppendLine($"<b>Velocity:</b> {data.velocity.magnitude:F2} m/s");
                sb.AppendLine($"<b>Acceleration:</b> {data.acceleration.magnitude:F2} m/s²");
                
                if (backgroundImage != null)
                {
                    backgroundImage.color = data.acceleration.magnitude >= accelerationThreshold ? warningColor : normalColor;
                }
            }
        }
        else
        {
            sb.AppendLine("<b><size=18>Grab a Planet</size></b>\n");
            sb.AppendLine("<i>Motion data will appear here</i>");
            
            if (backgroundImage != null)
            {
                backgroundImage.color = normalColor;
            }
        }

        displayText.text = sb.ToString();
    }
}
