using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField]
    private float lifetime = 5.0f;
    [SerializeField]
    private int damage = 1;
    // [SerializeField]
    // private GameObject hitEffectPrefab;
    
    private Rigidbody rb;
    private float currentLifetime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentLifetime = lifetime;
    }

    void Update()
    {
        currentLifetime -= Time.deltaTime;
        if (currentLifetime <= 0)
        {   
            DestroyProjectile();
        }
    }

    public void Initialize(Vector3 initialVelocity)
    {
        if (rb != null)
        {
            rb.linearVelocity = initialVelocity;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsAlive)
        {
            targetHealth.TakeDamage(damage);
            Debug.Log($"Projectile hit trigger on {other.gameObject.name} for {damage} damage.");
        }
        else if (!other.isTrigger)
        {
            Debug.Log($"Projectile hit trigger on non-target: {other.gameObject.name}");
            DestroyProjectile();
        }
        
        if (targetHealth != null)
        {
            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
} 