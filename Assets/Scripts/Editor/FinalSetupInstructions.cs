using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FinalSetupInstructions : EditorWindow
{
    [MenuItem("Tools/Show User Study Final Setup Instructions")]
    public static void ShowInstructions()
    {
        EditorUtility.DisplayDialog(
            "User Study - Final Setup Steps",
            "AUTOMATED SETUP COMPLETE! ✓\n\n" +
            "Created:\n" +
            "• UI Canvas with 4 panels (Instructions, Training, Task, Completion)\n" +
            "• Training Area with 3 reference spheres\n" +
            "• StudyManager with all UI wired up\n" +
            "• All button callbacks configured\n\n" +
            "MANUAL STEPS REMAINING:\n\n" +
            "1. Run 'Tools > Complete User Study Setup' to link final references\n\n" +
            "2. Open 'NewScene.unity'\n\n" +
            "3. Copy these GameObjects (Ctrl+C):\n" +
            "   - SolarPanel (contains all 8 planets)\n" +
            "   - [CameraRig] (VR setup)\n" +
            "   - EventSystem (for UI)\n" +
            "   - Directional Light (optional)\n\n" +
            "4. Open 'UserStudy.unity'\n\n" +
            "5. Paste (Ctrl+V)\n\n" +
            "6. Adjust positions:\n" +
            "   - SolarPanel: Y=10 (above placeholders)\n" +
            "   - [CameraRig]: (0,0,0)\n\n" +
            "7. Add GrabVibration to training spheres:\n" +
            "   - Copy GrabVibration component from any planet\n" +
            "   - Paste to LightSphere, MediumSphere, HeavySphere\n\n" +
            "8. TEST in Play Mode!\n\n" +
            "Ready to go!",
            "OK"
        );
    }

    [MenuItem("Tools/Open NewScene for Copying")]
    public static void OpenNewScene()
    {
        EditorSceneManager.OpenScene("Assets/NewScene.unity");
        Debug.Log("[Setup] NewScene.unity opened. Select and copy: SolarPanel, [CameraRig], EventSystem");
    }

    [MenuItem("Tools/Open UserStudy Scene")]
    public static void OpenUserStudyScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/UserStudy.unity");
        Debug.Log("[Setup] UserStudy.unity opened. Paste the copied GameObjects here!");
    }
}
