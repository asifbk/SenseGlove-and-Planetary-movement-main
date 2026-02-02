using UnityEngine;

public class ComparisonBucket : MonoBehaviour
{
    [Header("Bucket Type")]
    public bool isHeavierBucket = true;
    
    private PairwiseComparisonManager comparisonManager;
    
    void Start()
    {
        comparisonManager = FindObjectOfType<PairwiseComparisonManager>();
        
        if (comparisonManager == null)
        {
            Debug.LogError($"[ComparisonBucket] No PairwiseComparisonManager found in scene!");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (comparisonManager == null) return;
        
        if (other.CompareTag("Planet"))
        {
            comparisonManager.OnPlanetEnteredBucket(other.gameObject.name, isHeavierBucket);
            Debug.Log($"[ComparisonBucket] {other.name} entered {(isHeavierBucket ? "HEAVIER" : "LIGHTER")} bucket");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (comparisonManager == null) return;
        
        if (other.CompareTag("Planet"))
        {
            comparisonManager.OnPlanetExitedBucket(other.gameObject.name, isHeavierBucket);
            Debug.Log($"[ComparisonBucket] {other.name} exited {(isHeavierBucket ? "HEAVIER" : "LIGHTER")} bucket");
        }
    }
}
