using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public static class EntityMonoBehaviourBridge
{
    public static T GetMonoBehaviourComponent<T>(EntityManager entityManager, Entity entity) where T : Component
    {
        var allEntityLinks = GameObject.FindObjectsOfType<EntityLink>();
        
        foreach (var link in allEntityLinks)
        {
            if (link.Entity == entity)
            {
                var component = link.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
                
                component = link.GetComponentInChildren<T>();
                if (component != null)
                {
                    return component;
                }
                
                component = link.GetComponentInParent<T>();
                if (component != null)
                {
                    return component;
                }
                
                Debug.LogWarning($"Found GameObject '{link.gameObject.name}' linked to Entity {entity}, but it doesn't have component {typeof(T)}");
                return null;
            }
        }
        
        if (entityManager.HasComponent<EntityLink>(entity))
        {
            try
            {
                var entityLink = entityManager.GetComponentObject<EntityLink>(entity);
                if (entityLink != null && entityLink.gameObject != null)
                {
                    return entityLink.gameObject.GetComponent<T>();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error getting EntityLink: {e.Message}");
            }
        }
        
        Debug.LogError($"Cannot find MonoBehaviour component {typeof(T)} for entity {entity}. Make sure the GameObject has both EntityLink and {typeof(T)} components.");
        return null;
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class PlayerStatusEffectSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _ecbSystem.CreateCommandBuffer();
        float dt = SystemAPI.Time.DeltaTime;
        var entityManager = EntityManager;

        Entities
            .WithAll<PlayerStatusEffectData>()
            .WithoutBurst()
            .ForEach((Entity entity, ref PlayerStatusEffectData effect) =>
            {
                bool effectExpired = false;
                effect.RemainingDuration -= dt;
                if (effect.RemainingDuration <= 0)
                {
                    effectExpired = true;
                }
                
                var movementData = EntityMonoBehaviourBridge.GetMonoBehaviourComponent<MovementData>(entityManager, entity);
                
                Health health = null;
                if (effect.Type == EffectType.Regeneration)
                {
                    health = EntityMonoBehaviourBridge.GetMonoBehaviourComponent<Health>(entityManager, entity);
                    if (health == null)
                    {
                        Debug.LogWarning($"Effect {effect.Type} requires Health component, but none was found on entity {entity}. Effect will not be applied.");
                    }
                }

                if (!effectExpired)
                {
                    switch (effect.Type)
                    {
                        case EffectType.Regeneration:
                            if (health != null && health.CanRegenerate())
                            {
                                health.Heal(1);
                            }
                            break;

                        case EffectType.SpeedBoost:
                            if (movementData != null)
                            {
                                movementData.moveSpeed = effect.OriginalValue * effect.EffectStrength;
                                float baseMaxSpeed = movementData.moveSpeed * 1.5f;
                                movementData.maxSpeed = baseMaxSpeed;
                                Debug.Log($"StatusEffect: SpeedBoost applied. New speed: {movementData.moveSpeed} (base: {effect.OriginalValue}, multiplier: {effect.EffectStrength})");
                            }
                            else
                            {
                                Debug.LogWarning($"Effect {effect.Type} requires MovementData component, but none was found on entity {entity}.");
                            }
                            break;

                        case EffectType.TurnSpeedBoost:
                            if (movementData != null)
                            {
                                movementData.rotationSpeed = effect.OriginalValue * effect.EffectStrength;
                            }
                            else
                            {
                                Debug.LogWarning($"Effect {effect.Type} requires MovementData component, but none was found on entity {entity}.");
                            }
                            break;

                        case EffectType.JumpHeightBoost:
                            if (movementData != null)
                            {
                                movementData.jumpForce = effect.OriginalValue * effect.EffectStrength;
                            }
                            else
                            {
                                Debug.LogWarning($"Effect {effect.Type} requires MovementData component, but none was found on entity {entity}.");
                            }
                            break;

                    }
                }

                if (effectExpired)
                {
                    if (movementData != null)
                    {
                        switch (effect.Type)
                        {
                            case EffectType.SpeedBoost:
                                if (Mathf.Approximately(movementData.moveSpeed, effect.OriginalValue * effect.EffectStrength))
                                {
                                    movementData.moveSpeed = effect.OriginalValue;
                                }
                                break;
                            case EffectType.TurnSpeedBoost:
                                if (Mathf.Approximately(movementData.rotationSpeed, effect.OriginalValue * effect.EffectStrength))
                                {
                                    movementData.rotationSpeed = effect.OriginalValue;
                                }
                                break;
                            case EffectType.JumpHeightBoost:
                                if (Mathf.Approximately(movementData.jumpForce, effect.OriginalValue * effect.EffectStrength))
                                {
                                    movementData.jumpForce = effect.OriginalValue;
                                }
                                break;
                        }
                    }

                    ecb.RemoveComponent<PlayerStatusEffectData>(entity);
                    Debug.Log($"Effect {effect.Type} expired for entity {entity}");
                }

            }).Run();

        _ecbSystem.AddJobHandleForProducer(this.Dependency);
    }
}