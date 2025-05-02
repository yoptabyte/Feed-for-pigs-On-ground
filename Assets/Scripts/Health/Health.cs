using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Health : MonoBehaviour
{
    [SerializeField]
    private int maxHP = 3;
    [SerializeField]
    private int currentHP;
    [SerializeField]
    private float regenerationRate = 0.5f; 

    public float timeSinceLastDamage = float.MaxValue;
    public float regenerationDelay = 3.0f; 

    private float regenerationProgress = 0f;

    private bool canStartRegeneration = false; 

    private Vector3 originalScale;
    private Coroutine flattenCoroutine;

    public UnityEvent<int> OnDamageTaken;
    public UnityEvent OnDeath;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public bool IsAlive => currentHP > 0;

    private void Awake()
    {
        currentHP = maxHP;
        timeSinceLastDamage = float.MaxValue;
        regenerationProgress = 0f;
        canStartRegeneration = false; 
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (IsAlive && canStartRegeneration && timeSinceLastDamage < regenerationDelay)
        {
          timeSinceLastDamage += Time.deltaTime;
        }

        if (CanRegenerate())
        {
            regenerationProgress += regenerationRate * Time.deltaTime;
            if (regenerationProgress >= 1.0f)
            {
              int healAmountInt = Mathf.FloorToInt(regenerationProgress);
              Heal(healAmountInt);
              regenerationProgress -= healAmountInt; 
            }
        }
        else
        {
          regenerationProgress = 0f;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (!IsAlive || damageAmount <= 0)
        {
          return;
        }

        currentHP -= damageAmount;
        currentHP = Mathf.Max(currentHP, 0);

        timeSinceLastDamage = 0f;
        canStartRegeneration = false; 
        regenerationProgress = 0f;    

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
        if (!IsAlive || healAmount <= 0 || currentHP >= maxHP)
        {
            // regenerationProgress = 0f;
            return;
        }

        int oldHP = currentHP;
        currentHP += healAmount;
        currentHP = Mathf.Min(currentHP, maxHP);
        Debug.Log($"{gameObject.name} healed {currentHP - oldHP}. HP: {currentHP}"); 

        if (currentHP >= maxHP)
        {
             regenerationProgress = 0f;
             canStartRegeneration = false;
        }
    }

    public void EnableRegeneration()
    {
        if (IsAlive && currentHP < maxHP) 
        {
            canStartRegeneration = true;
            timeSinceLastDamage = 0f; 
            Debug.Log($"{gameObject.name} regeneration enabled. Delay timer started.");
        }
    }

    public bool CanRegenerate()
    {
        return canStartRegeneration && IsAlive && timeSinceLastDamage >= regenerationDelay && currentHP < maxHP;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Vehicle"))
        {
            if (flattenCoroutine != null)
            {
                StopCoroutine(flattenCoroutine);
            }
            flattenCoroutine = StartCoroutine(FlattenEffect(5.0f));
        }
    }

    private IEnumerator FlattenEffect(float duration)
    {
        Debug.Log($"{gameObject.name} entered Vehicle trigger. Starting flatten effect for {duration} seconds.");
        
        transform.localScale = new Vector3(originalScale.x, 0.1f, originalScale.z);
        
        TakeDamage(1);
        
        yield return new WaitForSeconds(duration);
        
        transform.localScale = originalScale;
        Debug.Log($"{gameObject.name} flatten effect finished. Restored original scale.");
        
        flattenCoroutine = null;
    }
}