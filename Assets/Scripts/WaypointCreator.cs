using UnityEngine;

public class WaypointCreator : MonoBehaviour
{
    [Header("Waypoint Creation")]
    public GameObject waypointPrefab;
    public int numberOfWaypoints = 4;
    public float radius = 5f;
    public bool createInCircle = true;
    
    [Header("Jump Waypoints")]
    public bool enableJumpWaypoints = true;
    [Range(0f, 1f)]
    public float jumpWaypointChance = 0.3f; // 30% chance for each waypoint to be a jump point
    public float jumpForceMultiplier = 1.2f;
    
    [Header("Manual Waypoint Setup")]
    public Transform[] manualWaypoints;
    
    [Space]
    [Header("Actions")]
    public bool createWaypoints = false;
    public bool clearWaypoints = false;
    public bool convertToJumpWaypoints = false; // Convert selected waypoints to jump points
    
    private Transform[] createdWaypoints;
    
    void Update()
    {
        // Handle editor buttons
        if (createWaypoints)
        {
            createWaypoints = false;
            CreateWaypoints();
        }
        
        if (clearWaypoints)
        {
            clearWaypoints = false;
            ClearWaypoints();
        }
        
        if (convertToJumpWaypoints)
        {
            convertToJumpWaypoints = false;
            ConvertRandomWaypointsToJumpPoints();
        }
    }
    
    public void CreateWaypoints()
    {
        ClearWaypoints(); // Clear existing waypoints first
        
        createdWaypoints = new Transform[numberOfWaypoints];
        
        for (int i = 0; i < numberOfWaypoints; i++)
        {
            Vector3 position;
            
            if (createInCircle)
            {
                // Create waypoints in a circle
                float angle = (360f / numberOfWaypoints) * i * Mathf.Deg2Rad;
                position = transform.position + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );
            }
            else
            {
                // Create waypoints in a line
                position = transform.position + (Vector3.forward * radius * i / numberOfWaypoints);
            }
            
            GameObject waypoint;
            if (waypointPrefab != null)
            {
                waypoint = Instantiate(waypointPrefab, position, Quaternion.identity, transform);
            }
            else
            {
                // Create simple waypoint
                waypoint = new GameObject($"Waypoint_{i}");
                waypoint.transform.position = position;
                waypoint.transform.parent = transform;
            }
            
            // Randomly make some waypoints jump points
            if (enableJumpWaypoints && Random.value < jumpWaypointChance)
            {
                JumpWaypoint jumpComponent = waypoint.AddComponent<JumpWaypoint>();
                jumpComponent.jumpForceMultiplier = jumpForceMultiplier;
                waypoint.name += "_Jump";
            }
            
            createdWaypoints[i] = waypoint.transform;
        }
        
        // Assign waypoints to WaypointSystem if present
        WaypointSystem waypointSystem = GetComponent<WaypointSystem>();
        if (waypointSystem != null)
        {
            waypointSystem.waypoints = createdWaypoints;
        }
        
        Debug.Log($"Created {numberOfWaypoints} waypoints with {CountJumpWaypoints()} jump points");
    }
    
    public void ConvertRandomWaypointsToJumpPoints()
    {
        if (createdWaypoints == null || createdWaypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints to convert. Create waypoints first.");
            return;
        }
        
        int convertedCount = 0;
        
        for (int i = 0; i < createdWaypoints.Length; i++)
        {
            if (createdWaypoints[i] != null && Random.value < jumpWaypointChance)
            {
                // Check if it doesn't already have JumpWaypoint component
                if (createdWaypoints[i].GetComponent<JumpWaypoint>() == null)
                {
                    JumpWaypoint jumpComponent = createdWaypoints[i].gameObject.AddComponent<JumpWaypoint>();
                    jumpComponent.jumpForceMultiplier = jumpForceMultiplier;
                    
                    if (!createdWaypoints[i].name.Contains("_Jump"))
                    {
                        createdWaypoints[i].name += "_Jump";
                    }
                    
                    convertedCount++;
                }
            }
        }
        
        Debug.Log($"Converted {convertedCount} waypoints to jump points");
    }
    
    public void ClearWaypoints()
    {
        // Clear waypoints created by this script
        if (createdWaypoints != null)
        {
            for (int i = 0; i < createdWaypoints.Length; i++)
            {
                if (createdWaypoints[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(createdWaypoints[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(createdWaypoints[i].gameObject);
                    }
                }
            }
        }
        
        // Clear all child waypoint objects
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Waypoint_"))
            {
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        
        createdWaypoints = null;
        Debug.Log("Cleared waypoints");
    }
    
    public void UseManualWaypoints()
    {
        if (manualWaypoints != null && manualWaypoints.Length > 0)
        {
            WaypointSystem waypointSystem = GetComponent<WaypointSystem>();
            if (waypointSystem != null)
            {
                waypointSystem.waypoints = manualWaypoints;
                Debug.Log($"Assigned {manualWaypoints.Length} manual waypoints");
            }
        }
    }
    
    private int CountJumpWaypoints()
    {
        int count = 0;
        if (createdWaypoints != null)
        {
            for (int i = 0; i < createdWaypoints.Length; i++)
            {
                if (createdWaypoints[i] != null && createdWaypoints[i].GetComponent<JumpWaypoint>() != null)
                {
                    count++;
                }
            }
        }
        return count;
    }
    
    void OnDrawGizmos()
    {
        // Preview waypoint positions
        if (!Application.isPlaying)
        {
            for (int i = 0; i < numberOfWaypoints; i++)
            {
                Vector3 position;
                
                if (createInCircle)
                {
                    float angle = (360f / numberOfWaypoints) * i * Mathf.Deg2Rad;
                    position = transform.position + new Vector3(
                        Mathf.Cos(angle) * radius,
                        0f,
                        Mathf.Sin(angle) * radius
                    );
                }
                else
                {
                    position = transform.position + (Vector3.forward * radius * i / numberOfWaypoints);
                }
                
                // Color based on whether it would be a jump waypoint (approximation)
                bool wouldBeJump = enableJumpWaypoints && (i % Mathf.Max(1, Mathf.RoundToInt(1f / jumpWaypointChance)) == 0);
                Gizmos.color = wouldBeJump ? Color.yellow : Color.cyan;
                Gizmos.DrawWireSphere(position, 0.5f);
                
                // Draw connections
                if (i > 0)
                {
                    Vector3 prevPosition;
                    if (createInCircle)
                    {
                        float prevAngle = (360f / numberOfWaypoints) * (i - 1) * Mathf.Deg2Rad;
                        prevPosition = transform.position + new Vector3(
                            Mathf.Cos(prevAngle) * radius,
                            0f,
                            Mathf.Sin(prevAngle) * radius
                        );
                    }
                    else
                    {
                        prevPosition = transform.position + (Vector3.forward * radius * (i - 1) / numberOfWaypoints);
                    }
                    
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(prevPosition, position);
                }
                
                // Draw closing connection for circle
                if (createInCircle && i == numberOfWaypoints - 1)
                {
                    Vector3 firstPosition = transform.position + new Vector3(
                        Mathf.Cos(0) * radius,
                        0f,
                        Mathf.Sin(0) * radius
                    );
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(position, firstPosition);
                }
            }
        }
    }
} 