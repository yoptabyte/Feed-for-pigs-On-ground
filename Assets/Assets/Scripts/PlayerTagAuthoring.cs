using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

// Simple component to tag the player entity
public struct PlayerTag : IComponentData { }

// Authoring component to add PlayerTag and configure EntityLink
public class PlayerTagAuthoring : MonoBehaviour
{
    // Debug info - check in Start
    private bool debugInfo = true;

    // Debug info - check in Start
    private void Start()
    {
        Debug.Log($"[PlayerTagAuthoring] Start called on {gameObject.name}");
        
        // Get or create EntityLink
        var entityLink = GetComponent<EntityLink>();
        if (entityLink == null)
        {
            Debug.LogWarning($"[PlayerTagAuthoring] Start - No EntityLink found on {gameObject.name}! Creating one manually...");
            entityLink = gameObject.AddComponent<EntityLink>();
        }
        else
        {
            Debug.Log($"[PlayerTagAuthoring] Start - EntityLink present on {gameObject.name}. Entity: {entityLink.Entity}. Is Entity.Null? {entityLink.Entity == Entity.Null}");
        }

        // Manual Entity creation if baking didn't work
        if (entityLink.Entity == Entity.Null)
        {
            Debug.LogWarning($"[PlayerTagAuthoring] Entity is null. Attempting manual entity creation...");
            
            // Check that the ECS world exists
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
            {
                var entityManager = world.EntityManager;
                
                // Manual entity creation
                var entity = entityManager.CreateEntity();
                
                // Add PlayerTag component
                entityManager.AddComponentData(entity, new PlayerTag());
                
                // Assign created entity to EntityLink component
                entityLink.Entity = entity;
                
                Debug.Log($"[PlayerTagAuthoring] Manually created Entity {entity} and assigned to EntityLink on {gameObject.name}");
            }
            else
            {
                Debug.LogError($"[PlayerTagAuthoring] Cannot manually create entity - World.DefaultGameObjectInjectionWorld is null!");
            }
        }
        
        // Try to find existing entity with PlayerTag (if manual creation didn't work)
        if (entityLink.Entity == Entity.Null)
        {
            // Try to force find existing entity with PlayerTag
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
            {
                var entityManager = world.EntityManager;
                
                // Create query to search for entity with PlayerTag
                var query = entityManager.CreateEntityQuery(typeof(PlayerTag));
                
                if (query.CalculateEntityCount() > 0)
                {
                    var playerEntities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                    Debug.Log($"[PlayerTagAuthoring] Found {playerEntities.Length} entities with PlayerTag");
                    
                    if (playerEntities.Length > 0)
                    {
                        // Use first found player entity
                        entityLink.Entity = playerEntities[0];
                        Debug.Log($"[PlayerTagAuthoring] Manually assigned Entity {playerEntities[0]} to EntityLink on {gameObject.name}");
                    }
                    
                    // Free memory
                    playerEntities.Dispose();
                }
                else
                {
                    Debug.LogWarning($"[PlayerTagAuthoring] No entities with PlayerTag found in the world.");
                }
            }
        }
        
        // Final check
        if (entityLink.Entity == Entity.Null)
        {
            Debug.LogError($"[PlayerTagAuthoring] Failed to obtain a valid Entity for {gameObject.name}. Pickup may still not work!");
        }
        else
        {
            Debug.Log($"[PlayerTagAuthoring] Final Entity.Null check: {entityLink.Entity == Entity.Null}. Entity: {entityLink.Entity}");
        }
    }

    // Baker runs during conversion from GameObject to Entity
    private class PlayerTagBaker : Baker<PlayerTagAuthoring>
    {
        public override void Bake(PlayerTagAuthoring authoring)
        {
            Debug.Log($"[PlayerTagBaker] BAKING STARTED for {authoring.gameObject.name}");
            
            // Get the entity being created for this GameObject
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            Debug.Log($"[PlayerTagBaker] Got entity {entity} for {authoring.gameObject.name}");
            
            // Add the PlayerTag component to the entity
            AddComponent<PlayerTag>(entity);
            
            Debug.Log($"[PlayerTagBaker] Added PlayerTag to entity {entity}");

            // Ensure EntityLink component exists on the GameObject 
            // and assign the created entity reference to it.
            // This allows MonoBehaviour scripts (like PickupItem) to find the entity.
            var entityLink = authoring.gameObject.GetComponent<EntityLink>();
            
            if (entityLink == null)
            {
                Debug.Log($"[PlayerTagBaker] EntityLink component not found on {authoring.gameObject.name}, adding one...");
                entityLink = authoring.gameObject.AddComponent<EntityLink>();
                Debug.Log($"[PlayerTagBaker] EntityLink component added to {authoring.gameObject.name}");
            }
            else
            {
                Debug.Log($"[PlayerTagBaker] EntityLink component already exists on {authoring.gameObject.name}");
            }
            
            // Set the entity reference
            entityLink.Entity = entity;
            
            Debug.Log($"[PlayerTagBaker] Set EntityLink.Entity = {entity} on {authoring.gameObject.name}. Is Entity.Null? {entity == Entity.Null}");
        }
    }
} 