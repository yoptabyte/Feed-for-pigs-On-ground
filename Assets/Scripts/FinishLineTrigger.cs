using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FinishLineTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private bool triggerOnce = true; // Prevent multiple triggers
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private bool hasTriggered = false;
    
    void Awake()
    {
        // Ensure the collider is set as trigger
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
        else
        {
            Debug.LogError("FinishLineTrigger requires a Collider component!");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if we should trigger only once and already triggered
        if (triggerOnce && hasTriggered)
            return;
        
        // Check if FinishLineController exists
        if (FinishLineController.Instance == null)
        {
            Debug.LogWarning("FinishLineTrigger: FinishLineController instance not found!");
            return;
        }
        
        // Check if game is still active
        if (!FinishLineController.Instance.IsGameActive())
            return;
        
        // Handle player finish
        if (other.CompareTag(playerTag))
        {
            if (showDebugInfo)
            {
                Debug.Log($"FinishLineTrigger: Player '{other.name}' crossed the finish line!");
            }
            
            FinishLineController.Instance.OnPlayerFinish();
            
            if (triggerOnce)
            {
                hasTriggered = true;
            }
        }
        // Handle enemy finish
        else if (other.CompareTag(enemyTag))
        {
            if (showDebugInfo)
            {
                Debug.Log($"FinishLineTrigger: Enemy '{other.name}' crossed the finish line!");
            }
            
            FinishLineController.Instance.OnEnemyFinish();
            
            if (triggerOnce)
            {
                hasTriggered = true;
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"FinishLineTrigger: Unknown object '{other.name}' with tag '{other.tag}' crossed the finish line");
            }
        }
    }
    
    // Public method to reset the trigger
    public void ResetTrigger()
    {
        hasTriggered = false;
        
        if (showDebugInfo)
        {
            Debug.Log("FinishLineTrigger: Trigger reset");
        }
    }
    
    // Method to check if trigger has been activated
    public bool HasTriggered()
    {
        return hasTriggered;
    }
    
    void OnDrawGizmos()
    {
        // Draw finish line visualization
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            Gizmos.color = hasTriggered ? Color.red : Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (triggerCollider is BoxCollider boxCollider)
            {
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }
            else if (triggerCollider is SphereCollider sphereCollider)
            {
                Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
            }
            else if (triggerCollider is CapsuleCollider capsuleCollider)
            {
                // Draw capsule as cylinder
                Gizmos.DrawWireCube(capsuleCollider.center, new Vector3(capsuleCollider.radius * 2, capsuleCollider.height, capsuleCollider.radius * 2));
            }
        }
        
        // Draw finish line label
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw more detailed visualization when selected
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (triggerCollider is BoxCollider boxCollider)
            {
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            }
            else if (triggerCollider is SphereCollider sphereCollider)
            {
                Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
            }
        }
    }
} 