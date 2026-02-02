using UnityEngine;
using System;
using TMPro;

public class StudyManager : MonoBehaviour
{
    [Header("Study Phases")]
    public StudyPhase currentPhase = StudyPhase.Instructions;
    
    [Header("UI References")]
    public GameObject instructionsPanel;
    public GameObject trainingPanel;
    public GameObject taskPanel;
    public GameObject completionPanel;
    
    [Header("Timer Display")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI completionTimeText;
    
    [Header("Validation")]
    public PlacementValidator placementValidator;
    
    [Header("Training References")]
    public GameObject trainingArea;
    public GameObject[] trainingObjects;
    
    private float taskStartTime;
    private float taskEndTime;
    private float elapsedTime;
    private bool timerRunning = false;
    
    public enum StudyPhase
    {
        Instructions,
        Training,
        Task,
        Completed
    }
    
    void Start()
    {
        ShowPhase(StudyPhase.Instructions);
    }
    
    void Update()
    {
        if (timerRunning)
        {
            elapsedTime = Time.time - taskStartTime;
            UpdateTimerDisplay();
        }
    }
    
    public void StartTrainingPhase()
    {
        ShowPhase(StudyPhase.Training);
        
        if (trainingArea != null)
        {
            trainingArea.SetActive(true);
        }
        
        Debug.Log("[StudyManager] Training phase started - users can now practice with reference objects");
    }
    
    public void StartTaskPhase()
    {
        ShowPhase(StudyPhase.Task);
        
        if (trainingArea != null)
        {
            trainingArea.SetActive(false);
        }
        
        taskStartTime = Time.time;
        timerRunning = true;
        
        if (placementValidator != null)
        {
            placementValidator.OnTaskStarted();
        }
        
        Debug.Log("[StudyManager] Task phase started - timer running");
    }
    
    public void OnTaskCompleted()
    {
        if (currentPhase != StudyPhase.Task) return;
        
        timerRunning = false;
        taskEndTime = Time.time;
        elapsedTime = taskEndTime - taskStartTime;
        
        ShowPhase(StudyPhase.Completed);
        
        if (completionTimeText != null)
        {
            completionTimeText.text = $"Completion Time: {FormatTime(elapsedTime)}";
        }
        
        Debug.Log($"[StudyManager] Task completed in {FormatTime(elapsedTime)}");
    }
    
    public void ResetStudy()
    {
        elapsedTime = 0f;
        timerRunning = false;
        
        if (placementValidator != null)
        {
            placementValidator.ResetPlacements();
        }
        
        ShowPhase(StudyPhase.Instructions);
        
        Debug.Log("[StudyManager] Study reset to initial state");
    }
    
    private void ShowPhase(StudyPhase phase)
    {
        currentPhase = phase;
        
        if (instructionsPanel != null)
            instructionsPanel.SetActive(phase == StudyPhase.Instructions);
        
        if (trainingPanel != null)
            trainingPanel.SetActive(phase == StudyPhase.Training);
        
        if (taskPanel != null)
            taskPanel.SetActive(phase == StudyPhase.Task);
        
        if (completionPanel != null)
            completionPanel.SetActive(phase == StudyPhase.Completed);
        
        if (timerText != null)
            timerText.gameObject.SetActive(phase == StudyPhase.Task);
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {FormatTime(elapsedTime)}";
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
    
    public float GetElapsedTime()
    {
        return elapsedTime;
    }
    
    public bool IsTaskRunning()
    {
        return currentPhase == StudyPhase.Task && timerRunning;
    }
}
