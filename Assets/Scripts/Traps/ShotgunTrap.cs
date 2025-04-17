using UnityEngine;
using System.Collections.Generic;

public class ShotgunTrap : BaseTrap
{
    [Header("Shotgun Specific Settings")]
    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private Transform shootPoint;
    [SerializeField]
    private float projectileSpeed = 20f;
    [SerializeField]
    private int pelletCount = 5;
    [SerializeField]
    private float spreadAngle = 120f;

    [Header("Tripwire Link")]
    [Tooltip("List of tripwires that activate this shotgun.")]
    [SerializeField]
    private List<Tripwire> linkedTripwires = new List<Tripwire>();

    protected override void Awake()
    {
        base.Awake();
        foreach (var tripwire in linkedTripwires)
        {
            if (tripwire != null)
            {
                tripwire.OnTripwireTriggered.AddListener(ExternalTrigger);
            }
            else
            {
                Debug.LogWarning($"Null tripwire linked in {gameObject.name}", this);
            }
        }
        if (projectilePrefab == null)
        {
            Debug.LogError($"Projectile Prefab is not assigned in {gameObject.name}!", this);
        }
        if (shootPoint == null)
        {
            Debug.LogWarning($"Shoot Point is not assigned in {gameObject.name}. Using trap's position.", this);
            shootPoint = transform;
        }
    }

    public void ExternalTrigger(GameObject triggerTarget)
    {
        base.TriggerTrap(triggerTarget);
    }

    protected override void ApplyEffect(GameObject target)
    {
        Shoot();
    }

    private void Shoot()
    {
        if (projectilePrefab == null) return;

        Debug.Log($"{gameObject.name} shooting!");

        for (int i = 0; i < pelletCount; i++)
        {
            Quaternion spreadRotation = Quaternion.Euler(Random.Range(-spreadAngle / 2, spreadAngle / 2), Random.Range(-spreadAngle / 2, spreadAngle / 2), 0);
            Vector3 direction = spreadRotation * shootPoint.forward;

            GameObject projectileGO = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            Projectile projectile = projectileGO.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.Initialize(direction * projectileSpeed);
            }
            else
            {
                Debug.LogError($"Projectile Prefab {projectilePrefab.name} is missing Projectile component!", this);
                Destroy(projectileGO);
            }
        }
    }

    protected override void ResetTrap()
    {
        base.ResetTrap();
        foreach (var tripwire in linkedTripwires)
        {
            if (tripwire != null)
            {
                tripwire.ResetTripwire();
            }
        }
    }

    protected override void OnTriggerEnter(Collider other) { }
    protected override void OnCollisionEnter(Collision collision) { }
} 