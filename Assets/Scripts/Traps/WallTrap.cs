using UnityEngine;

public class WallTrap : BaseTrap
{
    [Header("Wall Specific Settings")]
    [SerializeField]
    private int damageAmount = 1;
    [SerializeField]
    private float bounceForce = 10f;
    // [SerializeField]
    // private float speedReductionFactor = 0.2f;
    // [SerializeField]
    // private float speedReductionDuration = 1.0f;

    private Collision lastCollision;

    protected override void OnTriggerEnter(Collider other)
    {
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        lastCollision = collision;
        base.OnCollisionEnter(collision);
    }

    protected override void ApplyEffect(GameObject target)
    {
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damageAmount);
        }

        Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();
        if (targetRigidbody != null)
        {   
            if (lastCollision != null && lastCollision.contactCount > 0) 
            {
                ContactPoint contact = lastCollision.contacts[0];
                Vector3 bounceDirection = contact.normal;
                targetRigidbody.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
                Debug.Log($"Applied bounce force to {target.name}");
            }
            else
            {
                Debug.LogWarning($"Could not apply bounce force: lastCollision info is missing or has no contact points.", this);
            }
        }
        else
        {
            Debug.LogWarning($"Target {target.name} does not have a Rigidbody for bounce effect.", this);
        }

        // PlayerMovementController movementController = target.GetComponent<PlayerMovementController>();
        // if (movementController != null)
        // {
        //     movementController.ApplySpeedReduction(speedReductionFactor, speedReductionDuration);
        // }
    }
} 