using UnityEngine;
using UnityEditor;

public class VerifyStudySetup : EditorWindow
{
    [MenuItem("Tools/Verify User Study Setup")]
    public static void Verify()
    {
        int issues = 0;
        int warnings = 0;
        
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("VERIFYING USER STUDY SETUP...");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        // Check StudyManager
        StudyManager studyManager = FindObjectOfType<StudyManager>();
        if (studyManager == null)
        {
            Debug.LogError("✗ StudyManager not found!");
            issues++;
        }
        else
        {
            Debug.Log("✓ StudyManager found");
            
            if (studyManager.instructionsPanel == null) { Debug.LogWarning("  ⚠ instructionsPanel not assigned"); warnings++; }
            if (studyManager.trainingPanel == null) { Debug.LogWarning("  ⚠ trainingPanel not assigned"); warnings++; }
            if (studyManager.taskPanel == null) { Debug.LogWarning("  ⚠ taskPanel not assigned"); warnings++; }
            if (studyManager.completionPanel == null) { Debug.LogWarning("  ⚠ completionPanel not assigned"); warnings++; }
            if (studyManager.timerText == null) { Debug.LogWarning("  ⚠ timerText not assigned"); warnings++; }
            if (studyManager.completionTimeText == null) { Debug.LogWarning("  ⚠ completionTimeText not assigned"); warnings++; }
            if (studyManager.placementValidator == null) { Debug.LogWarning("  ⚠ placementValidator not assigned - Run 'Complete User Study Setup'"); warnings++; }
            if (studyManager.trainingArea == null) { Debug.LogWarning("  ⚠ trainingArea not assigned"); warnings++; }
        }
        
        // Check PlacementValidator
        PlacementValidator validator = FindObjectOfType<PlacementValidator>();
        if (validator == null)
        {
            Debug.LogError("✗ PlacementValidator not found!");
            issues++;
        }
        else
        {
            Debug.Log("✓ PlacementValidator found");
            
            if (validator.placeholders == null || validator.placeholders.Length != 8)
            {
                Debug.LogWarning("  ⚠ Should have 8 placeholders");
                warnings++;
            }
            
            if (validator.studyManager == null)
            {
                Debug.LogWarning("  ⚠ studyManager not assigned - Run 'Complete User Study Setup'");
                warnings++;
            }
        }
        
        // Check UI Canvas
        GameObject canvas = GameObject.Find("StudyUI");
        if (canvas == null)
        {
            Debug.LogError("✗ StudyUI Canvas not found!");
            issues++;
        }
        else
        {
            Debug.Log("✓ StudyUI Canvas found");
        }
        
        // Check Training Area
        GameObject trainingArea = GameObject.Find("TrainingArea");
        if (trainingArea == null)
        {
            Debug.LogError("✗ TrainingArea not found!");
            issues++;
        }
        else
        {
            Debug.Log("✓ TrainingArea found");
            
            TrainingObject[] trainingObjects = trainingArea.GetComponentsInChildren<TrainingObject>();
            if (trainingObjects.Length != 3)
            {
                Debug.LogWarning($"  ⚠ Expected 3 training objects, found {trainingObjects.Length}");
                warnings++;
            }
        }
        
        // Check for copied scene elements
        GameObject solarPanel = GameObject.Find("SolarPanel");
        if (solarPanel == null)
        {
            Debug.LogWarning("⚠ SolarPanel not found - Need to copy from NewScene.unity");
            warnings++;
        }
        else
        {
            Debug.Log("✓ SolarPanel found");
        }
        
        GameObject cameraRig = GameObject.Find("[CameraRig]");
        if (cameraRig == null)
        {
            Debug.LogWarning("⚠ [CameraRig] not found - Need to copy from NewScene.unity");
            warnings++;
        }
        else
        {
            Debug.Log("✓ [CameraRig] found");
        }
        
        GameObject eventSystem = GameObject.Find("EventSystem");
        if (eventSystem == null)
        {
            Debug.LogWarning("⚠ EventSystem not found - Need to copy from NewScene.unity");
            warnings++;
        }
        else
        {
            Debug.Log("✓ EventSystem found");
        }
        
        // Check training sphere GrabVibration
        if (trainingArea != null)
        {
            GameObject[] spheres = { 
                GameObject.Find("TrainingArea/LightSphere"),
                GameObject.Find("TrainingArea/MediumSphere"),
                GameObject.Find("TrainingArea/HeavySphere")
            };
            
            foreach (GameObject sphere in spheres)
            {
                if (sphere != null)
                {
                    GrabVibration gv = sphere.GetComponent<GrabVibration>();
                    if (gv == null)
                    {
                        Debug.LogWarning($"  ⚠ {sphere.name} missing GrabVibration component");
                        warnings++;
                    }
                }
            }
        }
        
        // Summary
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        if (issues == 0 && warnings == 0)
        {
            Debug.Log("✓✓✓ SETUP COMPLETE! All systems ready!");
            EditorUtility.DisplayDialog(
                "Setup Verification",
                "✓ PERFECT! All systems ready!\n\nYour user study is fully configured and ready to test!",
                "Awesome!"
            );
        }
        else if (issues == 0)
        {
            Debug.Log($"✓ Core setup complete! {warnings} warnings (see above)");
            EditorUtility.DisplayDialog(
                "Setup Verification",
                $"Core setup is good!\n\n{warnings} warnings found.\n\nCheck console for details.",
                "OK"
            );
        }
        else
        {
            Debug.LogError($"✗ {issues} critical issues and {warnings} warnings found!");
            EditorUtility.DisplayDialog(
                "Setup Verification",
                $"{issues} critical issues found!\n\nCheck console for details.",
                "OK"
            );
        }
        
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }
}
