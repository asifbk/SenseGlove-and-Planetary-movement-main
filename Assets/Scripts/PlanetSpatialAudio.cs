using UnityEngine;
using SG;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SG_Grabable))]
public class PlanetSpatialAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Whoosh sound clip")]
    public AudioClip whooshSound;
    
    [Tooltip("Minimum velocity to trigger sound (m/s)")]
    public float minVelocity = 0.5f;
    
    [Tooltip("Velocity for maximum volume (m/s)")]
    public float maxVelocity = 5f;
    
    [Tooltip("Minimum volume")]
    [Range(0f, 1f)]
    public float minVolume = 0.1f;
    
    [Tooltip("Maximum volume")]
    [Range(0f, 1f)]
    public float maxVolume = 0.8f;
    
    [Tooltip("Minimum pitch")]
    [Range(0.5f, 2f)]
    public float minPitch = 0.8f;
    
    [Tooltip("Maximum pitch")]
    [Range(0.5f, 2f)]
    public float maxPitch = 1.5f;
    
    [Header("3D Audio Settings")]
    [Tooltip("Minimum distance for 3D sound")]
    public float minDistance = 1f;
    
    [Tooltip("Maximum distance for 3D sound")]
    public float maxDistance = 20f;
    
    [Tooltip("Doppler effect level")]
    [Range(0f, 5f)]
    public float dopplerLevel = 1f;
    
    private AudioSource audioSource;
    private SG_Grabable grabable;
    private PlanetMotionTracker motionTracker;
    private Vector3 previousPosition;
    private Vector3 currentVelocity;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        grabable = GetComponent<SG_Grabable>();
        motionTracker = GetComponentInParent<PlanetMotionTracker>();
        
        previousPosition = transform.position;
        
        ConfigureAudioSource();
    }
    
    void Update()
    {
        UpdateVelocity();
        
        if (grabable != null && grabable.IsGrabbed())
        {
            UpdateAudioBasedOnVelocity();
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
    
    private void ConfigureAudioSource()
    {
        if (audioSource == null) return;
        
        audioSource.clip = whooshSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.dopplerLevel = dopplerLevel;
        audioSource.volume = 0f;
    }
    
    private void UpdateVelocity()
    {
        if (motionTracker != null)
        {
            PlanetMotionTracker.PlanetMotionData data = motionTracker.GetPlanetData(transform);
            if (data != null)
            {
                currentVelocity = data.velocity;
                return;
            }
        }
        
        Vector3 currentPosition = transform.position;
        currentVelocity = (currentPosition - previousPosition) / Time.deltaTime;
        previousPosition = currentPosition;
    }
    
    private void UpdateAudioBasedOnVelocity()
    {
        if (audioSource == null || whooshSound == null) return;
        
        float velocityMagnitude = currentVelocity.magnitude;
        
        if (velocityMagnitude < minVelocity)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            return;
        }
        
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        
        float velocityNormalized = Mathf.Clamp01((velocityMagnitude - minVelocity) / (maxVelocity - minVelocity));
        
        audioSource.volume = Mathf.Lerp(minVolume, maxVolume, velocityNormalized);
        audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, velocityNormalized);
    }
}
