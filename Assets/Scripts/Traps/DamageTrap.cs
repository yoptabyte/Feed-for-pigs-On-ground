using UnityEngine;
using Unity.Entities;
using System.Linq;

public class DamageTrap : BaseTrap
{
    [Header("Damage Settings")]
    [SerializeField]
    private int damageAmount = 10;
    [SerializeField]
    private bool instantKill = false;
    
    [Header("Slow Effect Settings")]
    [SerializeField]
    private bool applySlowEffect = true;
    [SerializeField]
    private float slowFactor = 0.3f; // Reduce speed to 30%
    [SerializeField]
    private float slowDuration = 3.0f; // For 3 seconds
    
    [Header("Audio Settings")]
    [SerializeField]
    private bool enableProximityMusic = true;
    [SerializeField]
    private float detectionRadius = 15f; // Distance to detect player
    [SerializeField]
    private AudioClip proximityMusicClip;
    [SerializeField]
    private float musicVolume = 0.5f;
    [SerializeField]
    private bool loopMusic = true;
    
    // Audio components
    private AudioSource audioSource;
    private GameObject player;
    private bool isPlayingMusic = false;
    
    protected override void Awake()
    {
        base.Awake();
        SetupAudioSystem();
    }
    
    private void SetupAudioSystem()
    {
        // Get or create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource
        audioSource.clip = proximityMusicClip;
        audioSource.volume = musicVolume;
        audioSource.loop = loopMusic;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        
        // Auto-assign music clip if not set
        if (proximityMusicClip == null)
        {
            proximityMusicClip = Resources.Load<AudioClip>("three_little");
            if (proximityMusicClip == null)
            {
                proximityMusicClip = Resources.Load<AudioClip>("Assets/Assets/three_little");
            }
            if (proximityMusicClip != null)
            {
                audioSource.clip = proximityMusicClip;
                Debug.Log($"🔥 DamageTrap: Auto-assigned three_little.mp3 as proximity music");
            }
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (enableProximityMusic)
        {
            CheckPlayerProximity();
        }
    }
    
    private void CheckPlayerProximity()
    {
        // Find player if not cached
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("PlayerTag");
            }
        }
        
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        bool shouldPlayMusic = distanceToPlayer <= detectionRadius;
        
        if (shouldPlayMusic && !isPlayingMusic)
        {
            StartProximityMusic();
        }
        else if (!shouldPlayMusic && isPlayingMusic)
        {
            StopProximityMusic();
        }
    }
    
    private void StartProximityMusic()
    {
        if (audioSource != null && proximityMusicClip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            isPlayingMusic = true;
            Debug.Log($"🔥 DamageTrap: Started proximity music for player nearby");
        }
    }
    
    private void StopProximityMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            isPlayingMusic = false;
            Debug.Log($"🔥 DamageTrap: Stopped proximity music - player moved away");
        }
    }
    
    protected override void ApplyEffect(GameObject target)
    {
        Debug.Log($"🔥 DamageTrap: Applying effect to {target.name}");
        
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            int damageToApply = instantKill ? targetHealth.MaxHP : damageAmount;
            Debug.Log($"🔥 DamageTrap: Dealing {damageToApply} damage to {target.name} (HP: {targetHealth.CurrentHP}/{targetHealth.MaxHP})");
            targetHealth.TakeDamage(damageToApply);
        }
        else
        {
            Debug.LogWarning($"🔥 DamageTrap: Target {target.name} does not have a Health component! Components: {string.Join(", ", target.GetComponents<Component>().Select(c => c.GetType().Name))}");
            
            // Try to add Health component if it's missing (for bots)
            EnemyPigAI botAI = target.GetComponent<EnemyPigAI>();
            if (botAI != null)
            {
                Debug.Log($"🔥 DamageTrap: Found bot AI, adding Health component to {target.name}");
                Health newHealth = target.AddComponent<Health>();
                int damageToApply = instantKill ? newHealth.MaxHP : damageAmount;
                Debug.Log($"🔥 DamageTrap: Dealing {damageToApply} damage to newly added Health component");
                newHealth.TakeDamage(damageToApply);
            }
        }

        // Apply slow effect if enabled
        if (applySlowEffect)
        {
            ApplySlowEffect(target);
        }
    }

    // Override OnTriggerEnter to add debug info
    protected override void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🔥 DamageTrap: OnTriggerEnter with {other.name}");
        base.OnTriggerEnter(other);
    }

    // Override OnCollisionEnter to add debug info
    protected override void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"🔥 DamageTrap: OnCollisionEnter with {collision.gameObject.name}");
        base.OnCollisionEnter(collision);
    }

    private void ApplySlowEffect(GameObject target)
    {
        // Try to get EntityLink for status effect system
        EntityLink entityLink = target.GetComponent<EntityLink>();
        if (entityLink != null && entityLink.Entity != Entity.Null)
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            MovementData movementData = target.GetComponent<MovementData>();
            
            if (movementData != null)
            {
                // Create slow effect
                PlayerStatusEffectData slowEffect = new PlayerStatusEffectData
                {
                    Type = EffectType.Slowed,
                    RemainingDuration = slowDuration,
                    EffectStrength = slowFactor,
                    OriginalValue = movementData.moveSpeed
                };

                // Apply or replace existing effect
                if (entityManager.HasComponent<PlayerStatusEffectData>(entityLink.Entity))
                {
                    entityManager.SetComponentData(entityLink.Entity, slowEffect);
                }
                else
                {
                    entityManager.AddComponentData(entityLink.Entity, slowEffect);
                }

                Debug.Log($"Applied slow effect to {target.name}: {slowFactor}x speed for {slowDuration}s");
            }
        }
        else
        {
            // Fallback for entities without EntityLink - direct MovementData modification
            MovementData movementData = target.GetComponent<MovementData>();
            if (movementData != null)
            {
                movementData.moveSpeed *= slowFactor;
                Debug.Log($"Applied direct slow effect to {target.name}: reduced to {movementData.moveSpeed}");
            }
        }

        // Special handling for bot AI interruption
        EnemyPigAI botAI = target.GetComponent<EnemyPigAI>();
        if (botAI != null)
        {
            botAI.InterruptMovement(slowDuration * 0.5f); // Interrupt for half the slow duration
            Debug.Log($"Interrupted bot AI movement for {slowDuration * 0.5f} seconds");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (enableProximityMusic)
        {
            // Draw detection radius in scene view
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
    
    private void OnDestroy()
    {
        // Stop music when trap is destroyed
        if (isPlayingMusic && audioSource != null)
        {
            audioSource.Stop();
        }
    }
} 