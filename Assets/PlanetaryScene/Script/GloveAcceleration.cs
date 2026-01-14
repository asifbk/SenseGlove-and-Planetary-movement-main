using UnityEngine;
using SG;

public class SG_GloveAcceleration : MonoBehaviour
{
    [Header("Assign the glove transform (SG_TrackedHand or Wrist object)")]
    public Transform gloveTransform;

    private Vector3 lastPos;
    private Vector3 lastVelocity;
    
    public Vector3 velocity;
    public Vector3 acceleration;

    void Start()
    {
        if (gloveTransform == null)
            gloveTransform = this.transform;

        lastPos = gloveTransform.position;
        lastVelocity = Vector3.zero;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // Velocity = Δposition / Δtime
        velocity = (gloveTransform.position - lastPos) / dt;

        // Acceleration = Δvelocity / Δtime
        acceleration = (velocity - lastVelocity) / dt;

        // Store for next frame
        lastPos = gloveTransform.position;
        lastVelocity = velocity;

        Debug.Log($"[Glove Accel] V={velocity.magnitude:F3} m/s | A={acceleration.magnitude:F3} m/s²");
    }
}
