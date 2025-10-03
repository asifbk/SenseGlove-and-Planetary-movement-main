using UnityEngine;
using SGCore.Nova;

public class ForceDebugger : MonoBehaviour
{
    public float deviceMaxForce = 20f;
    public float logInterval = 0.1f;
    private float lastLogTime = -999f;

    void Update()
    {
        NovaGlove[] gloves = NovaGlove.GetNovaGloves();
        if (gloves.Length == 0) return;

        NovaGlove glove = gloves[0];
        if (glove == null || !glove.IsConnected()) return;

        // Try to get live sensor data
        Nova_SensorData sensorData;
        if (glove.GetSensorData(out sensorData))
        {
            // 👇 Now we need to see what sensorData exposes
            // Examples: sensorData.ForceLevels, sensorData.FingerForces, etc.
            // For now, just dump the struct:
            if (Time.time - lastLogTime >= logInterval)
            {
                Debug.Log(sensorData.ToString());
                lastLogTime = Time.time;
            }
        }
    }
}
