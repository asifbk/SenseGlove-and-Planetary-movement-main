using UnityEngine;
using UnityEditor;

public class CompleteStudySetup : EditorWindow
{
    [MenuItem("Tools/Complete User Study Setup")]
    public static void Execute()
    {
        // Find the StudyManager
        StudyManager studyManager = FindObjectOfType<StudyManager>();
        if (studyManager == null)
        {
            Debug.LogError("[Setup] Could not find StudyManager in scene!");
            return;
        }

        // Find PairwiseComparisonManager
        PairwiseComparisonManager comparisonManager = FindObjectOfType<PairwiseComparisonManager>();
        if (comparisonManager == null)
        {
            Debug.LogWarning("[Setup] PairwiseComparisonManager not found in scene - will need to be assigned manually");
        }
        else
        {
            // Link StudyManager to PairwiseComparisonManager
            studyManager.comparisonManager = comparisonManager;
            EditorUtility.SetDirty(studyManager);
            Debug.Log("[Setup] ✓ StudyManager linked to PairwiseComparisonManager!");
        }

        Debug.Log("[Setup] ✓ All systems ready!");
        Debug.Log("[Setup] Next: Create HeavierBucket and LighterBucket GameObjects in scene");
    }
}
