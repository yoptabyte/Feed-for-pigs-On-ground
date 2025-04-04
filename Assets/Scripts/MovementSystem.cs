using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(InputData))]
[RequireComponent(typeof(MovementData))]
[RequireComponent(typeof(GroundCheckData))]
public class MovementSystem : MonoBehaviour
{
    private Rigidbody rb;
    private InputData inputData;
    private MovementData movementData;
    private GroundCheckData groundCheckData;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputData = GetComponent<InputData>();
        movementData = GetComponent<MovementData>();
        groundCheckData = GetComponent<GroundCheckData>();

        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        Vector3 currentVelocity = rb.linearVelocity;

        Vector3 targetHorizontalVelocity;

        if (groundCheckData.isGrounded)
        {
            Vector3 horizontalInput = new Vector3(inputData.moveInput.x, 0, inputData.moveInput.z);
            targetHorizontalVelocity = horizontalInput.normalized * movementData.moveSpeed;
        }
        else
        {

            targetHorizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        }

        rb.linearVelocity = new Vector3(targetHorizontalVelocity.x, currentVelocity.y, targetHorizontalVelocity.z);
        
        // Vector3 force = CalculateMovementForce(...);
        // rb.AddForce(force);
        
        if (inputData.jumpInput && groundCheckData.isGrounded)
        {
            Vector3 jumpForceVector = CalculateJumpForce(movementData.jumpForce);
            rb.AddForce(jumpForceVector, ForceMode.Impulse);
        }

        inputData.jumpInput = false;
    }
    
    /*
    public static Vector3 CalculateMovementVelocity(Vector3 moveInput, float speed, float currentYVelocity)
    {
        Vector3 horizontalInput = new Vector3(moveInput.x, 0, moveInput.z);
        Vector3 desiredVelocity = horizontalInput.normalized * speed;
        desiredVelocity.y = currentYVelocity; // Сохраняем Y скорость (гравитация обработается физикой)
        return desiredVelocity;
    }
    */

    public static Vector3 CalculateJumpForce(float jumpForce)
    {
        return Vector3.up * jumpForce;
    }

    // public static Vector3 CalculateMovementForce(Vector3 moveInput, float speed, Vector3 currentVelocity, float maxSpeed)
    // {
    //     // ...
    //     return force;
    // }
} 