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
            if (studyManager.taskPanel == null) { Debug.LogWarning("  ⚠ taskPanel not assigned"); warnings++; }
            if (studyManager.classificationPanel == null) { Debug.LogWarning("  ⚠ classificationPanel not assigned"); warnings++; }
            if (studyManager.completionPanel == null) { Debug.LogWarning("  ⚠ completionPanel not assigned"); warnings++; }
            if (studyManager.solarPanel == null) { Debug.LogWarning("  ⚠ solarPanel not assigned"); warnings++; }
            if (studyManager.heavierBucket == null) { Debug.LogWarning("  ⚠ heavierBucket not assigned"); warnings++; }
            if (studyManager.lighterBucket == null) { Debug.LogWarning("  ⚠ lighterBucket not assigned"); warnings++; }
            if (studyManager.rockyBox == null) { Debug.LogWarning("  ⚠ rockyBox not assigned"); warnings++; }
            if (studyManager.gaseousBox == null) { Debug.LogWarning("  ⚠ gaseousBox not assigned"); warnings++; }
            if (studyManager.timerText == null) { Debug.LogWarning("  ⚠ timerText not assigned"); warnings++; }
            if (studyManager.completionTimeText == null) { Debug.LogWarning("  ⚠ completionTimeText not assigned"); warnings++; }
            if (studyManager.comparisonManager == null) { Debug.LogWarning("  ⚠ comparisonManager not assigned"); warnings++; }
            if (studyManager.classificationManager == null) { Debug.LogWarning("  ⚠ classificationManager not assigned"); warnings++; }
        }
        
        // Check PairwiseComparisonManager
        PairwiseComparisonManager comparisonMgr = FindObjectOfType<PairwiseComparisonManager>();
        if (comparisonMgr == null)
        {
            Debug.LogWarning("⚠ PairwiseComparisonManager not found!");
            warnings++;
        }
        else
        {
            Debug.Log("✓ PairwiseComparisonManager found");
            
            if (comparisonMgr.heavierBucket == null)
            {
                Debug.LogWarning("  ⚠ heavierBucket not assigned");
                warnings++;
            }
            
            if (comparisonMgr.lighterBucket == null)
            {
                Debug.LogWarning("  ⚠ lighterBucket not assigned");
                warnings++;
            }
        }
        
        // Check PlacementValidator (legacy - optional)
        PlacementValidator validator = FindObjectOfType<PlacementValidator>();
        if (validator == null)
        {
            Debug.Log("ℹ PlacementValidator not found (using new pairwise comparison system)");
        }
        else
        {
            Debug.Log("✓ PlacementValidator found (legacy system)");
            
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
        
        // Check for classification boxes
        GameObject rockyBox = GameObject.Find("RockyBox");
        if (rockyBox == null)
        {
            Debug.LogWarning("⚠ RockyBox not found");
            warnings++;
        }
        else
        {
            Debug.Log("✓ RockyBox found");
        }
        
        GameObject gaseousBox = GameObject.Find("GaseousBox");
        if (gaseousBox == null)
        {
            Debug.LogWarning("⚠ GaseousBox not found");
            warnings++;
        }
        else
        {
            Debug.Log("✓ GaseousBox found");
        }
        
        // Check for copied scene elements
        GameObject solarPanel = GameObject.Find("SolarPanel");
        if (solarPanel == null)
        {
            Debug.LogWarning("⚠ SolarPanel not found");
            warnings++;
        }
        else
        {
            Debug.Log("✓ SolarPanel found");
        }
        
        GameObject cameraRig = GameObject.Find("[CameraRig]");
        if (cameraRig == null)
        {
            Debug.LogWarning("⚠ [CameraRig] not found");
            warnings++;
        }
        else
        {
            Debug.Log("✓ [CameraRig] found");
        }
        
        GameObject eventSystem = GameObject.Find("EventSystem");
        if (eventSystem == null)
        {
            Debug.LogWarning("⚠ EventSystem not found");
            warnings++;
        }
        else
        {
            Debug.Log("✓ EventSystem found");
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
