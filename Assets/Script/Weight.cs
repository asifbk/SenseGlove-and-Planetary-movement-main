using UnityEngine;

public class PlanetWeight : MonoBehaviour
{
    [Header("Planet Settings")]
    public string planetName;
    public float weight = 10f; // assign per planet in Inspector (e.g., Earth = 100, Mars = 50)

    [Header("Palm Interaction")]
    public string palmColliderTag = "Palm"; // Tag your SGHand palm collider with "Palm"

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(palmColliderTag))
        {
            Debug.Log($"Touched {planetName}. Weight = {weight} units");
            // Later: send haptic feedback or update UI here
        }
    }
}
