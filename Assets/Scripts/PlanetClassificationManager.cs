using UnityEngine;
using TMPro;

public class PlanetClassificationManager : MonoBehaviour
{
    [Header("Classification Boxes")]
    public PlanetClassificationBox rockyBox;
    public PlanetClassificationBox gaseousBox;
    
    [Header("UI References")]
    public GameObject classificationPanel;
    public TextMeshProUGUI instructionText;
    
    [Header("Expected Counts")]
    public int expectedRockyCount = 3;
    public int expectedGaseousCount = 5;
    
    private bool classificationComplete = false;
    private StudyManager studyManager;
    
    void Start()
    {
        studyManager = FindObjectOfType<StudyManager>();
        
        if (classificationPanel != null)
            classificationPanel.SetActive(false);
    }
    
    void Update()
    {
        if (classificationPanel != null && classificationPanel.activeSelf)
        {
            CheckClassificationCompletion();
        }
    }
    
    public void StartClassificationPhase()
    {
        if (classificationPanel != null)
        {
            classificationPanel.SetActive(true);
        }
        
        classificationComplete = false;
        
        Debug.Log("[PlanetClassificationManager] Classification phase started");
        
        if (instructionText != null)
        {
            instructionText.text = "Touch each planet to feel its surface.\nPlace Rocky planets in the ROCKY box.\nPlace Gaseous planets in the GASEOUS box.";
        }
    }
    
    void CheckClassificationCompletion()
    {
        if (classificationComplete)
            return;
        
        if (rockyBox == null || gaseousBox == null)
            return;
        
        int rockyCount = rockyBox.GetPlanetCount();
        int gaseousCount = gaseousBox.GetPlanetCount();
        
        if (rockyCount + gaseousCount != expectedRockyCount + expectedGaseousCount)
            return;
        
        bool rockyCorrect = rockyBox.HasCorrectPlanets() && rockyCount == expectedRockyCount;
        bool gaseousCorrect = gaseousBox.HasCorrectPlanets() && gaseousCount == expectedGaseousCount;
        
        if (rockyCorrect && gaseousCorrect)
        {
            classificationComplete = true;
            OnClassificationComplete();
        }
    }
    
    void OnClassificationComplete()
    {
        Debug.Log("[PlanetClassificationManager] Classification complete!");
        
        if (instructionText != null)
        {
            instructionText.text = "Classification Complete!\nAll planets correctly sorted!";
        }
        
        Invoke(nameof(CompleteStudy), 2f);
    }
    
    void CompleteStudy()
    {
        if (studyManager != null)
        {
            studyManager.CompleteStudy();
        }
    }
    
    public bool IsClassificationComplete()
    {
        return classificationComplete;
    }
}
