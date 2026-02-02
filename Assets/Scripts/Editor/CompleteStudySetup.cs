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

        // Find PlacementValidator
        PlacementValidator validator = FindObjectOfType<PlacementValidator>();
        if (validator == null)
        {
            Debug.LogError("[Setup] Could not find PlacementValidator in scene!");
            return;
        }

        // Link StudyManager to PlacementValidator
        studyManager.placementValidator = validator;
        EditorUtility.SetDirty(studyManager);

        // Link PlacementValidator to StudyManager
        validator.studyManager = studyManager;
        EditorUtility.SetDirty(validator);

        Debug.Log("[Setup] ✓ StudyManager and PlacementValidator cross-references complete!");
        Debug.Log("[Setup] ✓ All systems ready!");
        Debug.Log("[Setup] Next: Copy SolarPanel and [CameraRig] from NewScene.unity to UserStudy.unity");
    }
}
