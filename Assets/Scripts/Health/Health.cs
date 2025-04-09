using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int maxHP = 3;
    [SerializeField]
    private int currentHP;

    public UnityEvent<int> OnDamageTaken;
    public UnityEvent OnDeath;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public bool IsAlive => currentHP > 0;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damageAmount)
    {
        if (!IsAlive || damageAmount <= 0)
        {
            return;
        }

        currentHP -= damageAmount;
        currentHP = Mathf.Max(currentHP, 0);

        OnDamageTaken?.Invoke(damageAmount);
        Debug.Log($"{gameObject.name} took {damageAmount} damage. HP left: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} died.");
        // Destroy(gameObject);
    }

    public void Heal(int healAmount)
    {
        if (!IsAlive || healAmount <= 0)
        {
            return;
        }

        currentHP += healAmount;
        currentHP = Mathf.Min(currentHP, maxHP);
    }
} 