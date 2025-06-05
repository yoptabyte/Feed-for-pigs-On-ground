using UnityEngine;

public abstract class BaseTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField]
    protected float cooldown = 2.0f;
    [SerializeField]
    protected bool startsActive = true;

    protected bool isActive;
    protected float timeUntilActive;

    protected virtual void Awake()
    {
        isActive = startsActive;
        timeUntilActive = startsActive ? 0f : cooldown;
    }

    protected virtual void Update()
    {
        if (!isActive)
        {
            timeUntilActive -= Time.deltaTime;
            if (timeUntilActive <= 0)
            {
                ResetTrap();
            }
        }
    }

    protected virtual void TriggerTrap(GameObject target)
    {
        if (!isActive) return;

        Debug.Log($"🪤 {gameObject.name} triggered by {target.name}");
        ApplyEffect(target);
        isActive = false;
        timeUntilActive = cooldown;
    }

    protected abstract void ApplyEffect(GameObject target);

    protected virtual void ResetTrap()
    {
        isActive = true;
        timeUntilActive = 0f;
        Debug.Log($"🪤 {gameObject.name} reset.");
    }

    // Check if target is valid for trap activation
    protected virtual bool IsValidTarget(GameObject target)
    {
        // Check for Health component first
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsAlive)
        {
            return true;
        }
        
        // If no Health component, check if it's a bot that should have one
        EnemyPigAI botAI = target.GetComponent<EnemyPigAI>();
        if (botAI != null)
        {
            Debug.Log($"🪤 Found bot {target.name} without Health component, will trigger trap anyway");
            return true;
        }
        
        // Check for player tag as fallback
        if (target.CompareTag("Player"))
        {
            Debug.Log($"🪤 Found player {target.name}, will trigger trap");
            return true;
        }
        
        return false;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🪤 {gameObject.name}: OnTriggerEnter with {other.name}");
        if (IsValidTarget(other.gameObject))
        {
            TriggerTrap(other.gameObject);
        }
        else
        {
            Debug.Log($"🪤 {gameObject.name}: {other.name} is not a valid target");
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"🪤 {gameObject.name}: OnCollisionEnter with {collision.gameObject.name}");
        if (IsValidTarget(collision.gameObject))
        {
            TriggerTrap(collision.gameObject);
        }
        else
        {
            Debug.Log($"🪤 {gameObject.name}: {collision.gameObject.name} is not a valid target");
        }
    }
} 