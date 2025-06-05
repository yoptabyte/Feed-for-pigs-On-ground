using UnityEngine;
using System.Collections.Generic;

public class ShotgunTrap : BaseTrap
{
    [Header("Shotgun Specific Settings")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Transform shootPoint;
    [SerializeField]
    private float projectileSpeed = 20f;
    [SerializeField]
    private int pelletCount = 5;
    [SerializeField]
    private float spreadAngle = 120f;

    [Header("Tripwire Link")]
    [Tooltip("List of tripwires that activate this shotgun.")]
    [SerializeField]
    private List<Tripwire> linkedTripwires = new List<Tripwire>();
    
    [Header("Audio Settings")]
    [SerializeField]
    private bool enableShotSound = true;
    [SerializeField]
    private AudioClip shotSoundClip;
    [SerializeField]
    private float maxShotVolume = 1.0f;
    [SerializeField]
    private float maxAudibleDistance = 50f; // Max distance where sound can be heard
    [SerializeField]
    private float minVolumeThreshold = 0.1f; // Minimum volume before sound stops
    
    // Audio components
    private AudioSource audioSource;
    private GameObject player;

    protected override void Awake()
    {
        base.Awake();
        SetupAudioSystem();
        SetupTripwireLinks();
    }
    
    private void SetupAudioSystem()
    {
        // Get or create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource for 3D distance-based audio
        audioSource.clip = shotSoundClip;
        audioSource.volume = maxShotVolume;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = maxAudibleDistance;
        
        // Auto-assign shot sound if not set
        if (shotSoundClip == null)
        {
            shotSoundClip = Resources.Load<AudioClip>("canon");
            if (shotSoundClip == null)
            {
                shotSoundClip = Resources.Load<AudioClip>("Assets/canon");
            }
            if (shotSoundClip != null)
            {
                audioSource.clip = shotSoundClip;
                Debug.Log($"🔫 ShotgunTrap: Auto-assigned canon.mp3 as shot sound");
            }
        }
    }
    
    private void SetupTripwireLinks()
    {
        foreach (var tripwire in linkedTripwires)
        {
            if (tripwire != null)
            {
                tripwire.OnTripwireTriggered.AddListener(ExternalTrigger);
            }
            else
            {
                Debug.LogWarning($"Null tripwire linked in {gameObject.name}", this);
            }
        }
        if (projectilePrefab == null)
        {
            Debug.LogError($"Projectile Prefab is not assigned in {gameObject.name}!", this);
        }
        if (shootPoint == null)
        {
            Debug.LogWarning($"Shoot Point is not assigned in {gameObject.name}. Using trap's position.", this);
            shootPoint = transform;
        }
    }

    public void ExternalTrigger(GameObject triggerTarget)
    {
        base.TriggerTrap(triggerTarget);
    }

    protected override void ApplyEffect(GameObject target)
    {
        Shoot();
    }

    private void Shoot()
    {
        if (projectilePrefab == null) return;

        Debug.Log($"{gameObject.name} shooting!");
        
        // Play shot sound with distance-based volume
        PlayShotSound();

        for (int i = 0; i < pelletCount; i++)
        {
            Quaternion spreadRotation = Quaternion.Euler(Random.Range(-spreadAngle / 2, spreadAngle / 2), Random.Range(-spreadAngle / 2, spreadAngle / 2), 0);
            Vector3 direction = spreadRotation * shootPoint.forward;

            GameObject projectileGO = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            Projectile projectile = projectileGO.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.Initialize(direction * projectileSpeed);
            }
            else
            {
                Debug.LogError($"Projectile Prefab {projectilePrefab.name} is missing Projectile component!", this);
                Destroy(projectileGO);
            }
        }
    }
    
    private void PlayShotSound()
    {
        if (!enableShotSound || audioSource == null || shotSoundClip == null) return;
        
        // Find player for distance calculation
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("PlayerTag");
            }
        }
        
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            
            // Calculate volume based on distance
            float volumeMultiplier = CalculateVolumeByDistance(distanceToPlayer);
            
            if (volumeMultiplier >= minVolumeThreshold)
            {
                audioSource.volume = maxShotVolume * volumeMultiplier;
                audioSource.Play();
                
                Debug.Log($"🔫 ShotgunTrap: Played shot sound at {volumeMultiplier:F2}x volume (distance: {distanceToPlayer:F1}m)");
            }
            else
            {
                Debug.Log($"🔫 ShotgunTrap: Shot too far away to hear (distance: {distanceToPlayer:F1}m)");
            }
        }
        else
        {
            // If player not found, play at full volume
            audioSource.volume = maxShotVolume;
            audioSource.Play();
            Debug.Log($"🔫 ShotgunTrap: Played shot sound at full volume (player not found)");
        }
    }
    
    private float CalculateVolumeByDistance(float distance)
    {
        if (distance <= audioSource.minDistance)
        {
            return 1f; // Full volume when very close
        }
        else if (distance >= maxAudibleDistance)
        {
            return 0f; // No sound when too far
        }
        else
        {
            // Linear falloff between min and max distance
            float normalizedDistance = (distance - audioSource.minDistance) / (maxAudibleDistance - audioSource.minDistance);
            return Mathf.Lerp(1f, 0f, normalizedDistance);
        }
    }

    protected override void ResetTrap()
    {
        base.ResetTrap();
        foreach (var tripwire in linkedTripwires)
        {
            if (tripwire != null)
            {
                tripwire.ResetTripwire();
            }
        }
    }

    protected override void OnTriggerEnter(Collider other) { }
    protected override void OnCollisionEnter(Collision collision) { }
    
    private void OnDrawGizmosSelected()
    {
        if (enableShotSound)
        {
            // Draw audio range in scene view
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxAudibleDistance);
            
            // Draw inner range where volume is maximum
            Gizmos.color = Color.green;
            if (audioSource != null)
            {
                Gizmos.DrawWireSphere(transform.position, audioSource.minDistance);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, 1f);
            }
        }
    }
} 