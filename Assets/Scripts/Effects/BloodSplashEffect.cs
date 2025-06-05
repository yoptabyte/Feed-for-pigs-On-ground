using UnityEngine;

public class BloodSplashEffect : MonoBehaviour
{
    [Header("Blood Splash Settings")]
    [Tooltip("Particle system for blood effect")]
    [SerializeField] private ParticleSystem bloodParticleSystem;
    
    [Tooltip("Intensity of blood splash effect")]
    [Range(10, 200)]
    [SerializeField] private int bloodParticleCount = 80;
    
    [Tooltip("Duration of blood splash")]
    [Range(0.5f, 5.0f)]
    [SerializeField] private float splashDuration = 2.5f;
    
    [Tooltip("Force of blood particles")]
    [Range(5f, 30f)]
    [SerializeField] private float bloodForce = 15f;
    
    [Tooltip("Size of blood particles")]
    [Range(0.05f, 0.8f)]
    [SerializeField] private float particleSize = 0.2f;
    
    [Header("Realistic Blood Settings")]
    [Tooltip("Create multiple blood droplet sizes")]
    [SerializeField] private bool enableRealisticDroplets = true;
    
    [Tooltip("Add blood splatters on ground")]
    [SerializeField] private bool enableGroundSplatters = true;
    
    [Tooltip("Blood viscosity effect (slower falling)")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float bloodViscosity = 0.7f;
    
    [Header("Sound Effects")]
    [Tooltip("Audio source for blood splash sound")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("Blood splash sound clip")]
    [SerializeField] private AudioClip bloodSplashSound;
    
    [Tooltip("Volume of blood splash sound")]
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 0.7f;
    
    private bool hasTriggered = false;
    
    void Awake()
    {
        if (bloodParticleSystem == null)
        {
            bloodParticleSystem = GetComponent<ParticleSystem>();
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        SetupParticleSystem();
    }
    
    private void SetupParticleSystem()
    {
        if (bloodParticleSystem == null) return;
        
        var main = bloodParticleSystem.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(splashDuration * 0.5f, splashDuration);
        main.startSpeed = new ParticleSystem.MinMaxCurve(bloodForce * 0.3f, bloodForce);
        main.maxParticles = bloodParticleCount;
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.5f, particleSize * 2f);
        main.startColor = new Color(0.9f, 0.1f, 0.1f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        
        // Enhanced shape for more realistic spray
        var shape = bloodParticleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 60f; // Wider spray
        shape.radius = 0.3f;
        shape.radiusThickness = 0.8f; // More particles from edges
        
        // Realistic velocity with turbulence
        var velocityOverLifetime = bloodParticleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-9.81f * bloodViscosity); // Gravity with viscosity
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-2f, 2f); // Random horizontal drift
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-2f, 2f);
        
