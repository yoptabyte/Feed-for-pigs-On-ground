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
        float speedMultiplier = 1.0f;
        float accelerationMultiplier = 1.0f;
        float slipperiness = 0.0f;

        if (groundCheckData.isGrounded && groundCheckData.CurrentSurfaceData != null)
        {
            speedMultiplier = groundCheckData.CurrentSurfaceData.SpeedMultiplier;
            accelerationMultiplier = groundCheckData.CurrentSurfaceData.AccelerationMultiplier;
            slipperiness = groundCheckData.CurrentSurfaceData.Slipperiness;
        }

        float baseMaxSpeed = movementData.moveSpeed * 1.5f;
        movementData.maxSpeed = baseMaxSpeed;
        
        float currentMaxSpeed = movementData.maxSpeed * speedMultiplier;
        float currentAcceleration = movementData.acceleration * accelerationMultiplier;
        float currentDrag = movementData.drag * (1.0f - slipperiness);
        float currentBrakingDrag = movementData.drag * movementData.brakingDragMultiplier * (1.0f - slipperiness);

        currentDrag = Mathf.Max(currentDrag * 0.5f, 0.01f);
        currentBrakingDrag = Mathf.Max(currentBrakingDrag * 0.7f, 0.05f);

        float turnInput = inputData.moveInput.x;
        float turnAmount = turnInput * movementData.rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        float moveInputZ = inputData.moveInput.z;
        Vector3 inputDirection = transform.forward * moveInputZ;
        float currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        bool hasMoveInput = Mathf.Abs(moveInputZ) > 0.01f;

        if (groundCheckData.isGrounded)
        {
            if (hasMoveInput)
            {
                float targetSpeed = currentMaxSpeed * Mathf.Sign(moveInputZ);
                float speedDifference = targetSpeed - currentSpeed;
                Vector3 accelerationForce = transform.forward * speedDifference * currentAcceleration * rb.mass;

                Vector3 potentialVelocity = rb.linearVelocity + (accelerationForce / rb.mass * Time.fixedDeltaTime);
                float projectedVelocity = Vector3.Dot(potentialVelocity, transform.forward);
                if ((moveInputZ > 0 && projectedVelocity > currentMaxSpeed) ||
                    (moveInputZ < 0 && projectedVelocity < -currentMaxSpeed))
                {
                    float requiredAccelMagnitude = (targetSpeed - currentSpeed) / Time.fixedDeltaTime;
                    accelerationForce = transform.forward * requiredAccelMagnitude * rb.mass;
                }

                rb.AddForce(accelerationForce, ForceMode.Force);
            }

            Vector3 dragForce = -rb.linearVelocity.normalized * currentDrag * rb.mass;
            dragForce.y = 0; 

            if (!hasMoveInput)
            {
                if (rb.linearVelocity.magnitude > 0.01f)
                {
                   Vector3 brakingForce = -rb.linearVelocity.normalized * currentBrakingDrag * rb.mass;
                   brakingForce.y = 0;
                   rb.AddForce(brakingForce, ForceMode.Force);
                }
            }
            else if (Mathf.Abs(currentSpeed) > currentMaxSpeed)
            {
                rb.AddForce(dragForce, ForceMode.Force);
            }

            if (Mathf.Abs(turnInput) > 0.01f)
            {
                float turnDragFactor = movementData.turnDrag * (1.0f - slipperiness * 0.8f);
                Vector3 turnDragForce = -rb.linearVelocity.normalized * turnDragFactor * rb.linearVelocity.magnitude * Mathf.Abs(turnInput) * rb.mass;
                turnDragForce.y = 0;
                rb.AddForce(turnDragForce, ForceMode.Force);
            }
        }

        if (inputData.jumpInput && groundCheckData.isGrounded)
        {
            float jumpMultiplier = 1.0f;

            if (groundCheckData.CurrentSurfaceData != null)
            {
                jumpMultiplier = groundCheckData.CurrentSurfaceData.JumpMultiplier;
            }

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            
            Vector3 jumpForceVector = CalculateJumpForce(movementData.jumpForce * jumpMultiplier);
            rb.AddForce(jumpForceVector, ForceMode.Impulse);
        }

        inputData.jumpInput = false;
    }
    
    /*
    public static Vector3 CalculateMovementVelocity(Vector3 moveInput, float speed, float currentYVelocity)
    {
        Vector3 horizontalInput = new Vector3(moveInput.x, 0, moveInput.z);
        Vector3 desiredVelocity = horizontalInput.normalized * speed;
        desiredVelocity.y = currentYVelocity;
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