using UnityEngine;

// Component to mark a waypoint as a jump point
public class JumpWaypoint : MonoBehaviour
{
    [Header("Jump Settings")]
    public bool isJumpPoint = true;
    public float jumpForceMultiplier = 1.0f; // Multiplier for jump force
    
    void OnDrawGizmos()
    {
        if (isJumpPoint)
        {
            // Draw jump indicator
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
            
            // Draw upward arrow to indicate jump
            Vector3 arrowStart = transform.position;
            Vector3 arrowEnd = arrowStart + Vector3.up * 2f;
            Gizmos.DrawLine(arrowStart, arrowEnd);
            
            // Draw arrow head
            Vector3 arrowTip = arrowEnd;
            Gizmos.DrawLine(arrowTip, arrowTip + Vector3.left * 0.3f + Vector3.down * 0.3f);
            Gizmos.DrawLine(arrowTip, arrowTip + Vector3.right * 0.3f + Vector3.down * 0.3f);
        }
    }
}

public class WaypointSystem : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public Transform[] waypoints;
    public bool loopWaypoints = true;
    public float waypointReachedDistance = 2f;
    
    private int currentWaypointIndex = 0;
    
    void Start()
    {
        // Validate waypoints array
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("WaypointSystem: No waypoints assigned!");
        }
    }
    
    public Transform GetCurrentWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
            return null;
            
        return waypoints[currentWaypointIndex];
    }
    
    public Transform GetNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
            return null;
            
        currentWaypointIndex++;
        
        if (currentWaypointIndex >= waypoints.Length)
        {
            if (loopWaypoints)
            {
                currentWaypointIndex = 0;
            }
            else
            {
                currentWaypointIndex = waypoints.Length - 1;
            }
        }
        
        return waypoints[currentWaypointIndex];
    }
    
    public bool HasReachedCurrentWaypoint(Vector3 currentPosition)
    {
        Transform currentWaypoint = GetCurrentWaypoint();
        if (currentWaypoint == null)
            return false;
            
        float distance = Vector3.Distance(currentPosition, currentWaypoint.position);
        return distance <= waypointReachedDistance;
    }
    
    public Vector3 GetDirectionToCurrentWaypoint(Vector3 currentPosition)
    {
        Transform currentWaypoint = GetCurrentWaypoint();
        if (currentWaypoint == null)
            return Vector3.zero;
            
        Vector3 direction = (currentWaypoint.position - currentPosition).normalized;
        direction.y = 0; // Keep movement on horizontal plane
        return direction;
    }
    
    // Check if current waypoint is a jump point
    public bool IsCurrentWaypointJumpPoint()
    {
        Transform currentWaypoint = GetCurrentWaypoint();
        if (currentWaypoint == null)
            return false;
            
        JumpWaypoint jumpComponent = currentWaypoint.GetComponent<JumpWaypoint>();
        return jumpComponent != null && jumpComponent.isJumpPoint;
    }
    
    // Get jump force multiplier for current waypoint
    public float GetCurrentWaypointJumpMultiplier()
    {
        Transform currentWaypoint = GetCurrentWaypoint();
        if (currentWaypoint == null)
            return 1.0f;
            
        JumpWaypoint jumpComponent = currentWaypoint.GetComponent<JumpWaypoint>();
        return jumpComponent != null ? jumpComponent.jumpForceMultiplier : 1.0f;
    }
    
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;
            
        // Draw waypoint connections
        Gizmos.color = Color.blue;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
        
        // Draw loop connection if enabled
        if (loopWaypoints && waypoints[waypoints.Length - 1] != null && waypoints[0] != null)
        {
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
        
        // Draw waypoint spheres
        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                // Check if this is a jump waypoint
                JumpWaypoint jumpComponent = waypoints[i].GetComponent<JumpWaypoint>();
                if (jumpComponent != null && jumpComponent.isJumpPoint)
                {
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.color = Color.red;
                }
                
                Gizmos.DrawWireSphere(waypoints[i].position, waypointReachedDistance);
            }
        }
        
        // Highlight current waypoint
        if (Application.isPlaying && GetCurrentWaypoint() != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GetCurrentWaypoint().position, waypointReachedDistance + 0.5f);
        }
    }
}