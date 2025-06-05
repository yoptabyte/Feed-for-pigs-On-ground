using UnityEngine;
using Unity.Entities;

public class WallTrap : BaseTrap
{
    [Header("Wall Specific Settings")]
    [SerializeField]
    private int damageAmount = 1;
    [SerializeField]
    private float bounceForce = 10f;
    [SerializeField]
    private float speedReductionFactor = 0.5f; // Reduce speed to 50%
    [SerializeField]
    private float speedReductionDuration = 2.0f; // For 2 seconds

    private Collision lastCollision;

    protected override void OnTriggerEnter(Collider other)
    {
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        lastCollision = collision;
        base.OnCollisionEnter(collision);
    }

    protected override void ApplyEffect(GameObject target)
    {
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damageAmount);
        }

        Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();
        if (targetRigidbody != null)
        {   
            if (lastCollision != null && lastCollision.contactCount > 0) 
            {
                ContactPoint contact = lastCollision.contacts[0];
                Vector3 bounceDirection = contact.normal;
                targetRigidbody.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
                Debug.Log($"Applied bounce force to {target.name}");
            }
            else
            {
                Debug.LogWarning($"Could not apply bounce force: lastCollision info is missing or has no contact points.", this);
            }
        }
        else
        {
            Debug.LogWarning($"Target {target.name} does not have a Rigidbody for bounce effect.", this);
        }

        // Apply speed reduction effect to both players and bots
        ApplySpeedReduction(target);

        // Special handling for bot AI interruption
        EnemyPigAI botAI = target.GetComponent<EnemyPigAI>();
        if (botAI != null)
        {
            botAI.InterruptMovement(speedReductionDuration);
            Debug.Log($"Interrupted bot AI movement for {speedReductionDuration} seconds");
        }
    }

    private void ApplySpeedReduction(GameObject target)
    {
        // Try to get EntityLink for status effect system
        EntityLink entityLink = target.GetComponent<EntityLink>();
        if (entityLink != null && entityLink.Entity != Entity.Null)
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            MovementData movementData = target.GetComponent<MovementData>();
            
            if (movementData != null)
            {
                // Create speed reduction effect
                PlayerStatusEffectData slowEffect = new PlayerStatusEffectData
                {
                    Type = EffectType.Slowed,
                    RemainingDuration = speedReductionDuration,
                    EffectStrength = speedReductionFactor,
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

                Debug.Log($"Applied speed reduction effect to {target.name}: {speedReductionFactor}x speed for {speedReductionDuration}s");
            }
        }
        else
        {
            // Fallback for entities without EntityLink - direct MovementData modification
            MovementData movementData = target.GetComponent<MovementData>();
            if (movementData != null)
            {
                movementData.moveSpeed *= speedReductionFactor;
                Debug.Log($"Applied direct speed reduction to {target.name}: reduced to {movementData.moveSpeed}");
            }
        }
    }
} 