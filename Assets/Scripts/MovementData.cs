using UnityEngine;

public enum SurfaceType
{
    Ground,
    Mud,
    Ice,
    Sand,
    MetalGrate
}

public class MovementData : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float maxSpeed = 7f;
    public float acceleration = 10f;
    public float drag = 5f;
    public float brakingDragMultiplier = 10f;
    public float turnDrag = 2f;
    public float jumpForce = 8f;
    public float rotationSpeed = 20f;
}