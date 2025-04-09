using Unity.Entities;

// TODO: Реализовать логику системы

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

        // Логика обработки PlayerStatusEffectData
        // - Уменьшение RemainingDuration
        // - Применение эффектов к MovementData игрока (или других сущностей)
        //   - Отскок (может быть применен один раз при добавлении компонента)
        //   - Модификация скорости/трения
        //   - Запрет движения (Immobilized)
        // - Удаление компонента, когда RemainingDuration <= 0
        // - Восстановление нормальных параметров MovementData после удаления
    }
} 