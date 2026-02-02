using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public class StudyManager : MonoBehaviour
{
    [Header("Study Phases")]
    public StudyPhase currentPhase = StudyPhase.Instructions;
    
    [Header("UI References")]
    public GameObject instructionsPanel;
    public GameObject taskPanel;
    public GameObject classificationPanel;
    public GameObject completionPanel;
    
    [Header("3D Scene Objects")]
    public GameObject solarPanel;
    public GameObject heavierBucket;
    public GameObject lighterBucket;
    public GameObject rockyBox;
    public GameObject gaseousBox;
    
    [Header("Timer Display")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI completionTimeText;
    
    [Header("Validation")]
    public PairwiseComparisonManager comparisonManager;
    public PlanetClassificationManager classificationManager;
    
    private float taskStartTime;
    private float taskEndTime;
    private float elapsedTime;
    private bool timerRunning = false;
    
    private Vector3 solarPanelOriginalPosition;
    private Quaternion solarPanelOriginalRotation;
    private Vector3 solarPanelOriginalScale;
    private bool solarPanelTransformSaved = false;
    
    private Dictionary<Transform, TransformData> planetOriginalTransforms = new Dictionary<Transform, TransformData>();
    
    [System.Serializable]
    private class TransformData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        
        public TransformData(Transform transform)
        {
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
            localScale = transform.localScale;
        }
        
        public void ApplyTo(Transform transform)
        {
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            transform.localScale = localScale;
        }
    }
    
    public enum StudyPhase
    {
        Instructions,
        Task,
        Classification,
        Completed
    }
    
    void Start()
    {
        SaveSolarPanelOriginalTransform();
        SaveAllPlanetTransforms();
        ShowPhase(StudyPhase.Instructions);
    }
    
    private void SaveSolarPanelOriginalTransform()
    {
        if (solarPanel != null && !solarPanelTransformSaved)
        {
            Transform solarTransform = solarPanel.transform;
            solarPanelOriginalPosition = solarTransform.localPosition;
            solarPanelOriginalRotation = solarTransform.localRotation;
            solarPanelOriginalScale = solarTransform.localScale;
            solarPanelTransformSaved = true;
            Debug.Log("[StudyManager] Solar Panel original transform saved");
        }
    }
    
    private void SaveAllPlanetTransforms()
    {
        if (solarPanel == null) return;
        
        planetOriginalTransforms.Clear();
        
        foreach (Transform child in solarPanel.transform)
        {
            if (child.CompareTag("Planet"))
            {
                planetOriginalTransforms[child] = new TransformData(child);
                Debug.Log($"[StudyManager] Saved original transform for planet: {child.name}");
            }
        }
        
        Debug.Log($"[StudyManager] Saved {planetOriginalTransforms.Count} planet transforms");
    }
    
    private void ResetSolarPanelTransform()
    {
        if (solarPanel != null && solarPanelTransformSaved)
        {
            Transform solarTransform = solarPanel.transform;
            solarTransform.localPosition = solarPanelOriginalPosition;
            solarTransform.localRotation = solarPanelOriginalRotation;
            solarTransform.localScale = solarPanelOriginalScale;
            Debug.Log("[StudyManager] Solar Panel transform reset to original state");
        }
    }
    
    private void ResetAllPlanetTransforms()
    {
        if (solarPanel == null) return;
        
        int resetCount = 0;
        foreach (var kvp in planetOriginalTransforms)
        {
            Transform planet = kvp.Key;
            TransformData originalData = kvp.Value;
            
            if (planet != null)
            {
                originalData.ApplyTo(planet);
                
                Rigidbody rb = planet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                
                resetCount++;
            }
        }
        
        Debug.Log($"[StudyManager] Reset {resetCount} planets to original positions");
    }
    
    void Update()
    {
        if (timerRunning)
        {
            elapsedTime = Time.time - taskStartTime;
            UpdateTimerDisplay();
        }
    }
    
    public void StartTaskPhase()
    {
        ShowPhase(StudyPhase.Task);
        
        taskStartTime = Time.time;
        timerRunning = true;
        
        if (comparisonManager != null)
        {
            comparisonManager.OnTaskStarted();
        }
        
        Debug.Log("[StudyManager] Task phase started - compare planetary masses using both hands!");
    }
    
    public void OnComparisonTaskCompleted()
    {
        if (currentPhase != StudyPhase.Task) return;
        
        timerRunning = false;
        taskEndTime = Time.time;
        elapsedTime = taskEndTime - taskStartTime;
        
        StartClassificationPhase();
        
        Debug.Log($"[StudyManager] Comparison task completed in {FormatTime(elapsedTime)} - starting classification phase");
    }
    
    public void StartClassificationPhase()
    {
        ResetSolarPanelTransform();
        ResetAllPlanetTransforms();
        ShowPhase(StudyPhase.Classification);
        
        ResetClassificationBoxColors();
        
        if (classificationManager != null)
        {
            classificationManager.StartClassificationPhase();
        }
        
        Debug.Log("[StudyManager] Classification phase started - sort planets by type");
    }
    
    private void ResetClassificationBoxColors()
    {
        if (rockyBox != null)
        {
            PlanetClassificationBox rockyBoxComponent = rockyBox.GetComponent<PlanetClassificationBox>();
            if (rockyBoxComponent != null)
            {
                rockyBoxComponent.ResetBoxColor();
            }
        }
        
        if (gaseousBox != null)
        {
            PlanetClassificationBox gaseousBoxComponent = gaseousBox.GetComponent<PlanetClassificationBox>();
            if (gaseousBoxComponent != null)
            {
                gaseousBoxComponent.ResetBoxColor();
            }
        }
        
        Debug.Log("[StudyManager] Classification boxes reset to normal color");
    }
    
    public void CompleteStudy()
    {
        ShowPhase(StudyPhase.Completed);
        
        if (completionTimeText != null)
        {
            completionTimeText.text = $"Rank Task Time: {FormatTime(elapsedTime)}\nAll Tasks Completed!";
        }
        
        Debug.Log($"[StudyManager] Study completed!");
    }
    
    public void ResetStudy()
    {
        elapsedTime = 0f;
        timerRunning = false;
        
        ShowPhase(StudyPhase.Instructions);
        
        Debug.Log("[StudyManager] Study reset to initial state");
    }
    
    private void ShowPhase(StudyPhase phase)
    {
        currentPhase = phase;
        
        // UI Panels
        if (instructionsPanel != null)
            instructionsPanel.SetActive(phase == StudyPhase.Instructions);
        
        if (taskPanel != null)
            taskPanel.SetActive(phase == StudyPhase.Task);
        
        if (classificationPanel != null)
            classificationPanel.SetActive(phase == StudyPhase.Classification);
        
        if (completionPanel != null)
            completionPanel.SetActive(phase == StudyPhase.Completed);
        
        // 3D Scene Objects
        if (solarPanel != null)
            solarPanel.SetActive(phase == StudyPhase.Task || phase == StudyPhase.Classification);
        
        if (heavierBucket != null)
            heavierBucket.SetActive(phase == StudyPhase.Task);
        
        if (lighterBucket != null)
            lighterBucket.SetActive(phase == StudyPhase.Task);
        
        if (rockyBox != null)
            rockyBox.SetActive(phase == StudyPhase.Classification);
        
        if (gaseousBox != null)
            gaseousBox.SetActive(phase == StudyPhase.Classification);
        
        // Timer visibility
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
