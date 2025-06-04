using UnityEngine;

public class SpeedTracker : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Показывать отладочную информацию")]
    public bool showDebugInfo = false;
    
    [Tooltip("Интервал обновления скорости в секундах")]
    public float updateInterval = 0.1f;
    
    [Tooltip("Минимальная скорость для записи (игнорировать очень малые значения)")]
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
            Debug.LogError($"SpeedTracker: Rigidbody не найден на {gameObject.name} или его родителях/детях!");
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"SpeedTracker: Найден Rigidbody на {rb.gameObject.name} для отслеживания скорости");
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
                        Debug.Log($"SpeedTracker [{gameObject.name}]: Скорость {currentSpeed:F1}, макс {statsTracker.MaxSpeedReached:F1}");
                    }
                    
                    lastRecordedSpeed = currentSpeed;
                }
                else
                {
                    if (showDebugInfo)
                    {
                        Debug.LogWarning("SpeedTracker: GameStatsTracker Instance не найден!");
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
            Debug.Log($"SpeedTracker: Тестовая скорость {testSpeed:F1} отправлена в StatTracker");
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
                Debug.Log($"SpeedTracker: Принудительная запись скорости {currentSpeed:F1}");
            }
        }
    }
    
    // Show current speed info
    [ContextMenu("Show Speed Info")]
    public void ShowSpeedInfo()
    {
        if (rb != null)
        {
            Debug.Log($"SpeedTracker [{gameObject.name}]: Текущая скорость {rb.linearVelocity.magnitude:F1}");
            
            GameStatsTracker statsTracker = GameStatsTracker.Instance;
            if (statsTracker != null)
            {
                Debug.Log($"SpeedTracker: Максимальная записанная скорость {statsTracker.MaxSpeedReached:F1}");
            }
        }
        else
        {
            Debug.LogError("SpeedTracker: Rigidbody не найден!");
        }
    }
} 