using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int maxHP = 3;
    [SerializeField]
    private int currentHP;
    [SerializeField]
    private float regenerationRate = 0.5f; 

    public float timeSinceLastDamage = float.MaxValue;
    public float regenerationDelay = 3.0f; 

    private float regenerationProgress = 0f;

    private bool canStartRegeneration = false; 

    private Vector3 originalScale;
    private Coroutine flattenCoroutine;

    [Header("Blood Effects")]
    [Tooltip("Enable blood splash effects when taking high damage (like from chainsaw)")]
    [SerializeField] private bool enableBloodEffects = true;
    
    [Tooltip("Prefab for blood splash effect")]
    [SerializeField] private GameObject bloodEffectPrefab;
    
    [Tooltip("Minimum damage to trigger blood effect")]
    [SerializeField] private int minDamageForBlood = 2;
    
    [Tooltip("Number of blood splashes for high damage")]
    [Range(1, 5)]
    [SerializeField] private int bloodSplashCount = 3;

    [Header("Death Sound")]
    [Tooltip("Audio source for death sound")]
    [SerializeField] public AudioSource deathAudioSource;
    
    [Tooltip("Death sound clip (die.mp3)")]
    [SerializeField] private AudioClip deathSoundClip;
    
    [Tooltip("Volume of death sound")]
    [Range(0f, 1f)]
    [SerializeField] private float deathSoundVolume = 1.0f;

    public UnityEvent<int> OnDamageTaken;
    public UnityEvent OnDeath;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public bool IsAlive => currentHP > 0;

    private void Awake()
    {
        currentHP = maxHP;
        timeSinceLastDamage = float.MaxValue;
        regenerationProgress = 0f;
        canStartRegeneration = false; 
        originalScale = transform.localScale;
        
        // Initialize death audio source if not assigned
        if (deathAudioSource == null)
        {
            // Look for existing AudioSources that might be suitable for death sound
            AudioSource[] existingAudioSources = GetComponents<AudioSource>();
            
            foreach (AudioSource source in existingAudioSources)
            {
                // Check if this AudioSource is used by MovementSystem
                MovementSystem movementSystem = GetComponent<MovementSystem>();
                if (movementSystem != null)
                {
                    // Skip if this AudioSource is being used for walking
                    if (source.clip != null && source.clip.name.Contains("walk"))
                    {
                        continue;
                    }
                    // Skip if this is the walking AudioSource (loop = true indicates walking)
                    if (source.loop == true)
                    {
                        continue;
                    }
                }
                
                // This AudioSource seems available
                deathAudioSource = source;
                break;
            }
            
            // Create dedicated AudioSource for death if none found
            if (deathAudioSource == null)
            {
                deathAudioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log($"💀 Health: Created dedicated AudioSource for death sound on {gameObject.name}");
            }
            
            // Configure death AudioSource
            deathAudioSource.playOnAwake = false;
            deathAudioSource.loop = false; // Death sound should not loop
            deathAudioSource.spatialBlend = 1.0f; // 3D sound
            deathAudioSource.priority = 64; // Higher priority for death sounds
            
            // Try to auto-load die.mp3 if not assigned
            if (deathSoundClip == null)
            {
                deathSoundClip = Resources.Load<AudioClip>("die");
                if (deathSoundClip != null)
                {
                    Debug.Log($"💀 Health: Auto-loaded die.mp3 for {gameObject.name}");
                }
            }
        }
    }

    private void Update()
    {
        if (IsAlive && canStartRegeneration && timeSinceLastDamage < regenerationDelay)
        {
          timeSinceLastDamage += Time.deltaTime;
        }

        if (CanRegenerate())
        {
            regenerationProgress += regenerationRate * Time.deltaTime;
            if (regenerationProgress >= 1.0f)
            {
              int healAmountInt = Mathf.FloorToInt(regenerationProgress);
              Heal(healAmountInt);
              regenerationProgress -= healAmountInt; 
            }
        }
        else
        {
          regenerationProgress = 0f;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (!IsAlive || damageAmount <= 0)
        {
          return;
        }

        currentHP -= damageAmount;
        currentHP = Mathf.Max(currentHP, 0);

        timeSinceLastDamage = 0f;
        canStartRegeneration = false; 
        regenerationProgress = 0f;    

        // Trigger blood effect for high damage (like chainsaw)
        if (enableBloodEffects && damageAmount >= minDamageForBlood)
        {
            CreateBloodEffect(damageAmount);
            TriggerCameraShake(); // Camera shake for high damage
        }

        OnDamageTaken?.Invoke(damageAmount);
        Debug.Log($"{gameObject.name} took {damageAmount} damage. HP left: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void CreateBloodEffect(int damageAmount)
    {
        // Calculate number of blood splashes based on damage
        int bloodCount = Mathf.Min(damageAmount, bloodSplashCount);
        
        // Position blood effect slightly above the pig
        Vector3 bloodPosition = transform.position + Vector3.up * 0.5f;
        
        for (int i = 0; i < bloodCount; i++)
        {
            // Add some randomization to blood splash positions
            Vector3 randomOffset = Random.insideUnitSphere * 0.8f;
            randomOffset.y = Mathf.Abs(randomOffset.y); // Keep blood above ground
            Vector3 finalPosition = bloodPosition + randomOffset;
            
            // Random direction for blood spray
            Vector3 randomDirection = Random.onUnitSphere;
            randomDirection.y = Mathf.Max(0.3f, randomDirection.y); // Ensure upward spray
            
            // Create blood splash effect
            if (bloodEffectPrefab != null)
            {
                BloodSplashEffect.CreateBloodSplashAt(finalPosition, randomDirection, bloodEffectPrefab);
            }
            else
            {
                // Create basic blood effect if no prefab assigned
                BloodSplashEffect.CreateBloodSplashAt(finalPosition, randomDirection);
            }
        }
        
        Debug.Log($"Created {bloodCount} blood splashes for {damageAmount} damage on {gameObject.name}");
    }

    private void Die()
    {
        // Play death sound
        PlayDeathSound();
        
        // Create extra blood effect on death
        if (enableBloodEffects)
        {
            CreateBloodEffect(bloodSplashCount + 1); // Extra blood on death
        }
        
        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} died.");
        // Destroy(gameObject);
    }

    public void Heal(int healAmount)
    {
        if (!IsAlive || healAmount <= 0 || currentHP >= maxHP)
        {
            // regenerationProgress = 0f;
            return;
        }

        int oldHP = currentHP;
        currentHP += healAmount;
        currentHP = Mathf.Min(currentHP, maxHP);
        Debug.Log($"{gameObject.name} healed {currentHP - oldHP}. HP: {currentHP}"); 

        // Trigger UI update by invoking damage event with negative value (healing)
        OnDamageTaken?.Invoke(-(currentHP - oldHP));

        if (currentHP >= maxHP)
        {
             regenerationProgress = 0f;
             canStartRegeneration = false;
        }
    }

    // New method for instant healing effect (like from pickup items)
    public void InstantHeal(int healAmount)
    {
        if (!IsAlive || healAmount <= 0 || currentHP >= maxHP)
        {
            return;
        }

        int oldHP = currentHP;
        currentHP += healAmount;
        currentHP = Mathf.Min(currentHP, maxHP);
        Debug.Log($"{gameObject.name} instantly healed {currentHP - oldHP}. HP: {currentHP}"); 

        // Trigger UI update immediately
        OnDamageTaken?.Invoke(-(currentHP - oldHP));

        // Reset regeneration if at full health
        if (currentHP >= maxHP)
        {
             regenerationProgress = 0f;
             canStartRegeneration = false;
        }
    }

    public void EnableRegeneration()
    {
        if (IsAlive && currentHP < maxHP) 
        {
            canStartRegeneration = true;
            timeSinceLastDamage = 0f; 
            Debug.Log($"{gameObject.name} regeneration enabled. Delay timer started.");
        }
    }

    public bool CanRegenerate()
    {
        return canStartRegeneration && IsAlive && timeSinceLastDamage >= regenerationDelay && currentHP < maxHP;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Vehicle"))
        {
            if (flattenCoroutine != null)
            {
                StopCoroutine(flattenCoroutine);
            }
            flattenCoroutine = StartCoroutine(FlattenEffect(5.0f));
        }
    }

    private void TriggerCameraShake()
    {
        // Simple camera shake effect for high damage
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            StartCoroutine(CameraShakeCoroutine(mainCamera, 0.3f, 0.5f));
        }
    }

    private System.Collections.IEnumerator CameraShakeCoroutine(Camera camera, float duration, float intensity)
    {
        Vector3 originalPosition = camera.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            camera.transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        camera.transform.localPosition = originalPosition;
    }

    private IEnumerator FlattenEffect(float duration)
    {
        Debug.Log($"{gameObject.name} entered Vehicle trigger. Starting flatten effect for {duration} seconds.");
        
        transform.localScale = new Vector3(originalScale.x, 0.1f, originalScale.z);
        
        TakeDamage(1); // Regular damage - no blood effect because damage < minDamageForBlood
        
        yield return new WaitForSeconds(duration);
        
        transform.localScale = originalScale;
        Debug.Log($"{gameObject.name} flatten effect finished. Restored original scale.");
        
        flattenCoroutine = null;
    }

    private void PlayDeathSound()
    {
        // Stop any other sounds on this object that might interfere
        MovementSystem movementSystem = GetComponent<MovementSystem>();
        if (movementSystem != null)
        {
            // Stop walking audio if it's playing
            AudioSource[] allAudioSources = GetComponents<AudioSource>();
            foreach (AudioSource source in allAudioSources)
            {
                if (source != deathAudioSource && source.isPlaying && source.clip != null)
                {
                    if (source.clip.name.Contains("walk") || source.loop)
                    {
                        source.Stop();
                        Debug.Log($"💀 Health: Stopped conflicting audio ({source.clip.name}) to play death sound");
                    }
                }
            }
        }
        
        if (deathAudioSource != null && deathSoundClip != null)
        {
            // Force stop any current playback on this AudioSource
            if (deathAudioSource.isPlaying)
            {
                deathAudioSource.Stop();
            }
            
            // Set up and play death sound
            deathAudioSource.clip = deathSoundClip;
            deathAudioSource.volume = deathSoundVolume;
            deathAudioSource.pitch = 1.0f; // Reset pitch in case it was modified
            deathAudioSource.loop = false; // Ensure it doesn't loop
            deathAudioSource.Play();
            
            Debug.Log($"💀 Playing death sound for {gameObject.name} (Volume: {deathSoundVolume}, Clip: {deathSoundClip.name})");
        }
        else
        {
            Debug.LogWarning($"💀 Cannot play death sound for {gameObject.name}: AudioSource={deathAudioSource != null}, AudioClip={deathSoundClip != null}");
            
            // Try emergency fallback - use any available AudioSource
            if (deathSoundClip != null)
            {
                AudioSource[] allSources = GetComponents<AudioSource>();
                foreach (AudioSource source in allSources)
                {
                    if (source != null)
                    {
                        source.Stop();
                        source.clip = deathSoundClip;
                        source.volume = deathSoundVolume;
                        source.loop = false;
                        source.Play();
                        Debug.Log($"💀 Emergency: Playing death sound using fallback AudioSource on {gameObject.name}");
                        break;
                    }
                }
            }
        }
    }
}