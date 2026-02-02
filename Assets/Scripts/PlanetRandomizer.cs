using UnityEngine;

public class PlanetRandomizer : MonoBehaviour
{
    [Header("Randomization Area")]
    [Tooltip("Center point of the randomization area")]
    public Vector3 areaCenter = new Vector3(0, 10, 0);
    
    [Tooltip("Size of the randomization area")]
    public Vector3 areaSize = new Vector3(10, 2, 3);
    
    [Header("Planets")]
    [Tooltip("Parent GameObject containing all planets")]
    public Transform planetsParent;
    
    [Header("Settings")]
    [Tooltip("Randomize positions on scene start")]
    public bool randomizeOnStart = true;
    
    [Tooltip("Minimum distance between planets")]
    public float minDistanceBetweenPlanets = 1.5f;
    
    void Start()
    {
        if (randomizeOnStart)
        {
            RandomizePlanetPositions();
        }
    }
    
    public void RandomizePlanetPositions()
    {
        if (planetsParent == null)
        {
            Debug.LogError("[PlanetRandomizer] Planets parent is not assigned!");
            return;
        }
        
        int childCount = planetsParent.childCount;
        if (childCount == 0)
        {
            Debug.LogWarning("[PlanetRandomizer] No planets found under parent!");
            return;
        }
        
        Vector3[] positions = new Vector3[childCount];
        
        for (int i = 0; i < childCount; i++)
        {
            Transform planet = planetsParent.GetChild(i);
            Vector3 newPosition = GetRandomPositionWithMinDistance(positions, i);
            
            planet.position = newPosition;
            positions[i] = newPosition;
            
            Rigidbody rb = planet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        
        Debug.Log($"[PlanetRandomizer] Randomized {childCount} planets in area centered at {areaCenter}");
    }
    
    private Vector3 GetRandomPositionWithMinDistance(Vector3[] existingPositions, int currentIndex)
    {
        Vector3 newPosition;
        int maxAttempts = 50;
        int attempts = 0;
        
        do
        {
            newPosition = new Vector3(
                areaCenter.x + Random.Range(-areaSize.x / 2, areaSize.x / 2),
                areaCenter.y + Random.Range(-areaSize.y / 2, areaSize.y / 2),
                areaCenter.z + Random.Range(-areaSize.z / 2, areaSize.z / 2)
            );
            
            attempts++;
            
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning($"[PlanetRandomizer] Could not find position with min distance after {maxAttempts} attempts. Using last position.");
                break;
            }
            
        } while (!IsPositionValidWithMinDistance(newPosition, existingPositions, currentIndex));
        
        return newPosition;
    }
    
    private bool IsPositionValidWithMinDistance(Vector3 position, Vector3[] existingPositions, int currentIndex)
    {
        for (int i = 0; i < currentIndex; i++)
        {
            float distance = Vector3.Distance(position, existingPositions[i]);
            if (distance < minDistanceBetweenPlanets)
            {
                return false;
            }
        }
        return true;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(areaCenter, areaSize);
    }
}
