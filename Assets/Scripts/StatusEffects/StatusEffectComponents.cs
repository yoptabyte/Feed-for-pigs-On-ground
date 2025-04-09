using Unity.Entities;

public enum EffectType
{
    None,
    Bounced,        // Отскок (стена)
    Slowed,         // Замедление (разбрызгиватель)
    Slipping,       // Скольжение (навоз)
    Immobilized     // Обездвиживание (капкан)
}

// Компонент, временно добавляемый сущности (игроку)
// при срабатывании ловушки с эффектом
public struct PlayerStatusEffectData : IComponentData
{
    public EffectType Type;
    public float RemainingDuration;

    // Опционально: специфичные данные для эффекта
    // Например, сила отскока для Bounced
    public float EffectStrength; // Может использоваться по-разному в зависимости от Type
}