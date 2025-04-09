using UnityEngine;

public class GroundCheckData : MonoBehaviour
{
    [Header("State (Read Only)")]
    public bool isGrounded;
    public SurfaceType CurrentSurfaceType = SurfaceType.Ground;
    public SurfaceData CurrentSurfaceData = null;

    [Header("Settings")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayerMask = 1;
    public Transform groundCheckPoint;
} 