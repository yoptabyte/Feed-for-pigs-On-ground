using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    private Animator animator;
    private CharacterController characterController;
    private Vector3 moveDirection;
    
    void Start()
    {
        // Get components
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        
        // Check if animator exists
        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
    }
    
    void Update()
    {
        HandleMovement();
        UpdateAnimations();
    }
    
    void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Calculate movement direction
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        
        // Move character
        if (characterController != null)
        {
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
            
            // Apply gravity
            characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
        else
        {
            // If no CharacterController, use transform
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        }
        
        // Rotate character to face movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Check if player is moving
        bool isMoving = moveDirection.magnitude > 0.1f;
        
        // Set animation parameter
        animator.SetBool("isWalkeng", isMoving);
    }
} 