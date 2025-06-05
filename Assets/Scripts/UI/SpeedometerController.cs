using UnityEngine;
using UnityEngine.UI;

public class SpeedometerController : MonoBehaviour
{
    [Header("Speedometer Settings")]
    [Tooltip("Speedometer needle (object to rotate)")]
    public Transform speedometerNeedle;
    
    [Tooltip("Player or object with MovementSystem")]
    public Transform player;
    
    [Header("Rotation Settings")]
    [Tooltip("Minimum needle rotation angle (in degrees)")]
    public float minAngle = -90f;
    
    [Tooltip("Maximum needle rotation angle (in degrees)")]
    public float maxAngle = 90f;
    
    [Tooltip("Maximum speed to display")]
    public float maxSpeed = 100f;
    
    [Header("Pivot Settings")]
    [Tooltip("Rotation center offset on X axis (in pixels)")]
    public float pivotOffsetX = 0f;
    
    [Tooltip("Rotation center offset on Y axis (in pixels)")]
    public float pivotOffsetY = -50f;
    
    [Tooltip("Automatically setup pivot on start")]
    public bool autoSetPivot = true;
    
    [Header("Debug")]
    [Tooltip("Show debug information")]
    public bool showDebugInfo = false;
    
    // Private variables
    private Rigidbody playerRigidbody;
    private MovementData playerMovementData;
    private RectTransform needleRectTransform;
    private Image needleImage;
    private float currentSpeed;
    private Vector2 originalPivot;
    
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
            speedometerNeedle = transform.Find("speed");
            if (speedometerNeedle == null)
            {
                Debug.LogError("SpeedometerController: Speedometer needle not found! Assign it to the 'speedometerNeedle' field");
                return;
            }
        }
        
        // Get needle components
        needleRectTransform = speedometerNeedle.GetComponent<RectTransform>();
        needleImage = speedometerNeedle.GetComponent<Image>();
        
        if (needleRectTransform != null)
        {
            originalPivot = needleRectTransform.pivot;
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
                    Debug.Log($"SpeedometerController: Found object with MovementData: {player.name}");
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
                Debug.LogWarning("SpeedometerController: Rigidbody not found on player");
            }
            
            if (playerMovementData == null)
            {
                Debug.LogWarning("SpeedometerController: MovementData not found on player");
            }
        }
        else
        {
            Debug.LogError("SpeedometerController: Player not found! Assign it to the 'player' field");
        }
    }
    
    private void SetupPivot()
    {
        if (!autoSetPivot || needleRectTransform == null) return;
        
        // Calculate new pivot based on offset
        Vector2 sizeDelta = needleRectTransform.sizeDelta;
        Vector2 newPivot = new Vector2(
            0.5f + (pivotOffsetX / sizeDelta.x),
            0.5f + (pivotOffsetY / sizeDelta.y)
        );
        
        // Clamp pivot values to valid range
        newPivot.x = Mathf.Clamp01(newPivot.x);
        newPivot.y = Mathf.Clamp01(newPivot.y);
        
        needleRectTransform.pivot = newPivot;
        
        Debug.Log($"SpeedometerController: Pivot changed from {originalPivot} to {newPivot}");
    }
    
    private void UpdateSpeed()
    {
        if (playerRigidbody == null) return;
        
        // Get current speed from rigidbody velocity
        currentSpeed = playerRigidbody.linearVelocity.magnitude;
        
        // Alternative: get speed from forward direction only
        // currentSpeed = Vector3.Dot(playerRigidbody.linearVelocity, player.forward);
        // currentSpeed = Mathf.Abs(currentSpeed); // Make positive
    }
    
    private void UpdateNeedleRotation()
    {
        if (needleRectTransform == null) return;
        
        // Calculate rotation angle based on speed
        float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);
        float targetAngle = Mathf.Lerp(minAngle, maxAngle, speedRatio);
        
        // Apply rotation
        needleRectTransform.localRotation = Quaternion.Euler(0, 0, targetAngle);
    }
    
    private void DebugInfo()
    {
        if (player != null && playerRigidbody != null)
        {
            Debug.Log($"Speedometer - Speed: {currentSpeed:F1}, Angle: {needleRectTransform.localRotation.eulerAngles.z:F1}");
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
        SetupPivot();
    }
    
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    // Context menu methods for testing
    [ContextMenu("Reset Pivot to Center")]
    public void ResetPivotToCenter()
    {
        if (needleRectTransform != null)
        {
            needleRectTransform.pivot = new Vector2(0.5f, 0.5f);
            Debug.Log("Pivot reset to center (0.5, 0.5)");
        }
    }
    
    [ContextMenu("Apply Custom Pivot")]
    public void ApplyCustomPivot()
    {
        SetupPivot();
    }
    
    [ContextMenu("Test Speedometer Animation")]
    public void TestSpeedometerAnimation()
    {
        StartCoroutine(TestAnimation());
    }
    
    private System.Collections.IEnumerator TestAnimation()
    {
        Debug.Log("Testing speedometer animation...");
        
        for (int i = 0; i <= 100; i += 10)
        {
            currentSpeed = (i / 100f) * maxSpeed;
            UpdateNeedleRotation();
            yield return new UnityEngine.WaitForSeconds(0.2f);
        }
        
        for (int i = 100; i >= 0; i -= 10)
        {
            currentSpeed = (i / 100f) * maxSpeed;
            UpdateNeedleRotation();
            yield return new UnityEngine.WaitForSeconds(0.2f);
        }
        
        Debug.Log("Speedometer animation test complete!");
    }
} 