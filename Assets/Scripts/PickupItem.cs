using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Events;
// using StatusEffects;
// using HealthNamespace;


[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour
{
    public EffectType effectType = EffectType.None;
    public float duration = 10.0f;
    public float strength = 1.5f;
    public bool stackable = false;
    public float maxStackDuration = 30.0f;

    [Header("Animation Settings")]
    public float bounceHeight = 0.1f; 
    public float bounceSpeed = 2f;    
    public float rotationSpeed = 50f; 
    public Vector3 rotationAxis = Vector3.up; 

    private Vector3 initialPosition;
    private float randomOffset;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError($"PickupItem on {gameObject.name} requires a Collider component!");
        }

        initialPosition = transform.position;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    private void Update()
    {
        float newY = initialPosition.y + Mathf.Sin((Time.time + randomOffset) * bounceSpeed) * bounceHeight;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Entity playerEntity = GetPlayerEntity(other.gameObject);

        if (playerEntity == Entity.Null || effectType == EffectType.None)
        {
            if (playerEntity == Entity.Null) {
                Debug.LogWarning($"PickupItem: No player Entity found for collider {other.name}. Pickup aborted.");
            }
            return;
        }

        MovementData movementData = other.GetComponent<MovementData>();
        if (movementData == null)
        {
            movementData = other.GetComponentInParent<MovementData>();
        }

        if (movementData == null && (effectType == EffectType.SpeedBoost || effectType == EffectType.TurnSpeedBoost || effectType == EffectType.JumpHeightBoost))
        {
            Debug.LogError($"PickupItem: MovementData component not found on {other.name} or its parents, but is required for effect {effectType}.");
        }

        ApplyEffect(playerEntity, movementData);

        Debug.Log($"Destroying pickup: {gameObject.name} after applying effect {effectType}");
        Destroy(gameObject);
    }

    private Entity GetPlayerEntity(GameObject playerObject)
    {
        Debug.Log($"[GetPlayerEntity] Checking object: {playerObject.name} (Layer: {LayerMask.LayerToName(playerObject.layer)}, InstanceID: {playerObject.GetInstanceID()}) for EntityLink.");

        EntityLink entityLink = playerObject.GetComponent<EntityLink>();
        if (entityLink != null)
        {
            Debug.Log($"[GetPlayerEntity] Found EntityLink directly on {playerObject.name}. Returning Entity: {entityLink.Entity}");
            return entityLink.Entity;
        }
        else
        {
            Debug.Log($"[GetPlayerEntity] EntityLink not found directly on {playerObject.name}. Checking parents...");
            entityLink = playerObject.GetComponentInParent<EntityLink>();
            if (entityLink != null)
            {
                Debug.Log($"[GetPlayerEntity] Found EntityLink via GetComponentInParent on object: {entityLink.gameObject.name} (InstanceID: {entityLink.gameObject.GetInstanceID()}). Returning Entity: {entityLink.Entity}");
                return entityLink.Entity;
            }
            else
            {
                 Debug.LogWarning($"[GetPlayerEntity] Could not find EntityLink component using GetComponent or GetComponentInParent on the object hierarchy starting from {playerObject.name}.");
            }
        }

        // Debug.LogWarning($"Could not get Player Entity from GameObject {playerObject.name}.");
        return Entity.Null;
    }

    private void ApplyEffect(Entity playerEntity, MovementData movementData)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Health playerHealth = null;
        if (effectType == EffectType.Regeneration)
        {
            GameObject playerObject = movementData?.gameObject;

            if (playerObject == null)
            {
                 EntityLink[] allEntityLinks = FindObjectsOfType<EntityLink>();
                 foreach (var link in allEntityLinks)
                 {
                     if (link.Entity == playerEntity)
                     {
                        playerObject = link.gameObject;
                        break;
                     }
                 }
            }

            if (playerObject != null)
            {
                playerHealth = playerObject.GetComponent<Health>();
                if (playerHealth == null)
                {
                     playerHealth = playerObject.GetComponentInParent<Health>();
                }

                if (playerHealth == null)
                {
                    Debug.LogWarning($"PickupItem: Health component not found on player object {playerObject.name} (Entity: {playerEntity}) for Regeneration effect.");
                }
            }
             else
            {
                Debug.LogError($"PickupItem: Could not find player GameObject for Entity {playerEntity} to get Health component.");
            }
        }

        float originalValue = 0f;
        if (effectType == EffectType.SpeedBoost || effectType == EffectType.TurnSpeedBoost || effectType == EffectType.JumpHeightBoost)
        {
            if (movementData != null)
            {
                switch (effectType)
                {
                    case EffectType.SpeedBoost: originalValue = movementData.moveSpeed; break;
                    case EffectType.TurnSpeedBoost: originalValue = movementData.rotationSpeed; break;
                    case EffectType.JumpHeightBoost: originalValue = movementData.jumpForce; break;
                }
                 Debug.Log($"PickupItem: Storing original value {originalValue} for effect {effectType}");
            }
            else
            {
                 Debug.LogError($"PickupItem: Cannot store original value for {effectType} because MovementData component was not found on the player object.");
            }
        }

        bool hadExistingEffect = false;
        PlayerStatusEffectData effectToRestoreFrom = default;

        if (entityManager.HasComponent<PlayerStatusEffectData>(playerEntity))
        {
            hadExistingEffect = true;
            PlayerStatusEffectData existingEffect = entityManager.GetComponentData<PlayerStatusEffectData>(playerEntity);
            effectToRestoreFrom = existingEffect;

            if (existingEffect.Type == effectType)
            {
                if (stackable)
                {
                    existingEffect.RemainingDuration = Mathf.Min(existingEffect.RemainingDuration + duration, maxStackDuration);
                    entityManager.SetComponentData(playerEntity, existingEffect);
                    Debug.Log($"Stacked {effectType} on entity {playerEntity}. New duration: {existingEffect.RemainingDuration}");
                }
                else
                {
                    RestoreValueFromEffect(entityManager, playerEntity, existingEffect);

                    existingEffect.RemainingDuration = duration;
                    existingEffect.EffectStrength = strength;
                    existingEffect.OriginalValue = originalValue;
                    entityManager.SetComponentData(playerEntity, existingEffect);
                    Debug.Log($"Refreshed {effectType} on entity {playerEntity}. Duration: {duration}, OriginalValue: {originalValue}");
                }
            }
            else
            {
                 RestoreValueFromEffect(entityManager, playerEntity, existingEffect);

                PlayerStatusEffectData newEffect = new PlayerStatusEffectData
                {
                    Type = effectType,
                    RemainingDuration = duration,
                    EffectStrength = strength,
                    OriginalValue = originalValue
                };
                entityManager.SetComponentData(playerEntity, newEffect);
                Debug.Log($"Replaced effect {existingEffect.Type} with {effectType} on entity {playerEntity}. Duration: {duration}, OriginalValue: {originalValue}");
            }
        }
        else
        {
            PlayerStatusEffectData newEffect = new PlayerStatusEffectData
            {
                Type = effectType,
                RemainingDuration = duration,
                EffectStrength = strength,
                OriginalValue = originalValue
            };
            entityManager.AddComponentData(playerEntity, newEffect);
            Debug.Log($"Applied new {effectType} to entity {playerEntity}. Duration: {duration}, OriginalValue: {originalValue}");
        }

        if (effectType == EffectType.Regeneration && playerHealth != null)
        {
            playerHealth.EnableRegeneration();
            Debug.Log($"PickupItem: Called EnableRegeneration() on {playerHealth.gameObject.name} for entity {playerEntity}.");
        }

        if (movementData != null && (effectType == EffectType.SpeedBoost || effectType == EffectType.TurnSpeedBoost || effectType == EffectType.JumpHeightBoost))
        {
             ApplyModification(movementData, effectType, strength, originalValue);
        }
        else if (movementData == null && (effectType == EffectType.SpeedBoost || effectType == EffectType.TurnSpeedBoost || effectType == EffectType.JumpHeightBoost))
        {
             Debug.LogWarning($"PickupItem: Skipping immediate modification for {effectType} because MovementData was not found.");
        }
    }

    private void ApplyModification(MovementData movData, EffectType type, float effectStrength, float originalValue)
    {
        switch (type)
        {
             case EffectType.SpeedBoost:
                 if (originalValue > 0)
                 {
                    movData.moveSpeed = originalValue * effectStrength;
                    Debug.Log($"ApplyModification: Set SpeedBoost. New Speed: {movData.moveSpeed} (Original: {originalValue}, Strength: {effectStrength})");
                 } else {
                    Debug.LogWarning($"ApplyModification: Cannot apply SpeedBoost modifier because original value ({originalValue}) is invalid.");
                 }
                 break;
             case EffectType.TurnSpeedBoost:
                 if (originalValue > 0)
                 {
                    movData.rotationSpeed = originalValue * effectStrength;
                     Debug.Log($"ApplyModification: Set TurnSpeedBoost. New Turn Speed: {movData.rotationSpeed} (Original: {originalValue}, Strength: {effectStrength})");
                 } else {
                     Debug.LogWarning($"ApplyModification: Cannot apply TurnSpeedBoost modifier because original value ({originalValue}) is invalid.");
                 }
                 break;
             case EffectType.JumpHeightBoost:
                  if (originalValue > 0)
                 {
                     movData.jumpForce = originalValue * effectStrength;
                     Debug.Log($"ApplyModification: Set JumpHeightBoost. New Jump Force: {movData.jumpForce} (Original: {originalValue}, Strength: {effectStrength})");
                 } else {
                     Debug.LogWarning($"ApplyModification: Cannot apply JumpHeightBoost modifier because original value ({originalValue}) is invalid.");
                 }
                 break;
        }
    }

    private void RestoreValueFromEffect(EntityManager entityManager, Entity entity, PlayerStatusEffectData effectToRestoreFrom)
    {
        MovementData movementData = EntityMonoBehaviourBridge.GetMonoBehaviourComponent<MovementData>(entityManager, entity);
        // var healthData = EntityMonoBehaviourBridge.GetMonoBehaviourComponent<Health>(entityManager, entity); // Example for other types

        if (movementData == null && (effectToRestoreFrom.Type == EffectType.SpeedBoost || effectToRestoreFrom.Type == EffectType.TurnSpeedBoost || effectToRestoreFrom.Type == EffectType.JumpHeightBoost))
        {
            Debug.LogError($"RestoreValueFromEffect: Cannot restore {effectToRestoreFrom.Type} because MovementData component was not found on the player entity {entity} via bridge.");
            return;
        }

        switch (effectToRestoreFrom.Type)
        {
            case EffectType.SpeedBoost:
                if (movementData != null && Mathf.Approximately(movementData.moveSpeed, effectToRestoreFrom.OriginalValue * effectToRestoreFrom.EffectStrength))
                {
                    movementData.moveSpeed = effectToRestoreFrom.OriginalValue;
                    Debug.Log($"RestoreValueFromEffect: Restored SpeedBoost. Original Speed: {movementData.moveSpeed}");
                }
                else if (movementData != null)
                {
                     Debug.LogWarning($"RestoreValueFromEffect: Did not restore SpeedBoost. Current speed ({movementData.moveSpeed}) does not match expected modified value ({effectToRestoreFrom.OriginalValue * effectToRestoreFrom.EffectStrength}). Original intended: {effectToRestoreFrom.OriginalValue}");
                }
                break;

            case EffectType.TurnSpeedBoost:
                if (movementData != null && Mathf.Approximately(movementData.rotationSpeed, effectToRestoreFrom.OriginalValue * effectToRestoreFrom.EffectStrength))
                {
                    movementData.rotationSpeed = effectToRestoreFrom.OriginalValue;
                    Debug.Log($"RestoreValueFromEffect: Restored TurnSpeedBoost. Original Turn Speed: {movementData.rotationSpeed}");
                }
                else if (movementData != null)
                {
                    Debug.LogWarning($"RestoreValueFromEffect: Did not restore TurnSpeedBoost. Current turn speed ({movementData.rotationSpeed}) does not match expected modified value ({effectToRestoreFrom.OriginalValue * effectToRestoreFrom.EffectStrength}). Original intended: {effectToRestoreFrom.OriginalValue}");
                }
                break;

            case EffectType.JumpHeightBoost:
                if (movementData != null && Mathf.Approximately(movementData.jumpForce, effectToRestoreFrom.OriginalValue * effectToRestoreFrom.EffectStrength))
                {
                    movementData.jumpForce = effectToRestoreFrom.OriginalValue;
                    Debug.Log($"RestoreValueFromEffect: Restored JumpHeightBoost. Original Jump Force: {movementData.jumpForce}");
                }
                else if (movementData != null)
                {
                    Debug.LogWarning($"RestoreValueFromEffect: Did not restore JumpHeightBoost. Current jump force ({movementData.jumpForce}) does not match expected modified value ({effectToRestoreFrom.OriginalValue * effectToRestoreFrom.EffectStrength}). Original intended: {effectToRestoreFrom.OriginalValue}");
                }
                break;
        }
    }

}

// public struct PlayerTag : IComponentData { }

// Moved PlayerTagAuthoring and PlayerTagBaker to PlayerTagAuthoring.cs
/*
public class PlayerTagAuthoring : MonoBehaviour
{
    public class PlayerTagBaker : Baker<PlayerTagAuthoring>
    {
        public override void Bake(PlayerTagAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerTag>(entity);
            
            var entityLink = authoring.gameObject.AddComponent<EntityLink>();
            entityLink.Entity = entity;
        }
    }
}
*/