using UnityEngine;

public class WheelRotator : MonoBehaviour
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

    void Update()
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