using Unity.Entities;

public enum EffectType
{
    None,
    Bounced,        
    Slowed,         
    Slipping,       
    Immobilized,    
    Regeneration,   
    SpeedBoost,     
    TurnSpeedBoost, 
    JumpHeightBoost 
}



public struct PlayerStatusEffectData : IComponentData
{
    public EffectType Type;
    public float RemainingDuration;

    
    
    public float EffectStrength; 
    public float OriginalValue; 
}