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

        Debug.Log($"{gameObject.name} triggered by {target.name}");
        ApplyEffect(target);
        isActive = false;
        timeUntilActive = cooldown;
    }

    protected abstract void ApplyEffect(GameObject target);

    protected virtual void ResetTrap()
    {
        isActive = true;
        timeUntilActive = 0f;
        Debug.Log($"{gameObject.name} reset.");
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsAlive)
        {
            TriggerTrap(other.gameObject);
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        Health targetHealth = collision.gameObject.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsAlive)
        {
            TriggerTrap(collision.gameObject);
        }
    }
} 