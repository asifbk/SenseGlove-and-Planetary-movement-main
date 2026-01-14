using UnityEngine;
using TMPro;
using SG;
using SGCore;
using SGCore.Nova;
using System.IO;

public enum GaugeAxis { X, Y, Z }

[RequireComponent(typeof(SG_Grabable))]
public class GrabVibration : MonoBehaviour
{
    [Header("Physics Parameters")]
    [Tooltip("Mass of the object (kg). Affects force calculation.")]
    public float mass = 1.0f;

    [Tooltip("Maximum acceleration threshold (m/s²).")]
    public float aMax = 5.0f;

    [Tooltip("Minimum acceleration threshold to trigger vibration (m/s²).")]
    public float aMin = 0.1f;

    [Header("Haptic Frequency Range (Hz)")]
    [Tooltip("Minimum vibration frequency.")]
    public float fMin = 100f;

    [Tooltip("Maximum vibration frequency.")]
    public float fMax = 180f;

    [Header("Waveform Amplitudes per Channel (0–1)")]
    [Range(0f, 1f)] public float thumbAmplitude = 0.3f;
    [Range(0f, 1f)] public float indexAmplitude = 0.6f;
    [Range(0f, 1f)] public float wristAmplitude = 0.8f;

    [Header("Base Waveforms (Create via Assets > Create > SenseGlove > Custom Waveform)")]
    public SG_CustomWaveform thumbWaveform;
    public SG_CustomWaveform indexWaveform;
    public SG_CustomWaveform wristWaveform;

    [Header("Which Hand Does This Object Belong To?")]
    public SG.HandSide connectsTo = SG.HandSide.LeftHand;

    [Header("Optional Graph (3-channel)")]
    public UIBarGraphWithLabels graph;

    [Header("Haptic Output Display")]
    public TextMeshProUGUI a1Text;      // A1 amplitude
    public TextMeshProUGUI a2Text;      // A2 amplitude
    public TextMeshProUGUI a3Text;      // A3 amplitude
    public TextMeshProUGUI freqText;    // Frequency display
    public TextMeshProUGUI accelText;   // Acceleration display
    public Transform pointer;           // pointer needle transform

    [Header("Gauge Settings")]
    public float minRotation = 0f;      // degrees
    public float maxRotation = 180f;    // degrees
    public GaugeAxis rotationAxis = GaugeAxis.Z;

    [Header("Debug Options")]
    public bool logToConsole = true;

    [Header("Logging to Disk")]
    [Tooltip("If true, logs time, |A(t)|, frequency, amplitudes to CSV while object is grabbed.")]
    public bool logToFile = true;

    // --- internal state ---
    private SG_Grabable grabable;

    private Vector3 lastPosition;
    private Vector3 lastVelocity;

    private bool wasGrabbed = false;

    private Quaternion pointerBaseRotation;

    // logging
    private StreamWriter logWriter = null;
    private float logTime = 0f;
    private string logFilePath;

    void Start()
    {
        grabable = GetComponent<SG_Grabable>();
        lastPosition = transform.position;
        lastVelocity = Vector3.zero;

        if (pointer != null)
        {
            pointerBaseRotation = pointer.localRotation;
        }

        if (logToFile)
        {
#if UNITY_EDITOR
            logFilePath = Path.Combine(Application.dataPath, gameObject.name + "_haptics.csv");
#else
            logFilePath = Path.Combine(
                Application.persistentDataPath,
                gameObject.name + "_haptics.csv"
            );
#endif
        }
    }

    void Update()
    {
        if (grabable == null) return;

        bool isGrabbed = grabable.IsGrabbed();

        // Just started grabbing this frame
        if (isGrabbed && !wasGrabbed)
        {
            logTime = 0f;
            OpenLogFileIfNeeded();
        }

        if (isGrabbed)
        {
            StepPhysicsAndHaptics();
        }
        else if (wasGrabbed && !isGrabbed)
        {
            // Just released this frame
            HapticGlove glove = GetGloveForSide(connectsTo);
            if (glove != null)
            {
                StopVibrations(glove);
            }
            CloseLogFile();
        }

        wasGrabbed = isGrabbed;
    }