        // Size over lifetime for realistic droplet behavior
        var sizeOverLifetime = bloodParticleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f); // Start normal size
        sizeCurve.AddKey(0.3f, 1.2f); // Slightly expand in air
        sizeCurve.AddKey(0.8f, 0.8f); // Shrink as it dries
        sizeCurve.AddKey(1f, 0.3f); // Very small at end
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Realistic color transition
        var colorOverLifetime = bloodParticleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[4];
        colorKeys[0] = new GradientColorKey(new Color(0.95f, 0.1f, 0.1f), 0f); // Fresh bright red
        colorKeys[1] = new GradientColorKey(new Color(0.8f, 0.05f, 0.05f), 0.4f); // Darker red
        colorKeys[2] = new GradientColorKey(new Color(0.5f, 0.02f, 0.02f), 0.8f); // Dark red
        colorKeys[3] = new GradientColorKey(new Color(0.3f, 0.01f, 0.01f), 1f); // Almost black (dried)
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(0.8f, 0.7f);
        alphaKeys[2] = new GradientAlphaKey(0f, 1f);
        gradient.SetKeys(colorKeys, alphaKeys);
        colorOverLifetime.color = gradient;
        
        // Add rotation for more dynamic effect
        var rotationOverLifetime = bloodParticleSystem.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);
        
        // Add noise for more organic movement
        var noise = bloodParticleSystem.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.5f;
        noise.damping = true;
        
        // Set material for better blood appearance
        var renderer = bloodParticleSystem.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            // Use Default-Particle material for round particles
            Material bloodMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
            if (bloodMaterial.shader == null)
            {
                // Fallback to mobile shader if available
                bloodMaterial = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
            }
            if (bloodMaterial.shader == null)
            {
                // Final fallback
                bloodMaterial = new Material(Shader.Find("Sprites/Default"));
            }
            
            bloodMaterial.color = new Color(0.9f, 0.1f, 0.1f, 1f);
            bloodMaterial.mainTexture = null;
            renderer.material = bloodMaterial;
            
            // Make particles round and smooth
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortMode = ParticleSystemSortMode.Distance;
        }
        
        // Stop the particle system initially
        bloodParticleSystem.Stop();
    }
    
    public void TriggerBloodSplash(Vector3 impactPoint, Vector3 impactDirection)
    {
        if (hasTriggered) return;
        
        hasTriggered = true;
        
        // Position the effect at impact point
        transform.position = impactPoint;
        
        // Orient the effect based on impact direction
        if (impactDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(impactDirection);
        }
        
        // Create multiple blood effects for realism
        if (enableRealisticDroplets)
        {
            CreateAdditionalBloodEffects(impactPoint, impactDirection);
        }
        
        // Play main particle effect
        if (bloodParticleSystem != null)
        {
            bloodParticleSystem.Play();
        }
        
        // Create ground splatters
        if (enableGroundSplatters)
        {
            CreateGroundSplatters(impactPoint);
        }
        
        // Play sound effect
        PlayBloodSound();
        
        Debug.Log($"Realistic blood splash triggered at {impactPoint}");
        
        // Auto-destroy after effect finishes
        Destroy(gameObject, splashDuration + 2f);
    }
    
    private void CreateAdditionalBloodEffects(Vector3 position, Vector3 direction)
    {
        // Create smaller blood droplets
        for (int i = 0; i < 3; i++)
        {
            GameObject smallDroplet = new GameObject($"BloodDroplet_{i}");
            smallDroplet.transform.position = position + Random.insideUnitSphere * 0.3f;
            
            ParticleSystem ps = smallDroplet.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = splashDuration * 0.8f;
            main.startSpeed = bloodForce * 0.5f;
            main.maxParticles = 15;
            main.startSize = particleSize * 0.3f;
            main.startColor = new Color(0.85f, 0.08f, 0.08f, 1f);
            
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            
            // Fix material for droplets
            var dropletRenderer = ps.GetComponent<ParticleSystemRenderer>();
            if (dropletRenderer != null)
            {
                Material dropletMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
                if (dropletMaterial.shader == null)
                {
                    dropletMaterial = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
                }
                if (dropletMaterial.shader == null)
                {
                    dropletMaterial = new Material(Shader.Find("Sprites/Default"));
                }
                dropletMaterial.color = new Color(0.85f, 0.08f, 0.08f, 1f);
                dropletRenderer.material = dropletMaterial;
                dropletRenderer.alignment = ParticleSystemRenderSpace.View;
            }
            
            ps.Play();
            Destroy(smallDroplet, splashDuration + 1f);
        }
        
        // Create blood mist effect
        GameObject mist = new GameObject("BloodMist");
        mist.transform.position = position;
        
        ParticleSystem mistPS = mist.AddComponent<ParticleSystem>();
        var mistMain = mistPS.main;
        mistMain.startLifetime = splashDuration * 1.5f;
        mistMain.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        mistMain.maxParticles = 30;
        mistMain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        mistMain.startColor = new Color(0.7f, 0.05f, 0.05f, 0.3f); // Semi-transparent blood mist
        
        var mistShape = mistPS.shape;
        mistShape.enabled = true;
        mistShape.shapeType = ParticleSystemShapeType.Sphere;
        mistShape.radius = 0.8f;
        
        // Fix material for mist
        var mistRenderer = mistPS.GetComponent<ParticleSystemRenderer>();
        if (mistRenderer != null)
        {
            Material mistMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
            if (mistMaterial.shader == null)
            {
                mistMaterial = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
            }
            if (mistMaterial.shader == null)
            {
                mistMaterial = new Material(Shader.Find("Sprites/Default"));
            }
            mistMaterial.color = new Color(0.7f, 0.05f, 0.05f, 0.3f);
            mistRenderer.material = mistMaterial;
            mistRenderer.alignment = ParticleSystemRenderSpace.View;
        }
        
        mistPS.Play();
        Destroy(mist, splashDuration + 2f);
    }
    
    private void CreateGroundSplatters(Vector3 position)
    {
        // Raycast down to find ground
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, 10f))
        {
            Vector3 groundPos = hit.point + Vector3.up * 0.01f; // Slightly above ground
            
            // Create blood pool effect
            GameObject splatter = new GameObject("BloodSplatter");
            splatter.transform.position = groundPos;
            splatter.transform.rotation = Quaternion.LookRotation(hit.normal);
            
            ParticleSystem splatterPS = splatter.AddComponent<ParticleSystem>();
            var splatterMain = splatterPS.main;
            splatterMain.startLifetime = splashDuration * 2f;
            splatterMain.startSpeed = 0f; // No movement, just appears
            splatterMain.maxParticles = 10;
            splatterMain.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            splatterMain.startColor = new Color(0.6f, 0.03f, 0.03f, 0.8f);
            splatterMain.simulationSpace = ParticleSystemSimulationSpace.World;
            
            var splatterShape = splatterPS.shape;
            splatterShape.enabled = true;
            splatterShape.shapeType = ParticleSystemShapeType.Circle;
            splatterShape.radius = 0.5f;
            
            // Make splatters fade over time
            var splatterColor = splatterPS.colorOverLifetime;
            splatterColor.enabled = true;
            Gradient splatterGradient = new Gradient();
            GradientColorKey[] splatterColorKeys = new GradientColorKey[2];
            splatterColorKeys[0] = new GradientColorKey(new Color(0.6f, 0.03f, 0.03f), 0f);
            splatterColorKeys[1] = new GradientColorKey(new Color(0.3f, 0.01f, 0.01f), 1f);
            GradientAlphaKey[] splatterAlphaKeys = new GradientAlphaKey[2];
            splatterAlphaKeys[0] = new GradientAlphaKey(0.8f, 0f);
            splatterAlphaKeys[1] = new GradientAlphaKey(0f, 1f);
            splatterGradient.SetKeys(splatterColorKeys, splatterAlphaKeys);
            splatterColor.color = splatterGradient;
            
            splatterPS.Play();
            Destroy(splatter, splashDuration * 3f);
        }
    }
    
    public void TriggerBloodSplash()
    {
        TriggerBloodSplash(transform.position, transform.forward);
    }
    
    private void PlayBloodSound()
    {
        if (audioSource != null && bloodSplashSound != null)
        {
            audioSource.clip = bloodSplashSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }
    
    // Public method to create blood splash at specific location
    public static void CreateBloodSplashAt(Vector3 position, Vector3 direction, GameObject bloodEffectPrefab = null)
    {
        if (bloodEffectPrefab == null)
        {
            // Create a comprehensive blood effect if no prefab provided
            GameObject bloodEffect = new GameObject("BloodSplash");
            bloodEffect.transform.position = position;
            bloodEffect.transform.rotation = Quaternion.LookRotation(direction);
            
            // Add particle system and configure it for realistic blood
            ParticleSystem ps = bloodEffect.AddComponent<ParticleSystem>();
            
            // Configure the particle system for realistic blood
            var main = ps.main;
            main.startColor = new Color(0.9f, 0.1f, 0.1f, 1f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.prewarm = false; // Ensure particles start fresh
            
            // Enhanced shape for spray effect
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 60f;
            shape.radius = 0.3f;
            shape.radiusThickness = 0.8f;
            
            // Add gravity and air resistance
            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-7f); // Gravity
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
            
            // Realistic color transition
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(new Color(0.9f, 0.1f, 0.1f), 0f);
            colorKeys[1] = new GradientColorKey(new Color(0.6f, 0.03f, 0.03f), 0.5f);
            colorKeys[2] = new GradientColorKey(new Color(0.3f, 0.01f, 0.01f), 1f);
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(0f, 1f);
            gradient.SetKeys(colorKeys, alphaKeys);
            colorOverLifetime.color = gradient;
            
            // Add rotation
            var rotationOverLifetime = ps.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-90f * Mathf.Deg2Rad, 90f * Mathf.Deg2Rad);
            
            // Add noise for organic movement
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.2f;
            noise.frequency = 0.3f;
            
            // Emission settings - burst mode for instant effect
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0; // No continuous emission
            ParticleSystem.Burst burst = new ParticleSystem.Burst(0.0f, 80); // Instant burst
            emission.SetBursts(new ParticleSystem.Burst[] { burst });
            
            // CRITICAL: Set material BEFORE renderer setup
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                // Try different materials in order of preference
                Material bloodMaterial = null;
                
                // Try Legacy Particles shader first
                Shader particleShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
                if (particleShader != null)
                {
                    bloodMaterial = new Material(particleShader);
                }
                else
                {
                    // Try Mobile Particles shader
                    particleShader = Shader.Find("Mobile/Particles/Alpha Blended");
                    if (particleShader != null)
                    {
                        bloodMaterial = new Material(particleShader);
                    }
                    else
                    {
                        // Try Unlit shader as fallback
                        particleShader = Shader.Find("Unlit/Transparent");
                        if (particleShader != null)
                        {
                            bloodMaterial = new Material(particleShader);
                        }
                        else
                        {
                            // Final fallback
                            bloodMaterial = new Material(Shader.Find("Sprites/Default"));
                        }
                    }
                }
                
                // Set material properties
                if (bloodMaterial != null)
                {
                    bloodMaterial.color = new Color(0.9f, 0.1f, 0.1f, 1f);
                    bloodMaterial.mainTexture = null; // Use default white texture
                    
                    // Force override any purple tints
                    if (bloodMaterial.HasProperty("_TintColor"))
                    {
                        bloodMaterial.SetColor("_TintColor", new Color(0.9f, 0.1f, 0.1f, 1f));
                    }
                    if (bloodMaterial.HasProperty("_Color"))
                    {
                        bloodMaterial.SetColor("_Color", new Color(0.9f, 0.1f, 0.1f, 1f));
                    }
                    
                    renderer.material = bloodMaterial;
                }
                
                renderer.alignment = ParticleSystemRenderSpace.View;
                renderer.sortMode = ParticleSystemSortMode.Distance;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            
            // Add BloodSplashEffect component and trigger immediately
            BloodSplashEffect effect = bloodEffect.AddComponent<BloodSplashEffect>();
            
            // Override settings for immediate effect
            effect.bloodParticleCount = 80;
            effect.splashDuration = 2.5f;
            effect.bloodForce = 15f;
            effect.particleSize = 0.2f;
            effect.enableRealisticDroplets = false; // Disable extra effects for performance
            effect.enableGroundSplatters = false;
            
            // Start the effect immediately
            ps.Play();
            
            // FORCE RED COLOR - override any material issues
            var forceMain = ps.main;
            forceMain.startColor = new Color(0.9f, 0.1f, 0.1f, 1f);
            
            Debug.Log($"Created static blood splash at {position} with proper material");
            
            // Auto-destroy after effect finishes
            Object.Destroy(bloodEffect, 3f);
        }
        else
        {
            GameObject instance = Instantiate(bloodEffectPrefab, position, Quaternion.LookRotation(direction));
            BloodSplashEffect effect = instance.GetComponent<BloodSplashEffect>();
            if (effect != null)
            {
                effect.TriggerBloodSplash(position, direction);
            }
        }
    }
} 