using UnityEngine;

public class SpriteSpeedometerController : MonoBehaviour
{
    [Header("Speedometer Settings")]
    [Tooltip("Стрелка спидометра (SpriteRenderer)")]
    public Transform speedometerNeedle;
    
    [Tooltip("Игрок или объект с MovementSystem")]
    public Transform player;
    
    [Header("Rotation Settings")]
    [Tooltip("Минимальный угол поворота стрелки (в градусах)")]
    public float minAngle = -90f;
    
    [Tooltip("Максимальный угол поворота стрелки (в градусах)")]
    public float maxAngle = 90f;
    
    [Tooltip("Максимальная скорость для отображения")]
    public float maxSpeed = 100f;
    
    [Header("Pivot Settings")]
    [Tooltip("Смещение центра вращения по X (в юнитах)")]
    public float pivotOffsetX = 0f;
    
    [Tooltip("Смещение центра вращения по Y (в юнитах)")]
    public float pivotOffsetY = -0.5f;
    
    [Tooltip("Создать дочерний объект для правильного поворота")]
    public bool createPivotChild = true;
    
    [Header("Smooth Animation")]
    [Tooltip("Плавность анимации стрелки")]
    public float smoothSpeed = 5f;
    
    [Header("Debug")]
    [Tooltip("Показывать отладочную информацию")]
    public bool showDebugInfo = false;
    
    // Private variables
    private Rigidbody playerRigidbody;
    private MovementData playerMovementData;
    private Transform pivotTransform;
    private SpriteRenderer needleSpriteRenderer;
    private float currentSpeed;
    private float targetAngle;
    private float currentAngle;
    private bool pivotSetup = false;
    
    void Start()
    {
        SetupComponents();
        SetupPivot();
    }
    
    void Update()
    {
        UpdateSpeed();
        UpdateNeedleRotation();
        
        if (showDebugInfo)
        {
            DebugInfo();
        }
    }
    
    private void SetupComponents()
    {
        // Find speedometer needle if not assigned
        if (speedometerNeedle == null)
        {
            // Try to find by name
            Transform[] allTransforms = FindObjectsOfType<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.name == "speed" && t.GetComponent<SpriteRenderer>() != null)
                {
                    speedometerNeedle = t;
                    break;
                }
            }
            
