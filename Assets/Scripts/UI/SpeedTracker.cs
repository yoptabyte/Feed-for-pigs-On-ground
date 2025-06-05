using UnityEngine;

public class SpeedTracker : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Show debug information")]
    public bool showDebugInfo = false;
    
    [Tooltip("Speed update interval in seconds")]
    public float updateInterval = 0.1f;
    
    [Tooltip("Minimum speed threshold for recording (ignore very small values)")]
    public float minimumSpeedThreshold = 0.5f;
    
    private Rigidbody rb;
    private float lastUpdateTime;
    private float lastRecordedSpeed = 0f;
    
    void Start()
    {
        // Try multiple ways to find Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = GetComponentInChildren<Rigidbody>();
        }
        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody>();
        }
        
        if (rb == null)
        {
            Debug.LogError($"SpeedTracker: Rigidbody not found on {gameObject.name} or its parents/children!");
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"SpeedTracker: Found Rigidbody on {rb.gameObject.name} for speed tracking");
            }
        }
    }
    
    void Update()
    {
        if (rb != null && Time.time - lastUpdateTime >= updateInterval)
        {
            float currentSpeed = rb.linearVelocity.magnitude;
            
            // Only track speeds above threshold
            if (currentSpeed >= minimumSpeedThreshold)
            {
                GameStatsTracker statsTracker = GameStatsTracker.Instance;
                if (statsTracker != null)
                {
                    float previousMax = statsTracker.MaxSpeedReached;
                    statsTracker.UpdateMaxSpeed(currentSpeed);
                    
                    // Debug only when speed changes significantly or when we have a new max
                    if (showDebugInfo && (Mathf.Abs(currentSpeed - lastRecordedSpeed) > 1f || currentSpeed > previousMax))
                    {
                        Debug.Log($"SpeedTracker [{gameObject.name}]: Speed {currentSpeed:F1}, max {statsTracker.MaxSpeedReached:F1}");
                    }
                    
                    lastRecordedSpeed = currentSpeed;
                }
                else
                {
                    if (showDebugInfo)
                    {
                        Debug.LogWarning("SpeedTracker: GameStatsTracker Instance not found!");
                    }
                }
            }
            
            lastUpdateTime = Time.time;
        }
    }
    
    // Method to manually test speed tracking
    [ContextMenu("Test Speed Update")]
    public void TestSpeedUpdate()
    {
        float testSpeed = Random.Range(5f, 30f);
        GameStatsTracker statsTracker = GameStatsTracker.Instance;
        if (statsTracker != null)
        {
            statsTracker.UpdateMaxSpeed(testSpeed);
            Debug.Log($"SpeedTracker: Test speed {testSpeed:F1} sent to StatTracker");
        }
    }
    
    // Force speed tracking for this frame
    [ContextMenu("Force Track Current Speed")]
    public void ForceTrackCurrentSpeed()
    {
        if (rb != null)
        {
            float currentSpeed = rb.linearVelocity.magnitude;
            GameStatsTracker statsTracker = GameStatsTracker.Instance;
            if (statsTracker != null)
            {
                statsTracker.UpdateMaxSpeed(currentSpeed);
                Debug.Log($"SpeedTracker: Forced speed recording {currentSpeed:F1}");
            }
        }
    }
    
    // Show current speed info
    [ContextMenu("Show Speed Info")]
    public void ShowSpeedInfo()
    {
        if (rb != null)
        {
            Debug.Log($"SpeedTracker [{gameObject.name}]: Current speed {rb.linearVelocity.magnitude:F1}");
            
            GameStatsTracker statsTracker = GameStatsTracker.Instance;
            if (statsTracker != null)
            {
                Debug.Log($"SpeedTracker: Maximum recorded speed {statsTracker.MaxSpeedReached:F1}");
            }
        }
        else
        {
            Debug.LogError("SpeedTracker: Rigidbody not found!");
        }
    }
} 