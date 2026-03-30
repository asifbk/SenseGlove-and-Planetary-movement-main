using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

[System.Serializable]
public class PlacementRecord
{
    public string timestamp;
    public int rank;
    public string placedPlanetName;
    public float placedPlanetMass;
    public float expectedMass;
    public bool isCorrect;
}

public class PlacementValidator : MonoBehaviour
{
    [Header("Placeholders")]
    public PlanetPlaceholder[] placeholders;
    
    [Header("Logging")]
    public bool logToFile = true;
    public string logFileName = "UserStudy_PlanetPlacement";
    
    [Header("Study Manager")]
    public StudyManager studyManager;
    
    private List<PlacementRecord> placementHistory = new List<PlacementRecord>();
    private StreamWriter logWriter = null;
    private string logFilePath;
    private int attemptCount = 0;
    private bool taskCompleted = false;
    
    void Start()
    {
        if (logToFile)
        {
#if UNITY_EDITOR
            logFilePath = Path.Combine(Application.dataPath, logFileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
#else
            logFilePath = Path.Combine(Application.persistentDataPath, logFileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
#endif
            OpenLogFile();
        }
    }
    
    void OnDestroy()
    {
        CloseLogFile();
    }
    
    private void OpenLogFile()
    {
        try
        {
            logWriter = new StreamWriter(logFilePath, false);
            logWriter.WriteLine("# User Study - Planet Placement Based on Vibrotactile Feedback");
            logWriter.WriteLine("# Correct Order (Rank 1-8): Jupiter, Saturn, Uranus, Neptune, Earth, Venus, Mars, Mercury");
            logWriter.WriteLine("Timestamp,ElapsedTime,AttemptNumber,Rank,PlacedPlanetName,PlacedMass,ExpectedMass,IsCorrect");
            logWriter.Flush();
            Debug.Log("[PlacementValidator] Logging to: " + logFilePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[PlacementValidator] Could not open log file: " + e.Message);
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
    
    public void OnPlanetPlaced(PlanetPlaceholder placeholder, GameObject planet)
    {
        GrabVibration grabVibration = planet.GetComponent<GrabVibration>();
        if (grabVibration == null) return;
        
        attemptCount++;
        float planetMass = grabVibration.mass;
        bool isCorrect = Mathf.Approximately(planetMass, placeholder.expectedMass);
        
        PlacementRecord record = new PlacementRecord
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            rank = placeholder.rank,
            placedPlanetName = planet.name,
            placedPlanetMass = planetMass,
            expectedMass = placeholder.expectedMass,
            isCorrect = isCorrect
        };
        
        placementHistory.Add(record);
        
        if (logWriter != null)
        {
            float elapsedTime = studyManager != null ? studyManager.GetElapsedTime() : 0f;
            logWriter.WriteLine($"{record.timestamp},{elapsedTime:F2},{attemptCount},{record.rank},{record.placedPlanetName},{record.placedPlanetMass},{record.expectedMass},{record.isCorrect}");
            logWriter.Flush();
        }
        
        Debug.Log($"[PlacementValidator] Attempt {attemptCount} - Rank {placeholder.rank}: {planet.name} (mass={planetMass}) - {(isCorrect ? "CORRECT" : "INCORRECT")}");
        
        ValidateAllPlacements();
    }
    
    public void OnPlanetRemoved(PlanetPlaceholder placeholder)
    {
        ValidateAllPlacements();
    }
    
    private void ValidateAllPlacements()
    {
        bool allCorrect = true;
        int placedCount = 0;
        
        foreach (PlanetPlaceholder placeholder in placeholders)
        {
            if (placeholder.isOccupied && placeholder.placedPlanet != null)
            {
                placedCount++;
                GrabVibration grabVibration = placeholder.placedPlanet.GetComponent<GrabVibration>();
                if (grabVibration != null)
                {
                    bool isCorrect = Mathf.Approximately(grabVibration.mass, placeholder.expectedMass);
                    placeholder.SetCorrectVisual(isCorrect);
                    
                    if (!isCorrect)
                    {
                        allCorrect = false;
                    }
                }
            }
            else
            {
                placeholder.SetCorrectVisual(false);
                allCorrect = false;
            }
        }
        
        if (allCorrect && placedCount == placeholders.Length && !taskCompleted)
        {
            taskCompleted = true;
            float completionTime = studyManager != null ? studyManager.GetElapsedTime() : 0f;
            
            Debug.Log($"[PlacementValidator] SUCCESS! All planets correctly placed in {completionTime:F2} seconds with {attemptCount} total attempts!");
            
            if (logWriter != null)
            {
                logWriter.WriteLine($"# SUCCESS at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                logWriter.WriteLine($"# Completion Time: {completionTime:F2} seconds");
                logWriter.WriteLine($"# Total Attempts: {attemptCount}");
                logWriter.Flush();
            }
            
            if (studyManager != null)
            {
                studyManager.OnTaskCompleted();
            }
        }
    }
    
    public void OnTaskStarted()
    {
        attemptCount = 0;
        taskCompleted = false;
        
        if (logWriter != null)
        {
            logWriter.WriteLine($"# TASK STARTED at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            logWriter.Flush();
        }
    }
    
    public void ResetPlacements()
    {
        attemptCount = 0;
        taskCompleted = false;
        placementHistory.Clear();
        
        foreach (PlanetPlaceholder placeholder in placeholders)
        {
            if (placeholder != null)
            {
                placeholder.ClearPlacement();
            }
        }
    }
}
