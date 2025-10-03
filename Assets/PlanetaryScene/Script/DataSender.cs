using UnityEngine;
using System.IO;

public class DataToFile : MonoBehaviour
{
    string filePath;

    [Header("Waveform Parameters")]
    public float alpha = 1f;      // scale factor
    public float mass = 1f;       // virtual mass
    public float sinAmplitude = 1f;
    public float triAmplitude = 0.5f;
    public float squareAmplitude = 0.2f;
    public float frequency = 10f;

    private float timeElapsed = 0f;

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "data.txt");
        File.WriteAllText(filePath, "Time, SinWave, TriWave, SquareWave, TotalAmplitude\n");
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        // Individual waveform components
        float sinWave = sinAmplitude * Mathf.Sin(2 * Mathf.PI * frequency * timeElapsed);
        float triWave = triAmplitude * (2f / Mathf.PI) * Mathf.Asin(Mathf.Sin(2 * Mathf.PI * frequency * timeElapsed));
        float squareWave = squareAmplitude * Mathf.Sign(Mathf.Sin(2 * Mathf.PI * frequency * timeElapsed));

        // Total amplitude
        float vibrationAmplitude = alpha * mass * (sinWave + triWave + squareWave);

        // Save all values to file
        string line = string.Format("{0:F3},{1:F3},{2:F3},{3:F3},{4:F3}\n",
            timeElapsed, sinWave, triWave, squareWave, vibrationAmplitude);

        File.AppendAllText(filePath, line);

        // Optional: send to device
        SendVibration(vibrationAmplitude);
    }

    void SendVibration(float amplitude)
    {
        Debug.Log("Vibration amplitude: " + amplitude);
        // SGCore.SG.YourDevice.SetMotorAmplitude(amplitude);
    }
}
