using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

// Simple component to tag the player entity
public struct PlayerTag : IComponentData { }

// Authoring component to add PlayerTag and configure EntityLink
public class PlayerTagAuthoring : MonoBehaviour
{
    // Debug info - проверка в Start
    private void Start()
    {
        Debug.Log($"[PlayerTagAuthoring] Start called on {gameObject.name}");
        
        // Получить или создать EntityLink
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

        // Ручное создание Entity, если бэйкинг не сработал
        if (entityLink.Entity == Entity.Null)
        {
            Debug.LogWarning($"[PlayerTagAuthoring] Entity is null. Attempting manual entity creation...");
            
            // Проверка, что мир ECS существует
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
            {
                var entityManager = world.EntityManager;
                
                // Ручное создание сущности
                var entity = entityManager.CreateEntity();
                
                // Добавить компонент PlayerTag
                entityManager.AddComponentData(entity, new PlayerTag());
                
                // Присвоить созданную сущность компоненту EntityLink
                entityLink.Entity = entity;
                
                Debug.Log($"[PlayerTagAuthoring] Manually created Entity {entity} and assigned to EntityLink on {gameObject.name}");
            }
            else
            {
                Debug.LogError($"[PlayerTagAuthoring] Cannot manually create entity - World.DefaultGameObjectInjectionWorld is null!");
            }
        }
        
        // Попытка найти существующую сущность с PlayerTag (если ручное создание не сработало)
        if (entityLink.Entity == Entity.Null)
        {
            // Пробуем принудительно найти существующую сущность с PlayerTag
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
            {
                var entityManager = world.EntityManager;
                
                // Создадим запрос для поиска сущности с PlayerTag
                var query = entityManager.CreateEntityQuery(typeof(PlayerTag));
                
                if (query.CalculateEntityCount() > 0)
                {
                    var playerEntities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                    Debug.Log($"[PlayerTagAuthoring] Found {playerEntities.Length} entities with PlayerTag");
                    
                    if (playerEntities.Length > 0)
                    {
                        // Использовать первую найденную сущность игрока
                        entityLink.Entity = playerEntities[0];
                        Debug.Log($"[PlayerTagAuthoring] Manually assigned Entity {playerEntities[0]} to EntityLink on {gameObject.name}");
                    }
                    
                    // Освободить память
                    playerEntities.Dispose();
                }
                else
                {
                    Debug.LogWarning($"[PlayerTagAuthoring] No entities with PlayerTag found in the world.");
                }
            }
        }
        
        // Финальная проверка
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