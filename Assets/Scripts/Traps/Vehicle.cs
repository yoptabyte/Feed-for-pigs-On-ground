using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class VehicleMovement : MonoBehaviour
{
    [Header("Wheels")]
    [Tooltip("Front left wheel transform")]
    [SerializeField] private Transform frontLeftWheel;

    [Tooltip("Front right wheel transform")]
    [SerializeField] private Transform frontRightWheel;

    [Tooltip("Rear left wheel transform")]
    [SerializeField] private Transform rearLeftWheel;

    [Tooltip("Rear right wheel transform")]
    [SerializeField] private Transform rearRightWheel;

    [Header("Rotation Settings")]
    [Tooltip("Wheel rotation speed (degrees per second)")]
    [SerializeField] private float rotationSpeed = 360f;

    [Tooltip("Rotate wheels forward (positive speed)? If unchecked, rotates backward.")]
    [SerializeField] private bool rotateForward = true;

    [Header("Movement Settings")]
    [Tooltip("Waypoints for the vehicle path")]
    [SerializeField] private Transform[] waypoints;

    [Tooltip("Movement speed (units per second)")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Detection Settings")]
    [Tooltip("Detection radius for player and enemies")]
    [SerializeField] private float detectionRadius = 10f;

    [Tooltip("Layers to detect (Player and Enemy layers)")]
    [SerializeField] private LayerMask detectionLayers = -1;

    [Tooltip("Tags to detect for activation")]
    [SerializeField] private string[] activationTags = { "Player", "PlayerTag" };

    [Tooltip("Should detect enemies as well?")]
    [SerializeField] private bool detectEnemies = true;

    [Tooltip("Show detection radius in Scene view")]
    [SerializeField] private bool showDetectionGizmo = true;

    [Header("Audio Settings")]
    [Tooltip("Audio clip for car engine sound")]
    [SerializeField] private AudioClip carEngineClip;

    [Tooltip("Volume for car engine sound")]
    [SerializeField] private float engineVolume = 0.5f;

    [Tooltip("Engine sound pitch when moving")]
    [SerializeField] private float enginePitch = 1f;

    [Tooltip("Should loop engine sound?")]
    [SerializeField] private bool loopEngineSound = true;

    [Header("Collision Settings")]
    [Tooltip("Damage dealt to targets on collision")]
    [SerializeField] private float damage = 100f;

    [Tooltip("Sound to play when hitting targets")]
    [SerializeField] private AudioClip collisionSound;

    [Tooltip("Volume for collision sound")]
    [SerializeField] private float collisionVolume = 0.8f;

    [Tooltip("Cooldown between collisions with the same target")]
    [SerializeField] private float collisionCooldown = 1f;

    private int currentWaypointIndex = 0;
    private AudioSource audioSource;
    private bool hasReachedFinalDestination = false;
    private bool shouldMove = false;
    
    // Collision tracking
    private Dictionary<GameObject, float> lastCollisionTimes = new Dictionary<GameObject, float>();

    void Start()
    {
        SetupAudioSource();
        SetupCollider();
        // DO NOT start engine immediately - wait for detection
    }

    void Update()
    {
        CheckForNearbyTargets();
        HandleMovement();
        HandleWheelRotation();
        HandleEngineSound();
    }

    private void SetupCollider()
    {
        // Ensure the vehicle has a collider for collision detection
        Collider vehicleCollider = GetComponent<Collider>();
        if (vehicleCollider == null)
        {
            // Add a box collider if none exists
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = false; // Not a trigger, we want collision events
            Debug.Log("VehicleMovement: Added BoxCollider for collision detection");
        }

        // Ensure the vehicle has a Rigidbody for physics
        Rigidbody vehicleRb = GetComponent<Rigidbody>();
        if (vehicleRb == null)
        {
            vehicleRb = gameObject.AddComponent<Rigidbody>();
            vehicleRb.isKinematic = true; // Kinematic so it's not affected by physics but can trigger collisions
            Debug.Log("VehicleMovement: Added kinematic Rigidbody for collision detection");
        }
    }

    private void SetupAudioSource()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Auto-assign car.mp3 if no clip is assigned
        if (carEngineClip == null)
        {
            carEngineClip = Resources.Load<AudioClip>("car");
            if (carEngineClip == null)
            {
                // Try to load from Assets folder
                carEngineClip = UnityEngine.Resources.Load<AudioClip>("Assets/car");
            }
        }

        // Setup AudioSource properties
        if (audioSource != null)
        {
            audioSource.clip = carEngineClip;
            audioSource.volume = engineVolume;
            audioSource.pitch = enginePitch;
            audioSource.loop = loopEngineSound;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }

        if (carEngineClip == null)
        {
            Debug.LogWarning("VehicleMovement: car.mp3 audio clip not found! Please assign it manually in the inspector.");
        }
    }

    private void CheckForNearbyTargets()
    {
        shouldMove = false;
        
        // Check for overlapping colliders in detection radius
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayers);
        
        foreach (Collider collider in nearbyColliders)
        {
            // Skip self
            if (collider.transform == transform) continue;
            
            // Check for activation tags (Player)
            foreach (string tag in activationTags)
            {
                if (collider.CompareTag(tag))
                {
                    shouldMove = true;
                    return;
                }
            }
            
            // Check for enemies if enabled
            if (detectEnemies)
            {
                EnemyPigAI enemy = collider.GetComponent<EnemyPigAI>();
                if (enemy != null)
                {
                    shouldMove = true;
                    return;
                }
                
                NavMeshEnemyAI navEnemy = collider.GetComponent<NavMeshEnemyAI>();
                if (navEnemy != null)
                {
                    shouldMove = true;
                    return;
                }
                
                // Alternative enemy detection by tag
                if (collider.CompareTag("Enemy"))
                {
                    shouldMove = true;
                    return;
                }
            }
        }
    }

    private void HandleMovement()
    {
        if (waypoints == null || waypoints.Length == 0 || hasReachedFinalDestination)
        {
            return;
        }

        // Only move if targets detected
        if (!shouldMove)
        {
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Constantly move towards current waypoint
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, moveSpeed * Time.deltaTime);

        // Reached waypoint?
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex++;

            // If only one waypoint or reached the end
            if (waypoints.Length == 1 || currentWaypointIndex >= waypoints.Length)
            {
                hasReachedFinalDestination = true;
                StopEngine();
                Debug.Log("🚗🛑 Vehicle reached final destination and stopped");
                return;
            }
        }
    }

    private void HandleWheelRotation()
    {
        // Don't rotate wheels if stopped or no targets
        if (hasReachedFinalDestination || !shouldMove)
        {
            return;
        }

        float directionMultiplier = rotateForward ? 1f : -1f;
        float rotationAngle = rotationSpeed * directionMultiplier * Time.deltaTime;

        RotateWheel(frontLeftWheel, rotationAngle);
        RotateWheel(frontRightWheel, rotationAngle);
        RotateWheel(rearLeftWheel, rotationAngle);
        RotateWheel(rearRightWheel, rotationAngle);
    }

    private void RotateWheel(Transform wheel, float angle)
    {
        if (wheel != null)
        {
            wheel.Rotate(Vector3.right, angle, Space.Self);
        }
    }

    private void StartEngine()
    {
        if (audioSource != null && carEngineClip != null)
        {
            audioSource.Play();
            Debug.Log("Vehicle engine started");
        }
    }

    private void StopEngine()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("Vehicle engine stopped");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleVehicleCollision(collision);
    }

    private void HandleVehicleCollision(Collision collision)
    {
        GameObject target = collision.gameObject;
        
        // Check cooldown to prevent spam damage
        if (lastCollisionTimes.ContainsKey(target))
        {
            if (Time.time - lastCollisionTimes[target] < collisionCooldown)
            {
                return;
            }
        }

        // Record collision time
        lastCollisionTimes[target] = Time.time;

        // Try to damage the target
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage((int)damage);
            Debug.Log($"🚗💀 Vehicle crushed {target.name} for {damage} damage!");
        }
        
        // Play collision sound
        PlayCollisionSound();
    }

    private void PlayCollisionSound()
    {
        if (audioSource != null && collisionSound != null)
        {
            audioSource.PlayOneShot(collisionSound, collisionVolume);
        }
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (showDetectionGizmo)
        {
            Gizmos.color = shouldMove ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
        
        // Draw waypoint path
        if (waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    // Draw waypoint
                    Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);
                    
                    // Draw line to next waypoint
                    int nextIndex = (i + 1) % waypoints.Length;
                    if (waypoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
                    }
                }
            }
            
            // Highlight current waypoint
            if (currentWaypointIndex < waypoints.Length && waypoints[currentWaypointIndex] != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(waypoints[currentWaypointIndex].position, 0.7f);
            }
        }
    }

    private void HandleEngineSound()
    {
        bool shouldPlayEngine = shouldMove && !hasReachedFinalDestination;

        if (shouldPlayEngine && audioSource != null && !audioSource.isPlaying)
        {
            StartEngine();
        }
        else if (!shouldPlayEngine && audioSource != null && audioSource.isPlaying)
        {
            StopEngine();
        }
    }
}