    void OnDestroy()
    {
        CloseLogFile();
    }

    // -------------------------------------------------------
    // Main physics & haptics update when grabbed
    // -------------------------------------------------------
    private void StepPhysicsAndHaptics()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // --- 1) Velocity & acceleration from transform ---
        Vector3 currentPos = transform.position;
        Vector3 velocity = (currentPos - lastPosition) / dt;
        Vector3 acceleration = (velocity - lastVelocity) / dt;

        lastPosition = currentPos;
        lastVelocity = velocity;

        float accelMag = acceleration.magnitude; // |A(t)|

        // --- 2) Check if acceleration is below minimum threshold ---
        if (accelMag < aMin)
        {
            // No vibration if acceleration too low
            UpdateDisplayUI(0f, 0f, 0f, 0f, accelMag);
            
            HapticGlove gloveStop = GetGloveForSide(connectsTo);
            if (gloveStop != null)
            {
                StopVibrations(gloveStop);
            }
            return;
        }

        // --- 3) Clamp acceleration to aMax (amplitude locked above aMax) ---
        float accelClamped = Mathf.Min(accelMag, aMax);

        // --- 4) Calculate frequency based on clamped acceleration ---
        // f = f_min + ((a(t) - a_min) / (a_max - a_min)) * (f_max - f_min)
        float accelNorm = Mathf.Clamp01((accelClamped - aMin) / (aMax - aMin));
        float frequency = fMin + accelNorm * (fMax - fMin);

        // --- 5) Calculate normalized amplitude directly from acceleration ---
        // Amplitude scales linearly from 0 to 1 as acceleration goes from aMin to aMax
        float aClamped = accelNorm;

        // --- 6) Calculate per-channel amplitudes ---
        float a1 = aClamped * thumbAmplitude;
        float a2 = aClamped * indexAmplitude;
        float a3 = aClamped * wristAmplitude;

        // --- 7) Calculate force for display/logging purposes ---
        float force = mass * accelClamped;

        if (logToConsole)
        {
            Debug.Log(
                $"[GrabVibration] {gameObject.name} |A|={accelMag:F3} m/s², " +
                $"|A|_clamped={accelClamped:F3} m/s², " +
                $"F={force:F3} N, Freq={frequency:F1} Hz, " +
                $"accelNorm={accelNorm:F3}, " +
                $"A1={a1:F3}, A2={a2:F3}, A3={a3:F3}"
            );
        }

        // --- 8) Update UI displays ---
        UpdateDisplayUI(a1, a2, a3, frequency, accelMag);

        // --- 9) Optional graph display ---
        if (graph != null)
        {
            graph.AddData(a1, a2, a3);
        }

        // --- 10) Send vibration to glove ---
        HapticGlove glove = GetGloveForSide(connectsTo);
        if (glove != null && glove.IsConnected())
        {
            SendVibrations(glove, a1, a2, a3);
        }

        // --- 11) Log to file ---
        if (logToFile && logWriter != null)
        {
            logWriter.WriteLine($"{logTime:F5},{accelMag:F5},{frequency:F5},{a1:F5},{a2:F5},{a3:F5}");
        }

