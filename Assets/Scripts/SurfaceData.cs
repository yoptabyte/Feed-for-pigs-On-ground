using UnityEngine;

public class SurfaceData : MonoBehaviour
{
    public SurfaceType Type = SurfaceType.Ground;

    [Header("Surface Properties")]
    [Range(0.1f, 3.0f)]
    public float SpeedMultiplier = 1.0f;
    [Range(0.1f, 3.0f)]
    public float AccelerationMultiplier = 1.0f;
    [Range(0.0f, 1.0f)]
    public float Slipperiness = 0.0f;
    [Range(0.1f, 2.0f)]
    public float JumpMultiplier = 1.0f;
} 