            if (speedometerNeedle == null)
            {
                Debug.LogError("SpriteSpeedometerController: Стрелка спидометра не найдена! Назначьте её в поле 'speedometerNeedle'");
                return;
            }
        }
        
        // Get needle components
        needleSpriteRenderer = speedometerNeedle.GetComponent<SpriteRenderer>();
        
        if (needleSpriteRenderer == null)
        {
            Debug.LogError("SpriteSpeedometerController: SpriteRenderer не найден на стрелке спидометра!");
            return;
        }
        
        // Find player if not assigned
        if (player == null)
        {
            // Try to find player by tag
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                // Try to find MovementData component in scene
                MovementData[] movementDatas = FindObjectsOfType<MovementData>();
                if (movementDatas.Length > 0)
                {
                    player = movementDatas[0].transform;
                    Debug.Log($"SpriteSpeedometerController: Найден объект с MovementData: {player.name}");
                }
            }
        }
        
        // Get player components
        if (player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody>();
            playerMovementData = player.GetComponent<MovementData>();
            
            if (playerRigidbody == null)
            {
                Debug.LogWarning("SpriteSpeedometerController: Rigidbody не найден на игроке");
            }
            
            if (playerMovementData == null)
            {
                Debug.LogWarning("SpriteSpeedometerController: MovementData не найден на игроке");
            }
        }
        else
        {
            Debug.LogError("SpriteSpeedometerController: Игрок не найден! Назначьте его в поле 'player'");
        }
    }
    
    private void SetupPivot()
    {
        if (speedometerNeedle == null || pivotSetup) return;
        
        if (createPivotChild)
        {
            // Create pivot child object
            GameObject pivotObj = new GameObject("SpeedometerPivot");
            pivotTransform = pivotObj.transform;
            
            // Set pivot as child of needle
            pivotTransform.SetParent(speedometerNeedle);
            pivotTransform.localPosition = new Vector3(pivotOffsetX, pivotOffsetY, 0);
            pivotTransform.localRotation = Quaternion.identity;
            
            // Move needle to be child of pivot
            Vector3 needleWorldPos = speedometerNeedle.position;
            Quaternion needleWorldRot = speedometerNeedle.rotation;
            
            speedometerNeedle.SetParent(pivotTransform);
            speedometerNeedle.position = needleWorldPos;
            speedometerNeedle.rotation = needleWorldRot;
            
            // Adjust needle position relative to pivot
            speedometerNeedle.localPosition = new Vector3(-pivotOffsetX, -pivotOffsetY, 0);
            
            Debug.Log($"SpriteSpeedometerController: Создан pivot объект с смещением ({pivotOffsetX}, {pivotOffsetY})");
        }
        else
        {
            // Use needle transform directly with offset
            pivotTransform = speedometerNeedle;
            Debug.Log("SpriteSpeedometerController: Используется прямой поворот стрелки");
        }
        
        pivotSetup = true;
    }
    
    private void UpdateSpeed()
    {
        if (playerRigidbody == null) return;
        
        // Get current speed from rigidbody velocity
        currentSpeed = playerRigidbody.linearVelocity.magnitude;
        
        // Calculate target angle based on speed
        float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
        targetAngle = Mathf.Lerp(minAngle, maxAngle, speedRatio);
    }
    
    private void UpdateNeedleRotation()
    {
        if (pivotTransform == null) return;
        
        // Smooth rotation
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, smoothSpeed * Time.deltaTime);
        
        // Apply rotation to pivot transform
        if (createPivotChild)
        {
            pivotTransform.localRotation = Quaternion.Euler(0, 0, currentAngle);
        }
        else
        {
            pivotTransform.rotation = Quaternion.Euler(0, 0, currentAngle);
        }
    }
    
    private void DebugInfo()
    {
        if (player != null && playerRigidbody != null)
        {
            Debug.Log($"Sprite Speedometer - Speed: {currentSpeed:F1}, Target Angle: {targetAngle:F1}, Current Angle: {currentAngle:F1}");
        }
    }
    
    // Public methods for external control
    public void SetMaxSpeed(float newMaxSpeed)
    {
        maxSpeed = newMaxSpeed;
    }
    
    public void SetAngleRange(float newMinAngle, float newMaxAngle)
    {
        minAngle = newMinAngle;
        maxAngle = newMaxAngle;
    }
    
    public void SetPivotOffset(float offsetX, float offsetY)
    {
        pivotOffsetX = offsetX;
        pivotOffsetY = offsetY;
        
        if (pivotSetup)
        {
            // Re-setup pivot with new offset
            pivotSetup = false;
            
            // Destroy old pivot if it exists
            if (createPivotChild && pivotTransform != null && pivotTransform.name == "SpeedometerPivot")
            {
                DestroyImmediate(pivotTransform.gameObject);
            }
            
            SetupPivot();
        }
    }
    
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    // Context menu methods for testing
    [ContextMenu("Setup Pivot")]
    public void ManualSetupPivot()
    {
        pivotSetup = false;
        SetupPivot();
    }
    
    [ContextMenu("Test Speedometer Animation")]
    public void TestSpeedometerAnimation()
    {
        StartCoroutine(TestAnimation());
    }
    
    private System.Collections.IEnumerator TestAnimation()
    {
        Debug.Log("Testing sprite speedometer animation...");
        
        float originalSmoothSpeed = smoothSpeed;
        smoothSpeed = 15f; // Faster for testing
        
        for (int i = 0; i <= 100; i += 20)
        {
            currentSpeed = (i / 100f) * maxSpeed;
            float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
            targetAngle = Mathf.Lerp(minAngle, maxAngle, speedRatio);
            yield return new UnityEngine.WaitForSeconds(0.5f);
        }
        
        for (int i = 100; i >= 0; i -= 20)
        {
            currentSpeed = (i / 100f) * maxSpeed;
            float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
            targetAngle = Mathf.Lerp(minAngle, maxAngle, speedRatio);
            yield return new UnityEngine.WaitForSeconds(0.5f);
        }
        
        smoothSpeed = originalSmoothSpeed;
        Debug.Log("Sprite speedometer animation test complete!");
    }
} 