using UnityEngine;

public class GroundCheckData : MonoBehaviour
{
    [Header("State (Read Only)")]
    public bool isGrounded;

    [Header("Settings")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayerMask = 1;
    public Transform groundCheckPoint;
} 