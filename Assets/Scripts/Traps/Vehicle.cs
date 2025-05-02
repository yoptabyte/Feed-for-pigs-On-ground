using UnityEngine;

public class VehicleMovement : MonoBehaviour
{
    [Header("Wheels")]
    [Tooltip("Front left wheel transform")]
    [SerializeField] private Transform frontLeftWheel;

    [Tooltip("Front right wheel transform")]
    [SerializeField] private Transform frontRightWheel;

    [Tooltip("Rear left wheel transform")]
    [SerializeField] private Transform rearLeftWheel;

    [Tooltip("Rear right wheel transform")]
    [SerializeField] private Transform rearRightWheel;

    [Header("Rotation Settings")]
    [Tooltip("Wheel rotation speed (degrees per second)")]
    [SerializeField] private float rotationSpeed = 360f;

    [Tooltip("Rotate wheels forward (positive speed)? If unchecked, rotates backward.")]
    [SerializeField] private bool rotateForward = true;

    [Header("Movement Settings")]
    [Tooltip("Waypoints for the vehicle path")]
    [SerializeField] private Transform[] waypoints;

    [Tooltip("Movement speed (units per second)")]
    [SerializeField] private float moveSpeed = 5f;

    private int currentWaypointIndex = 0;

    void Update()
    {
        HandleMovement();
        RotateWheels();
    }

    private void HandleMovement()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }

    private void RotateWheels()
    {
        float directionMultiplier = rotateForward ? 1f : -1f;
        float rotationAngle = rotationSpeed * directionMultiplier * Time.deltaTime;

        RotateWheel(frontLeftWheel, rotationAngle);
        RotateWheel(frontRightWheel, rotationAngle);
        RotateWheel(rearLeftWheel, rotationAngle);
        RotateWheel(rearRightWheel, rotationAngle);
    }

    private void RotateWheel(Transform wheel, float angle)
    {
        if (wheel != null)
        {
            wheel.Rotate(Vector3.right, angle, Space.Self);
        }
    }
}