using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(InputData))]
[RequireComponent(typeof(MovementData))]
[RequireComponent(typeof(GroundCheckData))]
[RequireComponent(typeof(Animator))]
public class MovementSystem : MonoBehaviour
{
    [Header("Movement Audio")]
    [SerializeField]
    private AudioClip walkingSound;
    [SerializeField]
    private float walkingVolume = 0.5f;
    [SerializeField]
    private float minSpeedForWalking = 0.5f;
    
    [Header("Animation")]
    public float movementThreshold = 0.1f; // Minimum speed to consider as walking
    
    private Rigidbody rb;
    private InputData inputData;
    private MovementData movementData;
    private GroundCheckData groundCheckData;
    private Animator animator;
    
    // Audio components
    private AudioSource audioSource;
    private bool isWalkingAudioPlaying = false;
    
    // Animation state
    private bool isWalking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputData = GetComponent<InputData>();
        movementData = GetComponent<MovementData>();
        groundCheckData = GetComponent<GroundCheckData>();
        animator = GetComponent<Animator>();

        rb.freezeRotation = true;
        
        // Setup audio for walking
        SetupWalkingAudio();
    }
    
    private void SetupWalkingAudio()
    {
        // Get existing AudioSources on the object
        AudioSource[] existingAudioSources = GetComponents<AudioSource>();
        
        // Try to find an AudioSource that's not being used by Health component
        audioSource = null;
        foreach (AudioSource source in existingAudioSources)
        {
            // Check if this AudioSource is used by Health component
            Health healthComponent = GetComponent<Health>();
            if (healthComponent != null && healthComponent.deathAudioSource == source)
            {
                continue; // Skip this one, it's used by Health
            }
            
            // Check if this AudioSource has a death-related clip
            if (source.clip != null && (source.clip.name.Contains("die") || source.clip.name.Contains("death")))
            {
                continue; // Skip this one, it's probably for death sound
            }
            
            // This AudioSource seems available for walking
            audioSource = source;
            break;
        }
        
        // If no suitable AudioSource found, create a dedicated one for walking
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("🚶 MovementSystem: Created dedicated AudioSource for walking sound");
        }
        
        // Try to load walk.mp3 if not assigned
        if (walkingSound == null)
        {
            // Try to find walk.mp3 in Resources folder
            walkingSound = Resources.Load<AudioClip>("walk");
        }
        
        // Configure AudioSource for walking
        if (walkingSound != null)
        {
            audioSource.clip = walkingSound;
            audioSource.volume = walkingVolume;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound for player
            audioSource.priority = 128; // Normal priority for walking sounds
            Debug.Log("🚶 MovementSystem: Walking sound loaded successfully");
        }
        else
        {
            Debug.LogWarning("🚶 MovementSystem: Walking sound not found! Please assign walk.mp3 manually in inspector or place it in Resources folder.");
        }
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
        
        // Handle walking audio
        HandleWalkingAudio(hasMoveInput);
        
        // Handle animation
        UpdateAnimation(hasMoveInput);
    }
    
    private void UpdateAnimation(bool hasMoveInput)
    {
        if (animator == null) return;
        
        // Calculate current movement speed
        float currentSpeed = rb.linearVelocity.magnitude;
        
        // Determine if walking based on both input and actual speed
        bool shouldWalk = hasMoveInput && groundCheckData.isGrounded && currentSpeed > movementThreshold;
        
        // Update animation only if state changed
        if (shouldWalk != isWalking)
        {
            isWalking = shouldWalk;
            animator.SetBool("isWalking", isWalking);
            
            Debug.Log($"🚶 Player animation state changed: {(isWalking ? "Walking" : "Idle")} (Speed: {currentSpeed:F2}, Input: {hasMoveInput}, Grounded: {groundCheckData.isGrounded})");
        }
    }
    
    private void HandleWalkingAudio(bool hasMoveInput)
    {
        if (audioSource == null || walkingSound == null) return;
        
        // Check if player is alive
        Health healthComponent = GetComponent<Health>();
        if (healthComponent != null && !healthComponent.IsAlive)
        {
            // Stop walking sound if player is dead
            if (isWalkingAudioPlaying)
            {
                audioSource.Stop();
                isWalkingAudioPlaying = false;
                Debug.Log("🚶 MovementSystem: Stopped walking sound - player died");
            }
            return;
        }
        
        // Check if player is moving and grounded
        float currentSpeed = rb.linearVelocity.magnitude;
        bool shouldPlayWalkingSound = hasMoveInput && groundCheckData.isGrounded && currentSpeed > minSpeedForWalking;
        
        if (shouldPlayWalkingSound && !isWalkingAudioPlaying)
        {
            audioSource.Play();
            isWalkingAudioPlaying = true;
            Debug.Log("🚶 MovementSystem: Started walking sound");
        }
        else if (!shouldPlayWalkingSound && isWalkingAudioPlaying)
        {
            audioSource.Stop();
            isWalkingAudioPlaying = false;
            Debug.Log("🚶 MovementSystem: Stopped walking sound");
        }
        
        // Adjust pitch based on speed
        if (isWalkingAudioPlaying)
        {
            float speedRatio = Mathf.Clamp01(currentSpeed / movementData.maxSpeed);
            audioSource.pitch = 0.8f + (speedRatio * 0.4f); // Pitch range from 0.8 to 1.2
        }
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