        logTime += dt;
    }

    // -------------------------------------------------------
    // Logging helpers
    // -------------------------------------------------------
    private void OpenLogFileIfNeeded()
    {
        if (!logToFile) return;

        try
        {
            logWriter = new StreamWriter(logFilePath, false);
            logWriter.WriteLine("# Object: " + gameObject.name);
            logWriter.WriteLine("# Columns: time(s), |A(t)|(m/s^2), frequency(Hz), A1, A2, A3");
            logWriter.WriteLine("time,acceleration,frequency,A1,A2,A3");
            logWriter.Flush();
            Debug.Log("[GrabVibration] Logging to: " + logFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[GrabVibration] Could not open log file: " + e.Message);
            logWriter = null;
        }
    }

    private void CloseLogFile()
    {
        if (logWriter != null)
        {
            logWriter.Flush();
            logWriter.Close();
            logWriter = null;
        }
    }

    // -------------------------------------------------------
    // UI: displays and gauge
    // -------------------------------------------------------
    private void UpdateDisplayUI(float a1, float a2, float a3, float frequency, float accelMag)
    {
        // Display A1, A2, A3
        if (a1Text != null)
        {
            a1Text.text = $"A1: {a1:F3}";
        }

        if (a2Text != null)
        {
            a2Text.text = $"A2: {a2:F3}";
        }

        if (a3Text != null)
        {
            a3Text.text = $"A3: {a3:F3}";
        }

        if (freqText != null)
        {
            freqText.text = $"f: {frequency:F1} Hz";
        }

        if (accelText != null)
        {
            accelText.text = $"|A|: {accelMag:F3} m/s²";
        }

        // Pointer needle based on clamped amplitude average
        if (pointer != null)
        {
            float avgAmplitude = (a1 + a2 + a3) / 3f;
            float t = Mathf.Clamp01(avgAmplitude);
            float angle = Mathf.Lerp(minRotation, maxRotation, t);

            Vector3 euler = pointerBaseRotation.eulerAngles;
            switch (rotationAxis)
            {
                case GaugeAxis.X:
                    euler.x = angle;
                    break;
                case GaugeAxis.Y:
                    euler.y = angle;
                    break;
                case GaugeAxis.Z:
                    euler.z = angle;
                    break;
            }
            pointer.localRotation = Quaternion.Euler(euler);
        }
    }

    // -------------------------------------------------------
    // Haptic glove helpers
    // -------------------------------------------------------
    private HapticGlove GetGloveForSide(SG.HandSide side)
    {
        HapticGlove[] allGloves = HapticGlove.GetHapticGloves(true);
        foreach (var glove in allGloves)
        {
            bool isRight = glove.IsRight();
            if (side == SG.HandSide.LeftHand && !isRight) return glove;
            if (side == SG.HandSide.RightHand && isRight) return glove;
        }
        return null;
    }

    private void SendVibrations(HapticGlove glove, float a1, float a2, float a3)
    {
        // A1 for thumb
        if (thumbWaveform != null)
        {
            var wfThumb = thumbWaveform.GetWaveform();
            wfThumb.Amplitude = Mathf.Clamp01(a1);
            SG_CustomWaveform.CallCorrectWaveform(glove, wfThumb, VibrationLocation.Thumb_Tip);
        }

        // A2 for index
        if (indexWaveform != null)
        {
            var wfIndex = indexWaveform.GetWaveform();
            wfIndex.Amplitude = Mathf.Clamp01(a2);
            SG_CustomWaveform.CallCorrectWaveform(glove, wfIndex, VibrationLocation.Index_Tip);
        }

        // A3 for wrist
        if (wristWaveform != null)
        {
            var wfWrist = wristWaveform.GetWaveform();
            wfWrist.Amplitude = Mathf.Clamp01(a3);

            if (glove is NovaGlove)
                SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.WholeHand);
            else
                SG_CustomWaveform.CallCorrectWaveform(glove, wfWrist, VibrationLocation.Palm_IndexSide);
        }
    }

    private void StopVibrations(HapticGlove glove)
    {
        var wfZero = new SGCore.CustomWaveform();
        wfZero.Amplitude = 0f;

        SG_CustomWaveform.CallCorrectWaveform(glove, wfZero, VibrationLocation.Thumb_Tip);
        SG_CustomWaveform.CallCorrectWaveform(glove, wfZero, VibrationLocation.Index_Tip);

        if (glove is NovaGlove)
            SG_CustomWaveform.CallCorrectWaveform(glove, wfZero, VibrationLocation.WholeHand);
        else
            SG_CustomWaveform.CallCorrectWaveform(glove, wfZero, VibrationLocation.Palm_IndexSide);
    }
}