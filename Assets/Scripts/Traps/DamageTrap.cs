using UnityEngine;

public class DamageTrap : BaseTrap
{
    [Header("Damage Settings")]
    [SerializeField]
    private int damageAmount = 10;
    [SerializeField]
    private bool instantKill = false; 
    
    protected override void ApplyEffect(GameObject target)
    {
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            int damageToApply = instantKill ? targetHealth.MaxHP : damageAmount;
            targetHealth.TakeDamage(damageToApply);
        }
        else
        {
            Debug.LogWarning($"Target {target.name} does not have a Health component.", this);
        }
    }
} 